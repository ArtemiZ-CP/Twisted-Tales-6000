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

        public static bool IsAbleToAttack(Frame f, FightingHero fighingHero, out FightingHero targetHero)
        {
            if (fighingHero.AttackTarget == default || fighingHero.AttackTimer > 0)
            {
                targetHero = default;
                return false;
            }

            if (TryFindAttackTarget(f, fighingHero, out targetHero))
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
            if (IsAbleToAttack(f, fightingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            DamageHero(f, fightingHero, targetHero, fightingHero.Hero.AttackDamage, effects, damageType, attackType);
            ResetAttackTimer(f, fightingHero);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, DamageType damageType, AttackType attackType)
        {
            if (IsAbleToAttack(f, fightingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            ProjectileAttack(f, fightingHero, targetHero, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, FightingHero targetHero, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, targetHero, targetHero.Hero.AttackDamage, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, targetHero, damage, new HeroEffects.Effect[] { new() }, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, targetHero, damage,
                effects, damageType, attackType);
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
                if (effect.EffectIndex == (int)HeroEffects.EffectType.Curse)
                {
                    reloadMultiplier *= effect.EffectValue;
                }
            }

            fightingHero.AttackTimer -= f.DeltaTime * reloadMultiplier;
            fightingHero.CurrentMana += fightingHero.Hero.ManaRegen * f.DeltaTime;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ProcessAbility(Frame f, FightingHero fightingHero, Board board)
        {
            if (HeroAbility.TryGetAbility(f, fightingHero, out Func<Frame, FightingHero, Board, bool> ability) == false)
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

        public static void DamageHero(Frame f, FightingHero fightingHero, FightingHero targetHero, FP damage, QList<EffectQnt> effectsQnt, DamageType damageType, AttackType attackType)
        {
            HeroEffects.Effect[] effects = new HeroEffects.Effect[effectsQnt.Count];

            for (int i = 0; i < effectsQnt.Count; i++)
            {
                effects[i] = new HeroEffects.Effect(effectsQnt[i]);
            }

            DamageHero(f, fightingHero, targetHero, damage, effects, damageType, attackType);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Board board = HeroBoard.GetBoard(f, fightingHero);

            int fightingHeroIndex = fightingHero.Index;
            int targetHeroIndex = targetHero.Index;

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHeroIndex];
            targetHero = heroes[targetHeroIndex];

            if (targetHeroIndex < 0)
            {
                return;
            }

            QList<EffectQnt> heroEffects = f.ResolveList(targetHero.Effects);

            foreach (EffectQnt effectQnt in heroEffects)
            {
                if (effectQnt.EffectIndex == (int)HeroEffects.EffectType.IncteaseTakingDamage)
                {
                    damage *= effectQnt.EffectValue;
                }
            }

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
            else
            {
                if (gameConfig.AddManaWithPetsentage)
                {
                    targetHero.CurrentMana += damage * gameConfig.ManaTakeDamageRegen;
                }
                else
                {
                    targetHero.CurrentMana += gameConfig.ManaTakeDamageRegen;
                }
            }

            if (attackType == AttackType.Ability)
            {
                fightingHero.DealedAbilityDamage += damage;
            }
            else
            {
                if (gameConfig.AddManaWithPetsentage)
                {
                    fightingHero.CurrentMana += damage * gameConfig.ManaDealDamageRegen;
                }
                else
                {
                    fightingHero.CurrentMana += gameConfig.ManaDealDamageRegen;
                }

                fightingHero.DealedBaseDamage += damage;
            }

            targetHero.TakenDamage += damage;

            if (effects != null && effects.Length > 0)
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
                        OwnerIndex = fightingHero.Index,
                        EffectIndex = (int)effects[i].Type,
                        EffectValue = effects[i].Value,
                        EffectDuration = effects[i].Duration
                    });
                }
            }

            heroes[fightingHeroIndex] = fightingHero;
            heroes[targetHeroIndex] = targetHero;

            StatsDisplayer.UpdateStats(f, board);
        }

        public static void DamageHeroByEffect(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            int fightingHeroIndex = fightingHero.Index;
            int targetHeroIndex = targetHero.Index;

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHeroIndex];
            targetHero = heroes[targetHeroIndex];

            if (targetHeroIndex < 0)
            {
                return;
            }

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

            if (attackType == AttackType.Ability)
            {
                fightingHero.DealedAbilityDamage += damage;
            }
            else
            {
                fightingHero.DealedBaseDamage += damage;
            }

            targetHero.TakenDamage += damage;

            heroes[fightingHeroIndex] = fightingHero;
            heroes[targetHeroIndex] = targetHero;

            StatsDisplayer.UpdateStats(f, board);
        }

        private static bool TryFindAttackTarget(Frame f, FightingHero fighingHero, out FightingHero targetHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fighingHero).FightingHeroesMap);

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
