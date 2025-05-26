using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class HeroAttack
    {
        public static readonly int MaxDefense = 95;

        public enum DamageType
        {
            Physical,
            Magical,
            True
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
            // Проверяем, что герой, который ищет цель, существует
            if (!f.Exists(fightingHero.Hero.Ref))
            {
                targetHero = default;
                return false;
            }

            List<FightingHero> heroesList = HeroBoard.GetAllTargetsInRange(f, fightingHero, board);

            targetHero = HeroBoard.GetClosestTarget(f, heroesList, fightingHero);

            if (targetHero.Hero.Ref != default && f.Exists(targetHero.Hero.Ref))
            {
                return true;
            }

            return false;
        }

        public static FightingHero FindClosestTargetOutOfAttackRange(Frame f, FightingHero fightingHero, Board heroBoard, out Vector2Int moveTargetPosition)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargets(f, fightingHero, heroBoard);

            if (heroesList.Count == 0)
            {
                moveTargetPosition = default;
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

                int[,] board = HeroBoard.GetBoardMap(heroes);

                if (PathFinder.TryFindPath(board, HeroBoard.GetHeroCords(fightingHero),
                    HeroBoard.GetHeroCords(targetHero), fightingHero.Hero.Range, out moveTargetPosition))
                {
                    return targetHero;
                }

                heroesList.Remove(targetHero);
            }

            moveTargetPosition = default;
            return default;
        }

        public static void Update(Frame f, ref FightingHero fightingHero, Board board, out bool isStunned)
        {
            HeroEffects.ProcessEffects(f, ref fightingHero, board, out isStunned, out bool isSilenced);
            ProcessReloadAttack(f, fightingHero, board);

            if (isStunned)
            {
                Events.ChangeHeroStats(f, fightingHero, board);
                return;
            }

            if (isSilenced == false)
            {
                ProcessAbility(f, ref fightingHero, board);
            }

            Events.ChangeHeroStats(f, fightingHero, board);
        }

        public static bool InstantAttack(Frame f, FightingHero fightingHero, DamageType damageType, AttackType attackType)
        {
            return InstantAttack(f, fightingHero, new HeroEffects.Effect[] { new() }, damageType, attackType);
        }

        public static bool InstantAttack(Frame f, FightingHero fightingHero, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);

            if (IsAbleToAttack(f, fightingHero, board, out FightingHero targetHero) == false)
            {
                return false;
            }

            DamageHero(f, ref fightingHero, board, ref targetHero, fightingHero.Hero.AttackDamage, effects, damageType, attackType);
            ResetAttackTimer(f, ref fightingHero, board);
            return true;
        }

        public static bool ProjectileAttack(Frame f, FightingHero fightingHero, DamageType damageType, AttackType attackType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);

            if (IsAbleToAttack(f, fightingHero, board, out FightingHero targetHero) == false)
            {
                return false;
            }

            ProjectileAttack(f, fightingHero, board, targetHero, damageType, attackType);
            return true;
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, board, targetHero, targetHero.Hero.AttackDamage, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, board, targetHero, damage, null, null, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect effect, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, board, targetHero, damage, new HeroEffects.Effect[] { effect }, null, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.GlobalEffect effect, DamageType damageType, AttackType attackType)
        {
            ProjectileAttack(f, fightingHero, board, targetHero, damage, null, new HeroEffects.GlobalEffect[] { effect }, damageType, attackType);
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
                if (effect.Index == (int)HeroEffects.EffectType.IncreaseAttackSpeed)
                {
                    reloadMultiplier *= effect.Value;
                }
            }

            fightingHero.AttackTimer -= f.DeltaTime * reloadMultiplier;
            fightingHero.AbilityTimer -= f.DeltaTime;
            fightingHero.CurrentMana += fightingHero.Hero.ManaRegen * f.DeltaTime;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ProcessAbility(Frame f, ref FightingHero fightingHero, Board board)
        {
            if (fightingHero.AbilityTimer > 0)
            {
                return;
            }

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            if (HeroAbility.TryGetAbility(f, fightingHero,
                out Func<Frame, FightingHero, Board, QList<FightingHero>, (bool, FP)> TryCastAbility,
                out Action<Frame, FightingHero, Board, QList<FightingHero>> ProcessPassiveAbility) == false)
            {
                return;
            }

            ProcessPassiveAbility(f, fightingHero, board, heroes);

            if (fightingHero.CurrentMana >= fightingHero.Hero.MaxMana)
            {
                (bool casted, FP reloadTime) = TryCastAbility(f, fightingHero, board, heroes);

                if (casted == false)
                {
                    return;
                }

                fightingHero = HeroBoard.GetFightingHero(f, fightingHero.Hero.Ref, board);
                ResetMana(f, fightingHero, board);

                if (fightingHero.AbilityTimer <= 0)
                {
                    ResetAbilityTimer(f, ref fightingHero, board, reloadTime);
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

        public static void ResetAbilityTimer(Frame f, ref FightingHero fightingHero, Board board, FP time)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            fightingHero.AbilityTimer = time;
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
            amount = ApplyHealToHero(f, ref targetHero, amount);
            UpdateHealStats(ref fightingHero, ref targetHero, amount);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static void AddArmorToHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP amount)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);
            AddArmorToHero(f, ref targetHero, amount);
            UpdateHealStats(ref fightingHero, ref targetHero, amount);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
        }

        public static bool DamageHero(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            HeroEffects.Effect[] effects = new HeroEffects.Effect[0];

            return DamageHero(f, ref fightingHero, board, ref targetHero, damage, effects, damageType, attackType);
        }

        public static bool DamageHero(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, FP damage, HeroEffects.Effect effect, DamageType damageType, AttackType attackType)
        {
            HeroEffects.Effect[] effects = new HeroEffects.Effect[1];
            effects[0] = effect;

            return DamageHero(f, ref fightingHero, board, ref targetHero, damage, effects, damageType, attackType);
        }

        public static bool DamageHero(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, QList<EffectQnt> effectsQnt, DamageType damageType, AttackType attackType)
        {
            if (effectsQnt.Count == 0)
            {
                return DamageHero(f, ref fightingHero, board, ref targetHero, damage, damageType, attackType);
            }

            HeroEffects.Effect[] effects = new HeroEffects.Effect[effectsQnt.Count];

            for (int i = 0; i < effectsQnt.Count; i++)
            {
                effects[i] = new HeroEffects.Effect(effectsQnt[i]);
            }

            return DamageHero(f, ref fightingHero, board, ref targetHero, damage, effects, damageType, attackType);
        }

        public static bool DamageHero(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
        {
            bool enemyKilled = DamageHeroWithoutAddMana(f, ref fightingHero, board, ref targetHero, ref damage, effects, damageType, attackType);
            UpdateManaAfterDamage(f, ref fightingHero, board, ref targetHero, damage, attackType);
            return enemyKilled;
        }

        public static bool DamageHeroWithoutAddMana(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, ref FP damage, HeroEffects.Effect[] effects, DamageType damageType, AttackType attackType)
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
                ApplyEffectToTarget(f, ref fightingHero, board, ref targetHero, effects);
            }

            bool enemyKilled = ApplyDamageToHero(f, ref targetHero, board, ref damage, damageType);
            UpdateDamageStats(ref fightingHero, ref targetHero, damage, attackType);
            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);

            return enemyKilled;
        }

        public static void HealHeroWithDOT(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP healAmount, DamageType damageType, AttackType attackType)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);
            ApplyHealToHero(f, ref targetHero, healAmount);
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

                DamageHero(f, ref fightingHero, board, ref targetHero, damage, damageType, attackType);
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

                DamageHero(f, ref fightingHero, board, ref targetHero, damage, damageType, attackType);
            }
        }

        public static void DamageHeroByBlastWithoutAddMana(Frame f, FightingHero fightingHero, int centerIndex, Board board, FP damage, int size, bool includeSelf, HeroEffects.Effect effect, DamageType damageType, AttackType attackType)
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

                DamageHeroWithoutAddMana(f, ref fightingHero, board, ref targetHero, ref damage, new HeroEffects.Effect[] { effect }, damageType, attackType);
            }
        }

        public static void ApplyEffectToTarget(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, HeroEffects.Effect effect)
        {
            ApplyEffectToTarget(f, ref fightingHero, board, ref targetHero, new HeroEffects.Effect[] { effect });
        }

        public static void GetUpdatedHeroes(Frame f, Board board, ref FightingHero fightingHero, ref FightingHero targetHero, out QList<FightingHero> heroes)
        {
            heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHero.Index];
            targetHero = heroes[targetHero.Index];
        }

        private static bool ApplyDamageToHero(Frame f, ref FightingHero targetHero, Board board, ref FP damage, DamageType damageType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            QList<EffectQnt> effectQnts = f.ResolveList(targetHero.Effects);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            FP defense = targetHero.Hero.Defense;
            FP magicDefense = targetHero.Hero.MagicDefense;
            int transferingBleedingEffectIndex = -1;
            bool isRebirthing = false;

            for (int i = 0; i < effectQnts.Count; i++)
            {
                EffectQnt effectQnt = effectQnts[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.Immortal)
                {
                    damage = 0;
                    return false;
                }
                else if (effectQnt.Index == (int)HeroEffects.EffectType.ReduceDefense)
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
            }

            damage = damageType switch
            {
                DamageType.Physical => GetReducedDamage(damage, defense),
                DamageType.Magical => GetReducedDamage(damage, magicDefense),
                DamageType.True => damage,
                _ => throw new ArgumentException("Invalid damage type", nameof(damageType)),
            };

            FP temporaryDamage = damage;

            for (int i = 0; i < effectQnts.Count; i++)
            {
                EffectQnt effectQnt = effectQnts[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.TemporaryArmor)
                {
                    if (effectQnt.Value >= temporaryDamage)
                    {
                        effectQnt.Value -= temporaryDamage;
                        temporaryDamage = 0;
                    }
                    else
                    {
                        temporaryDamage -= effectQnt.Value;
                        effectQnt.Value = 0;
                    }
                }
                else if (effectQnt.Index == (int)HeroEffects.EffectType.TransferingBleeding)
                {
                    transferingBleedingEffectIndex = i;
                }
                else if (effectQnt.Index == (int)HeroEffects.EffectType.FirebirdRebirth)
                {
                    isRebirthing = true;
                }

                effectQnts[i] = effectQnt;
            }

            if (targetHero.CurrentArmor >= temporaryDamage)
            {
                targetHero.CurrentArmor -= temporaryDamage;
                temporaryDamage = 0;
            }
            else
            {
                temporaryDamage -= targetHero.CurrentArmor;
                targetHero.CurrentArmor = 0;
            }

            targetHero.CurrentHealth -= temporaryDamage;

            if (targetHero.CurrentHealth <= 0 && targetHero.Hero.ID >= 0)
            {
                targetHero.CurrentHealth = 0;
                HeroAbility.ProcessAbilityOnDeath(f, ref targetHero, board, heroes);
                FirebirdAbilities.ProcessDeathInFirebirdRebirthRange(f, ref targetHero, board, heroes);
                HeroEffects.ProcessTransferingBleedingEffect(f, ref targetHero, board, effectQnts, transferingBleedingEffectIndex);

                if (isRebirthing)
                {
                    return false;
                }

                if (targetHero.ExtraLives > 0)
                {
                    HeroNameEnum heroName = gameConfig.GetHeroInfo(f, targetHero.Hero.ID).Name;

                    if (heroName == HeroNameEnum.Firebird)
                    {
                        FirebirdAbilities.ProcessLoseLife(f, ref targetHero, board, heroes);
                    }
                }
                else
                {
                    f.Destroy(targetHero.Hero.Ref);
                    targetHero.IsAlive = false;
                    targetHero.Hero.Ref = default;
                }

                return true;
            }

            return false;
        }

        public static void DestroyHero(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            f.Destroy(fightingHero.Hero.Ref);
            fightingHero.IsAlive = false;
            fightingHero.Hero.Ref = default;
            heroes[fightingHero.Index] = fightingHero;
        }

        public static void ApplyEffectToTarget(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, HeroEffects.Effect[] effects)
        {
            GetUpdatedHeroes(f, board, ref fightingHero, ref targetHero, out QList<FightingHero> heroes);

            QList<EffectQnt> targetEffects = f.ResolveList(targetHero.Effects);

            for (int i = 0; i < effects.Length; i++)
            {
                HeroEffects.Effect effect = effects[i];

                if (effect == null || effect.Type == HeroEffects.EffectType.None)
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

            heroes[targetHero.Index] = targetHero;
        }

        private static FP GetReducedDamage(FP damage, FP defense)
        {
            return damage * (1 - (FPMath.Min(defense, MaxDefense) / 100));
        }

        private static FP ApplyHealToHero(Frame f, ref FightingHero targetHero, FP amount)
        {
            if (targetHero.CurrentHealth <= 0)
            {
                return 0;
            }

            var effects = f.ResolveList(targetHero.Effects);

            foreach (var effect in effects)
            {
                if (effect.Index == (int)HeroEffects.EffectType.IncreaseHealAndArmor)
                {
                    amount *= effect.Value;
                }
            }

            if (targetHero.Hero.Health - targetHero.CurrentHealth < amount)
            {
                amount = targetHero.Hero.Health - targetHero.CurrentHealth;
            }

            targetHero.CurrentHealth += amount;
            return amount;
        }

        private static void AddArmorToHero(Frame f, ref FightingHero targetHero, FP amount)
        {
            if (targetHero.CurrentHealth <= 0)
            {
                return;
            }

            var effects = f.ResolveList(targetHero.Effects);

            foreach (var effect in effects)
            {
                if (effect.Index == (int)HeroEffects.EffectType.IncreaseHealAndArmor)
                {
                    amount *= effect.Value;
                }
            }

            targetHero.CurrentArmor += amount;
        }

        private static void UpdateManaAfterDamage(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero targetHero, FP damage, AttackType attackType)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
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

            UpdateHeroesAndStats(f, board, heroes, fightingHero, targetHero);
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

        private static void UpdateHeroesAndStats(Frame f, Board board, QList<FightingHero> heroes, FightingHero fightingHero, FightingHero targetHero)
        {
            heroes[fightingHero.Index] = fightingHero;
            heroes[targetHero.Index] = targetHero;

            StatsDisplayer.UpdateStats(f, heroes, board);
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
