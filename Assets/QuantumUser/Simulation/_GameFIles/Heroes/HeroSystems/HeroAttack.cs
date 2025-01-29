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
        
        public enum ProjectileType
        {
            Attack,
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

        public static bool TryFindClosestTargetInAttackRange(Frame f, FightingHero fightingHero, out FightingHero targetHero)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargetsInRange(f, fightingHero);

            targetHero = HeroBoard.GetClosestTarget(f, heroesList, fightingHero);

            if (targetHero.Hero.Ref != default)
            {
                return true;
            }

            return false;
        }

        public static FightingHero FindClosestTargetOutOfAttackRange(Frame f, FightingHero fightingHero, out Vector2Int moveTargetPosition, out bool inRange)
        {
            List<FightingHero> heroesList = HeroBoard.GetAllTargets(f, fightingHero);

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

        public static void Update(Frame f, FightingHero fightingHero)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            fightingHero = heroes[fightingHero.Index];
            fightingHero.AttackTimer -= f.DeltaTime;
            fightingHero.CurrentMana += fightingHero.Hero.ManaRegen * f.DeltaTime;
            heroes[fightingHero.Index] = fightingHero;

            ProcessAbility(f, fightingHero);

            f.Events.HeroHealthChanged(board.Player1.Ref, board.Player2.Ref, fightingHero.Hero.Ref, fightingHero.CurrentHealth, fightingHero.Hero.Health);
        }

        public static void InstantAttack(Frame f, FightingHero fightingHero, DamageType damageType)
        {
            if (IsAbleToAttack(f, fightingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            DamageHero(f, fightingHero, targetHero, damageType);
            ResetAttackTimer(f, fightingHero);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, DamageType damageType)
        {
            if (IsAbleToAttack(f, fightingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, targetHero, targetHero.Hero.AttackDamage,
                damageType, ProjectileType.Attack);
            ResetAttackTimer(f, fightingHero);
        }

        public static void ProcessAbility(Frame f, FightingHero fightingHero)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroInfo heroInfo = gameConfig.GetHeroInfo(f, fightingHero.Hero.ID);
            Board board = HeroBoard.GetBoard(f, fightingHero);

            switch (heroInfo.AbilityType)
            {
                case HeroAbilityType.RandomProjectileAttack:
                    RandomProjectileManaAttack(f, fightingHero, (DamageType)fightingHero.Hero.AbilityDamageType);
                    break;
                default:
                    return;
            }

            f.Events.HeroManaChanged(board.Player1.Ref, board.Player2.Ref, fightingHero.Hero.Ref, fightingHero.CurrentMana, fightingHero.Hero.MaxMana);
        }

        public static void RandomProjectileManaAttack(Frame f, FightingHero fightingHero, DamageType damageType)
        {
            if (IsAbleToManaAttack(fightingHero) == false)
            {
                return;
            }

            if (HeroBoard.TryGetRandomTarget(f, fightingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, targetHero, targetHero.Hero.AbilityDamage,
                damageType, ProjectileType.Ability);
                
            ResetMana(f, fightingHero);
        }

        public static void ResetAttackTimer(Frame f, FightingHero fightingHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fightingHero).FightingHeroesMap);
            FightingHero hero = heroes[fightingHero.Index];
            hero.AttackTimer = 1 / hero.Hero.AttackSpeed;
            heroes[fightingHero.Index] = hero;
        }

        public static void ResetMana(Frame f, FightingHero fightingHero)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero hero = heroes[fightingHero.Index];
            hero.CurrentMana = 0;
            heroes[fightingHero.Index] = hero;
        }

        public static void DamageHero(Frame f, FightingHero fightingHero, FightingHero targetHero, DamageType damageType)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero); 
            FP damage = fightingHero.Hero.AttackDamage;

            int targetHeroIndex = targetHero.Index;

            if (targetHeroIndex < 0)
            {
                return;
            }

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            switch (damageType)
            {
                case DamageType.Physical:
                    targetHero.CurrentHealth -= damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.Hero.Defense));
                    break;
                case DamageType.Magical:
                    targetHero.CurrentHealth -= damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.Hero.MagicDefense));
                    break;
                default:
                    throw new System.ArgumentException("Invalid damage type", nameof(damageType));
            }

            if (targetHero.CurrentHealth <= 0)
            {
                f.Destroy(targetHero.Hero.Ref);
                targetHero.IsAlive = false;
                targetHero.Hero.Ref = default;
            }
            else
            {
                fightingHero.CurrentMana += damage * fightingHero.Hero.ManaDealDamageRegenPersent;
                targetHero.CurrentMana += damage * targetHero.Hero.ManaTakeDamageRegenPersent;
            }

            heroes[fightingHero.Index] = fightingHero;
            heroes[targetHeroIndex] = targetHero;

            StatsDisplayer.UpdateDamageStats(f, fightingHero, damage);
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
