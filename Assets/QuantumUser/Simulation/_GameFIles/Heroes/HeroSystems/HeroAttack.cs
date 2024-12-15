using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class HeroAttack
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
            if (fighingHero.Hero.AttackTarget == default || fighingHero.Hero.AttackTimer > 0)
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
            if (fighingHero.Hero.CurrentMana < fighingHero.Hero.MaxMana)
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

                            if (heroes[index].Hero.IsAlive && heroes[index].Hero.Ref != default)
                            {
                                heroID = heroes[index].Hero.ID;
                            }

                            board[x, y] = heroID;
                        }
                    }
                }

                if (PathFinder.TryFindPath(board, HeroBoard.GetHeroCords(fightingHero.Hero),
                    HeroBoard.GetHeroCords(targetHero.Hero), fightingHero.Hero.Range, out moveTargetPosition, out inRange))
                {
                    return targetHero;
                }

                heroesList.Remove(targetHero);
            }

            moveTargetPosition = default;
            inRange = false;
            return default;
        }

        public static void Update(Frame f, FightingHero fighingHero)
        {
            Board board = HeroBoard.GetBoard(f, fighingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            FightingHero fightingHero = heroes[fighingHero.Index];
            fightingHero.Hero.AttackTimer -= f.DeltaTime;
            fightingHero.Hero.CurrentMana += fightingHero.Hero.ManaRegen * f.DeltaTime;
            heroes[fighingHero.Index] = fightingHero;

            ProcessAbility(f, fightingHero);

            f.Events.HeroHealthChanged(board.Player1.Ref, board.Player2.Ref, fighingHero.Hero.Ref, fighingHero.Hero.CurrentHealth, fighingHero.Hero.Health);
        }

        public static void InstantAttack(Frame f, FightingHero fighingHero, DamageType damageType)
        {
            if (IsAbleToAttack(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            DamageHero(f, HeroBoard.GetBoard(f, fighingHero), fighingHero.Hero.AttackDamage, targetHero.Hero, damageType);
            ResetAttackTimer(f, fighingHero);
        }

        public static void ProjectileAttack(Frame f, FightingHero fighingHero, DamageType damageType)
        {
            if (IsAbleToAttack(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            HeroProjectilesSystem.SpawnProjectile(f, fighingHero, targetHero.Hero, targetHero.Hero.AttackDamage,
                damageType, ProjectileType.Attack);
            ResetAttackTimer(f, fighingHero);
        }

        public static void ProcessAbility(Frame f, FightingHero fighingHero)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroInfo heroInfo = gameConfig.GetHeroInfo(f, fighingHero.Hero.ID);
            Board board = HeroBoard.GetBoard(f, fighingHero);

            switch (heroInfo.AbilityType)
            {
                case HeroAbilityType.RandomProjectileAttack:
                    RandomProjectileManaAttack(f, fighingHero, (DamageType)fighingHero.Hero.AbilityDamageType);
                    break;
                default:
                    return;
            }

            f.Events.HeroManaChanged(board.Player1.Ref, board.Player2.Ref, fighingHero.Hero.Ref, fighingHero.Hero.CurrentMana, fighingHero.Hero.MaxMana);
        }

        public static void RandomProjectileManaAttack(Frame f, FightingHero fighingHero, DamageType damageType)
        {
            if (IsAbleToManaAttack(fighingHero) == false)
            {
                return;
            }

            if (HeroBoard.TryGetRandomTarget(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            HeroProjectilesSystem.SpawnProjectile(f, fighingHero, targetHero.Hero, targetHero.Hero.AbilityDamage,
                damageType, ProjectileType.Ability);
            ResetMana(f, fighingHero);
        }

        public static void ResetAttackTimer(Frame f, FightingHero fighingHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fighingHero).FightingHeroesMap);
            FightingHero hero = heroes[fighingHero.Index];
            hero.Hero.AttackTimer = 1 / hero.Hero.AttackSpeed;
            heroes[fighingHero.Index] = hero;
        }

        public static void ResetMana(Frame f, FightingHero fighingHero)
        {
            Board board = HeroBoard.GetBoard(f, fighingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero hero = heroes[fighingHero.Index];
            hero.Hero.CurrentMana = 0;
            heroes[fighingHero.Index] = hero;
        }

        public static void DamageHero(Frame f, Board board, FP damage, HeroEntity targetHero, DamageType damageType)
        {
            int targetHeroIndex = f.ResolveList(board.FightingHeroesMap).ToList().FindIndex(hero => hero.Hero.Ref == targetHero.Ref);

            if (targetHeroIndex < 0)
            {
                return;
            }

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero target = heroes[targetHeroIndex];
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            switch (damageType)
            {
                case DamageType.Physical:
                    target.Hero.CurrentHealth -= damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.Defense));
                    break;
                case DamageType.Magical:
                    target.Hero.CurrentHealth -= damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.MagicDefense));
                    break;
                default:
                    throw new System.ArgumentException("Invalid damage type", nameof(damageType));
            }

            if (target.Hero.CurrentHealth <= 0)
            {
                f.Events.DestroyHero(board.Player1.Ref, board.Player2.Ref, target.Hero.Ref);
                target.Hero.IsAlive = false;
                target.Hero.ID = -1;
                target.Hero.Ref = default;
            }
            else
            {
                target.Hero.CurrentMana += damage * target.Hero.ManaDamageRegenPersent;
            }

            heroes[targetHeroIndex] = target;
        }

        private static bool TryFindAttackTarget(Frame f, FightingHero fighingHero, out FightingHero targetHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fighingHero).FightingHeroesMap);

            targetHero = heroes.ToList().Find(hero => hero.Hero.Ref == fighingHero.Hero.AttackTarget);

            if (targetHero.Hero.Ref == default)
            {
                return false;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);
            Transform3D targetTransform = f.Get<Transform3D>(targetHero.Hero.Ref);
            FP targetDistanceToCell = FPVector3.Distance(targetTransform.Position, HeroBoard.GetHeroPosition(f, targetHero.Hero));

            if (fighingHero.Hero.RangePercentage * tileSize < targetDistanceToCell)
            {
                return false;
            }

            return true;
        }
    }
}
