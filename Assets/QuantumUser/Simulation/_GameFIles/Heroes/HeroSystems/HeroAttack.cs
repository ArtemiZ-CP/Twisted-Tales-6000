using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class HeroAttack
    {
        public static bool IsAbleToAttack(Frame f, FightingHero fighingHero, out FightingHero targetHero)
        {
            targetHero = default;

            if (fighingHero.Hero.AttackTarget == default || fighingHero.Hero.AttackTimer > 0)
            {
                return false;
            }

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

        public static bool TryFindClosestTargetInAttackRange(Frame f, QList<FightingHero> heroes, FightingHero fightingHero, out FightingHero targetHero)
        {
            List<Vector2Int> closeTiles = new();
            List<FightingHero> heroesList = new();

            HeroBoard.GetCloseCords(fightingHero.Index, ref closeTiles, fightingHero.Hero.Range);

            foreach (var tile in closeTiles)
            {
                if (HeroBoard.TryConvertCordsToIndex(tile, out int index) == false)
                {
                    continue;
                }

                if (heroes[index].Hero.ID < 0)
                {
                    continue;
                }

                if (fightingHero.Hero.TeamNumber == heroes[index].Hero.TeamNumber || heroes[index].Hero.IsAlive == false)
                {
                    continue;
                }

                heroesList.Add(heroes[index]);
            }

            targetHero = HeroBoard.GetClosestTarget(f, heroesList, fightingHero);

            if (targetHero.Hero.Ref != default)
            {
                return true;
            }

            return false;
        }

        public static FightingHero FindClosestTargetOutOfAttackRange(Frame f, QList<FightingHero> heroes, FightingHero fightingHero, out Vector2Int moveTargetPosition, out bool inRange)
        {
            List<FightingHero> heroesList = new();

            foreach (FightingHero target in heroes)
            {
                if (target.Hero.Ref == default)
                {
                    continue;
                }

                if (fightingHero.Hero.TeamNumber == target.Hero.TeamNumber || target.Hero.IsAlive == false)
                {
                    continue;
                }

                heroesList.Add(target);
            }

            if (heroesList.Count == 0)
            {
                moveTargetPosition = default;
                inRange = false;
                return default;
            }

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

        public static void ProcessReload(Frame f, FightingHero fighingHero)
        {
            QList<FightingHero> heroes = f.ResolveList(HeroBoard.GetBoard(f, fighingHero).FightingHeroesMap);

            FightingHero fightingHero = heroes[fighingHero.Index];
            fightingHero.Hero.AttackTimer -= f.DeltaTime;
            heroes[fighingHero.Index] = fightingHero;
        }

        public static void ProcessInstantAttack(Frame f, FightingHero fighingHero)
        {
            if (IsAbleToAttack(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            DamageHero(f, HeroBoard.GetBoard(f, fighingHero), fighingHero.Hero.Damage, targetHero.Hero);
            ResetAttackTimer(f, fighingHero);
        }

        public static void ProcessProjectileAttack(Frame f, FightingHero fighingHero)
        {
            if (IsAbleToAttack(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            HeroProjectilesSystem.SpawnProjectile(f, fighingHero, targetHero.Hero);
            ResetAttackTimer(f, fighingHero);
        }

        public static void ResetAttackTimer(Frame f, FightingHero fighingHero)
        {
            Board board = HeroBoard.GetBoard(f, fighingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero hero = heroes[fighingHero.Index];
            hero.Hero.AttackTimer = 1 / hero.Hero.AttackSpeed;
            heroes[fighingHero.Index] = hero;
        }

        public static void DamageHero(Frame f, Board board, FP damage, Hero targetHero)
        {
            int targetHeroIndex = f.ResolveList(board.FightingHeroesMap).ToList().FindIndex(hero => hero.Hero.Ref == targetHero.Ref);

            if (targetHeroIndex < 0)
            {
                return;
            }

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero target = heroes[targetHeroIndex];
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            target.Hero.CurrentHealth -= damage * (gameConfig.HeroDefenseRatio / (gameConfig.HeroDefenseRatio + targetHero.Defense));

            if (target.Hero.CurrentHealth <= 0)
            {
                f.Events.DestroyHero(board.Player1.Ref, board.Player2.Ref, target.Hero.Ref);
                target.Hero.IsAlive = false;
                target.Hero.ID = -1;
                target.Hero.Ref = default;
            }
            else
            {
                f.Events.HeroHealthChanged(board.Player1.Ref, board.Player2.Ref, target.Hero.Ref, target.Hero.CurrentHealth, target.Hero.Health);
            }

            heroes[targetHeroIndex] = target;
        }
    }
}
