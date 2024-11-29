using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe static class BoardPosition
    {
        public static Vector2Int GetHeroCords(Hero hero)
        {
            return new Vector2Int(hero.TargetPositionX, hero.TargetPositionY);
        }

        public static FPVector3 GetHeroPosition(Frame f, Hero hero)
        {
            Vector2Int cords = GetHeroCords(hero);

            return GetTilePosition(f, cords);
        }

        public static bool TryGetHeroCords(int heroIndex, out Vector2Int cords)
        {
            return TryConvertIndexToCords(heroIndex, out cords);
        }

        public static FPVector3 GetTilePosition(Frame f, Vector2Int cords)
        {
            return GetTilePosition(f, cords.x, cords.y);
        }

        public static FPVector3 GetTilePosition(Frame f, int x, int y)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);

            FPVector3 position = new FPVector3(x, 0, y) * tileSize;
            position -= tileSize * new FPVector3(GameConfig.BoardSize, 0, GameConfig.BoardSize) / 2;
            position += new FPVector3(tileSize, 0, tileSize) / 2;

            return position;
        }

        public static bool TryConvertIndexToCords(int index, out Vector2Int cords)
        {
            cords = new Vector2Int(index % GameConfig.BoardSize, index / GameConfig.BoardSize);

            return cords.x >= 0 && cords.x < GameConfig.BoardSize && cords.y >= 0 && cords.y < GameConfig.BoardSize;
        }

        public static bool TryConvertCordsToIndex(Vector2Int cords, out int index)
        {
            index = cords.x + cords.y * GameConfig.BoardSize;

            return index >= 0 && index < GameConfig.BoardSize * GameConfig.BoardSize;
        }

        private static void GetCloseCords(int heroIndex, ref List<Vector2Int> closeTiles, int range = 1)
        {
            if (TryGetHeroCords(heroIndex, out Vector2Int cords) == false)
            {
                return;
            }

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector2Int newCords = cords + new Vector2Int(x, y);

                    if (newCords == cords)
                    {
                        continue;
                    }

                    if (newCords.x < 0 || newCords.x >= GameConfig.BoardSize)
                    {
                        continue;
                    }

                    if (newCords.y < 0 || newCords.y >= GameConfig.BoardSize)
                    {
                        continue;
                    }

                    closeTiles.Add(newCords);
                }
            }
        }

        public static FightingHero GetClosestTarget(Frame f, List<FightingHero> heroes, FightingHero hero)
        {
            FP minDistance = FP.MaxValue;
            FightingHero closestHero = default;

            foreach (var target in heroes)
            {
                if (target.Hero.ID < 0 || target.Hero.Ref == hero.Hero.Ref)
                {
                    continue;
                }

                Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(target.Hero.Ref);
                Transform3D* heroTransform = f.Unsafe.GetPointer<Transform3D>(hero.Hero.Ref);

                FP distance = FPVector3.Distance(targetTransform->Position, heroTransform->Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHero = target;
                }
            }

            return closestHero;
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
                FightingHero targetHero = GetClosestTarget(f, heroesList, fightingHero);

                if (targetHero.Hero.Ref == default)
                {
                    continue;
                }

                int[,] board = new int[GameConfig.BoardSize, GameConfig.BoardSize];

                for (int x = 0; x < GameConfig.BoardSize; x++)
                {
                    for (int y = 0; y < GameConfig.BoardSize; y++)
                    {
                        if (TryConvertCordsToIndex(new Vector2Int(x, y), out int index))
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

                if (PathFinder.TryFindPath(board, GetHeroCords(fightingHero.Hero),
                    GetHeroCords(targetHero.Hero), fightingHero.Hero.Range, out moveTargetPosition, out inRange))
                {
                    return targetHero;
                }

                heroesList.Remove(targetHero);
            }

            moveTargetPosition = default;
            inRange = false;
            return default;
        }

        public static bool TryFindClosestTargetInAttackRange(Frame f, QList<FightingHero> heroes, FightingHero fightingHero, out FightingHero targetHero)
        {
            List<Vector2Int> closeTiles = new();
            List<FightingHero> heroesList = new();

            GetCloseCords(fightingHero.Index, ref closeTiles, fightingHero.Hero.Range);

            foreach (var tile in closeTiles)
            {
                if (TryConvertCordsToIndex(tile, out int index) == false)
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

            targetHero = GetClosestTarget(f, heroesList, fightingHero);

            if (targetHero.Hero.Ref != default)
            {
                return true;
            }

            return false;
        }

        public static bool IsAbleToAttack(Frame f, FightingHero fighingHero, out FightingHero targetHero)
        {
            targetHero = default;

            if (fighingHero.Hero.AttackTarget == default || fighingHero.Hero.AttackTimer > 0)
            {
                return false;
            }

            QList<FightingHero> heroes = f.ResolveList(BaseHeroFightingSystem.GetBoard(f, fighingHero).FightingHeroesMap);

            targetHero = heroes.ToList().Find(hero => hero.Hero.Ref == fighingHero.Hero.AttackTarget);

            if (targetHero.Hero.Ref == default)
            {
                return false;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);
            Transform3D targetTransform = f.Get<Transform3D>(targetHero.Hero.Ref);
            FP targetDistanceToCell = FPVector3.Distance(targetTransform.Position, GetHeroPosition(f, targetHero.Hero));

            if (fighingHero.Hero.RangePercentage * tileSize < targetDistanceToCell)
            {
                return false;
            }

            return true;
        }
    }
}
