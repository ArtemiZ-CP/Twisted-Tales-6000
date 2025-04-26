using System;
using System.Collections.Generic;
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

        public static bool TryFindTargetWithMaxAttackInAttackRange(Frame f, FightingHero fightingHero, Board board, out FightingHero targetHero)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargetsInRange(f, fightingHero, board);

            targetHero = default;
            FP maxDamage = 0;

            foreach (FightingHero hero in heroesList)
            {
                if (hero.Hero.Ref == default || hero.IsAlive == false)
                {
                    continue;
                }

                if (hero.Hero.AttackDamage > maxDamage)
                {
                    maxDamage = hero.Hero.AttackDamage;
                    targetHero = hero;
                }
            }

            if (targetHero.Hero.Ref != default)
            {
                return true;
            }

            return false;
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

            QList<FightingHero> heroes = f.ResolveList(heroBoard.FightingHeroesMap);

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

        public static void Update(Frame f, ref FightingHero fightingHero, Board board, out bool isStunned)
        {
            HeroEffects.ProcessEffects(f, ref fightingHero, board, out isStunned);

            if (isStunned)
            {
                Events.ChangeHeroStats(f, fightingHero, board);
                return;
            }

            ProcessReloadAttack(f, fightingHero, board);
            ProcessAbility(f, ref fightingHero, board);
            Events.ChangeHeroStats(f, fightingHero, board);
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
            ResetAttackTimer(f, ref fightingHero, board);
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
            ResetAttackTimer(f, ref fightingHero, board);
        }

        public static void ProcessReloadAttack(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHero.Index];

            FP reloadMultiplier = 1;
            QList<EffectQnt> effects = f.ResolveList(fightingHero.Effects);

            foreach (EffectQnt effect in effects)
            {
                if (effect.Index == (int)HeroEffects.EffectType.ReduceAttackSpeed)
                {
                    reloadMultiplier *= 1 - effect.Value;
                }
            }

            fightingHero.AttackTimer -= f.DeltaTime * reloadMultiplier;
            fightingHero.CurrentMana += fightingHero.Hero.ManaRegen * f.DeltaTime;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ProcessAbility(Frame f, ref FightingHero fightingHero, Board board)
        {
            if (fightingHero.AttackTimer > 0)
            {
                return;
            }

            if (HeroAbility.TryGetAbility(f, fightingHero, out Func<Frame, FightingHero, Board, bool> tryCastAbility) == false)
            {
                return;
            }

            if (fightingHero.CurrentMana >= fightingHero.Hero.MaxMana && tryCastAbility(f, fightingHero, board))
            {
                fightingHero = HeroBoard.GetFightingHero(f, fightingHero.Hero.Ref, board);
                ResetMana(f, fightingHero, board);

                if (fightingHero.AttackTimer <= 0)
                {
                    ResetAttackTimer(f, ref fightingHero, board);
                }
            }
        }

        public static void ResetAttackTimer(Frame f, ref FightingHero fightingHero, Board board)
        {
            ResetAttackTimer(f, ref fightingHero, board, 1 / fightingHero.Hero.AttackSpeed);
        }

        public static void ResetAttackTimer(Frame f, ref FightingHero fightingHero, Board board, FP time)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            fightingHero.AttackTimer = time;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ResetMana(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            fightingHero.CurrentMana = 0;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void HealHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP amount)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);
            amount = ApplyHealToHero(ref targetHero, amount);
            UpdateHealStats(ref fightingHero, ref targetHero, amount);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void AddArmorToHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP amount)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);
            AddArmorToHero(ref targetHero, amount);
            UpdateHealStats(ref fightingHero, ref targetHero, amount);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            HeroEffects.Effect[] effects = new HeroEffects.Effect[0];

            DamageHero(f, fightingHero, board, targetHero, damage, effects, damageType, attackType);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect effect, DamageType damageType, AttackType attackType)
        {
            HeroEffects.Effect[] effects = new HeroEffects.Effect[1];
            effects[0] = effect;

            DamageHero(f, fightingHero, board, targetHero, damage, effects, damageType, attackType);
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, QList<EffectQnt> effectsQnt, DamageType damageType, AttackType attackType)
        {
            if (effectsQnt.Count == 0)
            {
                DamageHero(f, fightingHero, board, targetHero, damage, damageType, attackType);
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
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);

            QList<EffectQnt> targetHeroEffects = f.ResolveList(targetHero.Effects);
            QList<EffectQnt> heroEffects = f.ResolveList(fightingHero.Effects);

            for (int i = 0; i < heroEffects.Count; i++)
            {
                EffectQnt effectQnt = heroEffects[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.ExtraBaseDamage && attackType == AttackType.Base)
                {
                    damage += effectQnt.Value;
                    effectQnt.Size--;

                    if (effectQnt.Size <= 0)
                    {
                        heroEffects.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        heroEffects[i] = effectQnt;
                    }
                }
            }

            for (int i = 0; i < targetHeroEffects.Count; i++)
            {
                EffectQnt effectQnt = targetHeroEffects[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.IncreaseTakingDamage)
                {
                    damage *= effectQnt.Value;
                }
            }

            if (effects != null && effects.Length > 0)
            {
                ApplyEffectsToTarget(f, fightingHero, board, ref targetHero, effects);
            }

            ApplyDamageToHero(f, ref targetHero, board, damage, damageType);
            UpdateManaAfterDamage(f, ref fightingHero, ref targetHero, damage, attackType);
            UpdateDamageStats(ref fightingHero, ref targetHero, damage, attackType);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void DamageHeroWithDOT(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);
            ApplyDamageToHero(f, ref targetHero, board, damage, damageType);
            UpdateDamageStats(ref fightingHero, ref targetHero, damage, attackType);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void HealHeroWithDOT(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP healAmount, DamageType damageType, AttackType attackType)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);
            ApplyHealToHero(ref targetHero, healAmount);
            UpdateDamageStats(ref fightingHero, ref targetHero, healAmount, attackType);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void DamageHeroByHorizontalBlast(Frame f, FightingHero fightingHero, int centerIndex, Board board, FP damage, int size, bool includeSelf, DamageType damageType, AttackType attackType)
        {
            if (fightingHero.Hero.Ref == default)
            {
                return;
            }

            List<FightingHero> heroesList = HeroBoard.GetAllTeamHeroesInHorizontalRange(f, centerIndex, HeroBoard.GetEnemyTeamNumber(fightingHero.TeamNumber), board, size, includeSelf);

            for (int i = 0; i < heroesList.Count; i++)
            {
                FightingHero targetHero = heroesList[i];

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                DamageHero(f, fightingHero, board, targetHero, damage, damageType, attackType);
            }
        }

        public static void DamageHeroByBlast(Frame f, FightingHero fightingHero, int centerIndex, Board board, FP damage, int size, bool includeSelf, DamageType damageType, AttackType attackType)
        {
            if (fightingHero.Hero.Ref == default)
            {
                return;
            }

            List<FightingHero> heroesList = HeroBoard.GetAllTeamHeroesInRange(f, centerIndex, HeroBoard.GetEnemyTeamNumber(fightingHero.TeamNumber), board, size, includeSelf);

            for (int i = 0; i < heroesList.Count; i++)
            {
                FightingHero targetHero = heroesList[i];

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                DamageHero(f, fightingHero, board, targetHero, damage, damageType, attackType);
            }
        }

        public static void DamageHeroByBlastWithoutApplyingEffects(Frame f, FightingHero fightingHero, int centerIndex, Board board, FP damage, int size, DamageType damageType, AttackType attackType)
        {
            if (fightingHero.Hero.Ref == default)
            {
                return;
            }

            List<FightingHero> heroesList = HeroBoard.GetAllTeamHeroesInRange(f, centerIndex, HeroBoard.GetEnemyTeamNumber(fightingHero.TeamNumber), board, size, includeSelf: true);

            for (int i = 0; i < heroesList.Count; i++)
            {
                FightingHero targetHero = heroesList[i];

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                DamageHeroWithDOT(f, fightingHero, board, targetHero, damage, damageType, attackType);
            }
        }

        public static void ApplyEffectToTarget(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, HeroEffects.Effect effects)
        {
            ApplyEffectsToTarget(f, fightingHero, board, ref targetHero, new HeroEffects.Effect[] { effects });
        }

        public static void ApplyEffectsToTarget(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, HeroEffects.Effect[] effects)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);

            if (effects != null && effects.Length > 0)
            {
                ApplyEffectsToTarget(f, fightingHero, board, ref targetHero, effects);
            }

            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        private static void GetUpdatedHeroes(Frame f, Board board, ref FightingHero fightingHero, ref FightingHero targetHero, out QList<FightingHero> heroes)
        {
            int fightingHeroIndex = fightingHero.Index;
            int targetHeroIndex = targetHero.Index;

            heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHeroIndex];
            targetHero = heroes[targetHeroIndex];
        }

        private static void ApplyDamageToHero(Frame f, ref FightingHero targetHero, Board board, FP damage, DamageType damageType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            QList<EffectQnt> effectQnts = f.ResolveList(targetHero.Effects);

            FP defense = targetHero.Hero.Defense;
            FP magicDefense = targetHero.Hero.MagicDefense;
            int transferingBleedingEffectIndex = -1;

            for (int i = 0; i < effectQnts.Count; i++)
            {
                EffectQnt effectQnt = effectQnts[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.ReduceDefense)
                {
                    defense -= effectQnt.Value;

                    if (defense < 0)
                    {
                        defense = 0;
                    }
                }
                else if (effectQnt.Index == (int)HeroEffects.EffectType.ReduceMagicDefense)
                {
                    magicDefense -= effectQnt.Value;

                    if (magicDefense < 0)
                    {
                        magicDefense = 0;
                    }
                }

                effectQnts[i] = effectQnt;
            }

            damage = damageType switch
            {
                DamageType.Physical => damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + defense)),
                DamageType.Magical => damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + magicDefense)),
                _ => throw new ArgumentException("Invalid damage type", nameof(damageType)),
            };

            for (int i = 0; i < effectQnts.Count; i++)
            {
                EffectQnt effectQnt = effectQnts[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.TemporaryArmor)
                {
                    if (effectQnt.Value >= damage)
                    {
                        effectQnt.Value -= damage;
                        damage = 0;
                    }
                    else
                    {
                        damage -= effectQnt.Value;
                        effectQnt.Value = 0;
                    }
                }
                else if (effectQnt.Index == (int)HeroEffects.EffectType.TransferingBleeding)
                {
                    transferingBleedingEffectIndex = i;
                }

                effectQnts[i] = effectQnt;
            }

            if (targetHero.CurrentArmor >= damage)
            {
                targetHero.CurrentArmor -= damage;
                damage = 0;
            }
            else
            {
                damage -= targetHero.CurrentArmor;
                targetHero.CurrentArmor = 0;
            }

            targetHero.CurrentHealth -= damage;

            if (targetHero.CurrentHealth <= 0)
            {
                if (transferingBleedingEffectIndex >= 0)
                {
                    EffectQnt effectQnt = effectQnts[transferingBleedingEffectIndex];

                    HeroEffects.Effect bleedEffect = new(effectQnt)
                    {
                        Duration = effectQnt.MaxDuration
                    };

                    List<FightingHero> alies = HeroBoard.GetAllTeamHeroesInRange(f, targetHero.Index, targetHero.TeamNumber, board, bleedEffect.Size);

                    if (alies.Count > 0)
                    {
                        FightingHero closestTarget = HeroBoard.GetClosestTarget(f, alies, targetHero);
                        FightingHero owner = HeroBoard.GetFightingHero(f, bleedEffect.Owner, board);
                        ApplyEffectToTarget(f, owner, board, closestTarget, bleedEffect);
                    }
                }

                f.Destroy(targetHero.Hero.Ref);
                targetHero.IsAlive = false;
                targetHero.Hero.Ref = default;
            }
        }

        private static FP ApplyHealToHero(ref FightingHero targetHero, FP amount)
        {
            if (targetHero.CurrentHealth <= 0)
            {
                return 0;
            }

            if (targetHero.Hero.Health - targetHero.CurrentHealth < amount)
            {
                amount = targetHero.Hero.Health - targetHero.CurrentHealth;
            }

            targetHero.CurrentHealth += amount;
            return amount;
        }

        private static void AddArmorToHero(ref FightingHero targetHero, FP amount)
        {
            if (targetHero.CurrentHealth <= 0)
            {
                return;
            }

            targetHero.CurrentArmor += amount;
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

        private static void ApplyEffectsToTarget(Frame f, FightingHero fightingHero, Board board, ref FightingHero targetHero, HeroEffects.Effect[] effects)
        {
            QList<EffectQnt> targetEffects = f.ResolveList(targetHero.Effects);

            for (int i = 0; i < effects.Length; i++)
            {
                HeroEffects.Effect effect = effects[i];

                if (effect.Type == HeroEffects.EffectType.None)
                {
                    continue;
                }
                else if (effect.Type == HeroEffects.EffectType.HorizontalBlast)
                {
                    DamageHeroByHorizontalBlast(f, fightingHero, targetHero.Index, board, effect.Value, effect.Size, includeSelf: false, DamageType.Magical, AttackType.Ability);
                    continue;
                }

                targetEffects.Add(new EffectQnt()
                {
                    Owner = effect.Owner,
                    Index = (int)effect.Type,
                    MaxValue = effect.MaxValue,
                    Value = effect.Value,
                    MaxDuration = effect.MaxDuration,
                    Duration = effect.Duration,
                    Size = effect.Size,
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

            targetHero = default;

            foreach (FightingHero hero in heroes)
            {
                if (hero.Hero.Ref == fighingHero.AttackTarget)
                {
                    targetHero = hero;
                    break;
                }
            }

            if (targetHero.Hero.Ref == default)
            {
                return false;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);
            Transform3D targetTransform = f.Get<Transform3D>(targetHero.Hero.Ref);
            FP targetDistanceToCellSqr = FPVector3.DistanceSquared(targetTransform.Position, HeroBoard.GetHeroPosition(f, targetHero));
            FP range = fighingHero.Hero.RangePercentage * tileSize;

            if (range * range < targetDistanceToCellSqr)
            {
                return false;
            }

            return true;
        }
    }
}
