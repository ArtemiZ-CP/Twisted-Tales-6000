using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class HeroAttack
    {
        public enum DamageType
        {
            Physical,
            Magical
        }

        public enum AttackType
        {
            Base,
            Ability
        }

        public static bool IsAbleToAttack(Frame f, FightingHero fighingHero, Board board, out FightingHero targetHero)
        {
            if (fighingHero.AttackTarget == default || fighingHero.AttackTimer > 0)
            {
                targetHero = default;
                return false;
            }

            if (TryFindAttackTarget(f, fighingHero, board, out targetHero))
            {
                return true;
            }

            return false;
        }

        public static bool IsAbleToManaAttack(FightingHero fighingHero)
        {
            if (fighingHero.CurrentMana < fighingHero.Hero.MaxMana)
            {
                return false;
            }

            return true;
        }

        public static bool TryFindClosestTarget(Frame f, FightingHero fightingHero, Board board, out FightingHero targetHero)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargets(f, fightingHero, board);

            targetHero = HeroBoard.GetClosestTarget(f, heroesList, fightingHero);

            if (targetHero.Hero.Ref != default)
            {
                return true;
            }

            return false;
        }

        public static bool TryGetRandomTarget(Frame f, FightingHero fightingHero, Board board, out FightingHero target)
        {
            List<FightingHero> targets = HeroBoard.GetAllTargets(f, fightingHero, board);

            if (targets.Count == 0)
            {
                target = default;
                return false;
            }

            target = targets[f.RNG->Next(0, targets.Count)];
            return true;
        }

        public static bool TryFindClosestTargetInAttackRange(Frame f, FightingHero fightingHero, Board board, out FightingHero targetHero)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargetsInRange(f, fightingHero, board);

            targetHero = HeroBoard.GetClosestTarget(f, heroesList, fightingHero);

            if (targetHero.Hero.Ref != default)
            {
                return true;
            }

            return false;
        }

        public static FightingHero FindClosestTargetOutOfAttackRange(Frame f, FightingHero fightingHero, Board heroBoard, out Vector2Int moveTargetPosition, out bool inRange)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargets(f, fightingHero, heroBoard);

            if (heroesList.Count == 0)
            {
                moveTargetPosition = default;
                inRange = false;
                return default;
            }

            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fightingHero).FightingHeroesMap);

            for (int i = 0; i < heroesList.Count; i++)
            {
                FightingHero targetHero = HeroBoard.GetClosestTarget(f, heroesList, fightingHero);

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                int[,] board = new int[GameConfig.BoardSize, GameConfig.BoardSize];

                for (int x = 0; x < GameConfig.BoardSize; x++)
                {
                    for (int y = 0; y < GameConfig.BoardSize; y++)
                    {
                        if (HeroBoard.TryConvertCordsToIndex(new Vector2Int(x, y), out int index))
                        {
                            int heroID = -1;

                            if (heroes[index].IsAlive && heroes[index].Hero.Ref != default)
                            {
                                heroID = heroes[index].Hero.ID;
                            }

                            board[x, y] = heroID;
                        }
                    }
                }

                if (PathFinder.TryFindPath(board, HeroBoard.GetHeroCords(fightingHero),
                    HeroBoard.GetHeroCords(targetHero), fightingHero.Hero.Range, out moveTargetPosition, out inRange))
                {
                    return targetHero;
                }

                heroesList.Remove(targetHero);
            }

            moveTargetPosition = default;
            inRange = false;
            return default;
        }

        public static void Update(Frame f, FightingHero fightingHero, Board board)
        {
            ProcessReloadAttack(f, fightingHero, board);
            ProcessAbility(f, fightingHero, board);
            HeroEffects.ProcessEffects(f, fightingHero, board);
            Events.ChangeHeroHealth(f, fightingHero, board);
        }

        public static void InstantAttack(Frame f, FightingHero fightingHero, DamageType damageType, AttackType attackType)
        {
            InstantAttack(f, fightingHero, new HeroEffects.Effect[] { new() }, damageType, attackType);
        }

        public static void InstantAttack(Frame f, FightingHero fightingHero, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);
            
            if (IsAbleToAttack(f, fightingHero, board, out FightingHero targetHero) == false)
            {
                return;
            }

            DamageHero(f, fightingHero, board, targetHero, fightingHero.Hero.AttackDamage, effects, damageType, attackType);
            ResetAttackTimer(f, fightingHero);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, DamageType damageType, AttackType attackType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);

            if (IsAbleToAttack(f, fightingHero, board, out FightingHero targetHero) == false)
            {
                return;
            }

            ProjectileAttack(f, fightingHero, board, targetHero, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, board, targetHero, targetHero.Hero.AttackDamage, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, board, targetHero, damage, null, null, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, HeroEffects.GlobalEffect[] globalEffects, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, effects, globalEffects, damageType, attackType);
            ResetAttackTimer(f, fightingHero);
        }

        public static void ProcessReloadAttack(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHero.Index];

            FP reloadMultiplier = 1;
            QList<EffectQnt> effects = f.ResolveList(fightingHero.Effects);

            foreach (EffectQnt effect in effects)
            {
                if (effect.Index == (int)HeroEffects.EffectType.IncreaseReloadTime)
                {
                    reloadMultiplier *= effect.Value;
                }
            }

            fightingHero.AttackTimer -= f.DeltaTime * reloadMultiplier;
            fightingHero.CurrentMana += fightingHero.Hero.ManaRegen * f.DeltaTime;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ProcessAbility(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAbility.TryCastAbility(f, fightingHero, out Func<Frame, FightingHero, Board, bool> ability) == false)
            {
                return;
            }

            if (fightingHero.CurrentMana >= fightingHero.Hero.MaxMana && ability(f, fightingHero, board))
            {
                ResetMana(f, fightingHero);
            }

            f.Events.HeroManaChanged(board.Player1.Ref, board.Player2.Ref, fightingHero.Hero.Ref, fightingHero.CurrentMana, fightingHero.Hero.MaxMana);
        }

        public static void ResetAttackTimer(Frame f, FightingHero fightingHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fightingHero).FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            fightingHero.AttackTimer = 1 / fightingHero.Hero.AttackSpeed;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ResetMana(Frame f, FightingHero fightingHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fightingHero).FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            fightingHero.CurrentMana = 0;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void HealHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP amount, bool isAbleToOverHeal)
        {
            if (GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes) == false)
            {
                return;
            }

            ApplyHealToHero(f, ref targetHero, amount, isAbleToOverHeal);
            UpdateHealStats(ref fightingHero, ref targetHero, amount);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            HeroEffects.Effect[] effects = new HeroEffects.Effect[0];

            DamageHero(f, fightingHero, board, targetHero, damage, effects, damageType, attackType);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, FightingHero targetHero, FP damage, QList<EffectQnt> effectsQnt, DamageType damageType, AttackType attackType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);

            if (effectsQnt.Count == 0)
            {
                DamageHero(f, fightingHero, board, targetHero, damage, null, damageType, attackType);
                return;
            }

            HeroEffects.Effect[] effects = new HeroEffects.Effect[effectsQnt.Count];

            for (int i = 0; i < effectsQnt.Count; i++)
            {
                effects[i] = new HeroEffects.Effect(effectsQnt[i]);
            }

            DamageHero(f, fightingHero, board, targetHero, damage, effects, damageType, attackType);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            if (GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes) == false)
            {
                return;
            }

            QList<EffectQnt> heroEffects = f.ResolveList(targetHero.Effects);

            foreach (EffectQnt effectQnt in heroEffects)
            {
                if (effectQnt.Index == (int)HeroEffects.EffectType.IncreaseTakingDamage)
                {
                    damage *= effectQnt.Value;
                }
            }

            if (effects != null && effects.Length > 0)
            {
                ApplyEffectsToTarget(f, ref targetHero, effects);
            }

            ApplyDamageToHero(f, ref targetHero, damage, damageType);
            UpdateManaAfterDamage(f, ref fightingHero, ref targetHero, damage, attackType);
            UpdateDamageStats(ref fightingHero, ref targetHero, damage, attackType);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void DamageHeroWithDOT(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            if (GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes) == false)
            {
                return;
            }

            ApplyDamageToHero(f, ref targetHero, damage, damageType);
            UpdateDamageStats(ref fightingHero, ref targetHero, damage, attackType);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void DamageHeroByBlast(Frame f, FightingHero fightingHero, int centerIndex, Board board, FP damage, int size, DamageType damageType, AttackType attackType)
        {
            if (fightingHero.Hero.Ref == default)
            {
                return;
            }

            List<FightingHero> heroesList = HeroBoard.GetAllTargetsInRange(f, centerIndex, fightingHero.TeamNumber, board, size, includeSelf: true);

            for (int i = 0; i < heroesList.Count; i++)
            {
                FightingHero targetHero = heroesList[i];

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                DamageHero(f, fightingHero, board, targetHero, damage, null, damageType, attackType);
            }
        }

        public static void DamageHeroByBlastWithoutApplyingEffects(Frame f, FightingHero fightingHero, int centerIndex, Board board, FP damage, int size, DamageType damageType, AttackType attackType)
        {
            if (fightingHero.Hero.Ref == default)
            {
                return;
            }

            List<FightingHero> heroesList = HeroBoard.GetAllTargetsInRange(f, centerIndex, fightingHero.TeamNumber, board, size, includeSelf: true);

            for (int i = 0; i < heroesList.Count; i++)
            {
                FightingHero targetHero = heroesList[i];

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                DamageHeroWithDOT(f, fightingHero, board, targetHero, damage, null, damageType, attackType);
            }
        }

        private static bool GetUpdatedHeroes(Frame f, Board board, ref FightingHero fightingHero, ref FightingHero targetHero, out QList<FightingHero> heroes)
        {
            int fightingHeroIndex = fightingHero.Index;
            int targetHeroIndex = targetHero.Index;

            heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHeroIndex];
            targetHero = heroes[targetHeroIndex];

            if (targetHeroIndex < 0)
            {
                return false;
            }

            return true;
        }

        private static void ApplyDamageToHero(Frame f, ref FightingHero targetHero, FP damage, DamageType damageType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            targetHero.CurrentHealth -= damageType switch
            {
                DamageType.Physical => damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.Hero.Defense)),
                DamageType.Magical => damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.Hero.MagicDefense)),
                _ => throw new ArgumentException("Invalid damage type", nameof(damageType)),
            };

            if (targetHero.CurrentHealth <= 0)
            {
                f.Destroy(targetHero.Hero.Ref);
                targetHero.IsAlive = false;
                targetHero.Hero.Ref = default;
            }
        }

        private static FP ApplyHealToHero(Frame f, ref FightingHero targetHero, FP amount, bool isAbleToOverHeal)
        {
            if (targetHero.CurrentHealth <= 0)
            {
                return 0;
            }

            if (isAbleToOverHeal == false)
            {
                if (targetHero.Hero.Health - targetHero.CurrentHealth < amount)
                {
                    amount = targetHero.Hero.Health - targetHero.CurrentHealth;
                }
            }

            targetHero.CurrentHealth += amount;
            return amount;
        }

        private static void UpdateManaAfterDamage(Frame f, ref FightingHero fightingHero, ref FightingHero targetHero, FP damage, AttackType attackType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (targetHero.CurrentHealth > 0)
            {
                if (gameConfig.AddManaByPercentage)
                {
                    targetHero.CurrentMana += damage * gameConfig.ManaTakeDamageRegen;
                }
                else
                {
                    targetHero.CurrentMana += gameConfig.ManaTakeDamageRegen;
                }
            }

            if (attackType != AttackType.Ability)
            {
                if (gameConfig.AddManaByPercentage)
                {
                    fightingHero.CurrentMana += damage * gameConfig.ManaDealDamageRegen;
                }
                else
                {
                    fightingHero.CurrentMana += gameConfig.ManaDealDamageRegen;
                }
            }
        }

        private static void UpdateDamageStats(ref FightingHero fightingHero, ref FightingHero targetHero, FP damage, AttackType attackType)
        {
            if (attackType == AttackType.Ability)
            {
                fightingHero.DealedAbilityDamage += damage;
            }
            else
            {
                fightingHero.DealedBaseDamage += damage;
            }

            targetHero.TakenDamage += damage;
        }

        private static void UpdateHealStats(ref FightingHero fightingHero, ref FightingHero targetHero, FP amount)
        {
            // fightingHero.DealedHeal += amount;
            // targetHero.TakenHeal += amount;
        }

        private static void ApplyEffectsToTarget(Frame f, ref FightingHero targetHero, HeroEffects.Effect[] effects)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].Type == HeroEffects.EffectType.None)
                {
                    continue;
                }

                QList<EffectQnt> targetEffects = f.ResolveList(targetHero.Effects);
                targetEffects.Add(new EffectQnt()
                {
                    Owner = effects[i].Owner,
                    Index = (int)effects[i].Type,
                    Value = effects[i].Value,
                    Duration = effects[i].Duration,
                    Size = effects[i].Size,
                });
            }
        }

        private static void UpdateHeroesAndStats(Frame f, Board board, QList<FightingHero> heroes, FightingHero fightingHero, FightingHero targetHero)
        {
            heroes[fightingHero.Index] = fightingHero;
            heroes[targetHero.Index] = targetHero;

            StatsDisplayer.UpdateStats(f, board);
        }

        private static bool TryFindAttackTarget(Frame f, FightingHero fighingHero, Board board, out FightingHero targetHero)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            targetHero = heroes.ToList().Find(hero => hero.Hero.Ref == fighingHero.AttackTarget);

            if (targetHero.Hero.Ref == default)
            {
                return false;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);
            Transform3D targetTransform = f.Get<Transform3D>(targetHero.Hero.Ref);
            FP targetDistanceToCell = FPVector3.Distance(targetTransform.Position, HeroBoard.GetHeroPosition(f, targetHero));

            if (fighingHero.Hero.RangePercentage * tileSize < targetDistanceToCell)
            {
                return false;
            }

            return true;
        }
    }
}
