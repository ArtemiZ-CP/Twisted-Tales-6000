using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class HeroBoard
    {
        public static Board GetBoard(Frame f, FightingHero fighingHero)
        {
            List<Board> boards = BoardSystem.GetBoards(f);
            return boards[fighingHero.BoardIndex];
        }

        public static Vector2Int GetHeroCords(FightingHero hero)
        {
            return new Vector2Int(hero.TargetPositionX, hero.TargetPositionY);
        }

        public static FPVector3 GetHeroPosition(Frame f, FightingHero hero)
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

        public static bool IsHeroMoving(Frame f, FightingHero hero)
        {
            Transform3D transform = f.Get<Transform3D>(hero.Hero.Ref);
            FPVector3 position = GetHeroPosition(f, hero);

            return transform.Position != position;
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

        public static void GetCloseCords(int heroIndex, ref List<Vector2Int> closeTiles, int range = 1)
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

        public static bool TrySetTarget(Frame f, FightingHero fightingHero, Board board)
        {
            FightingHero target = GetHeroTarget(f, fightingHero, board, out Vector2Int moveTargetPosition);

            if (target.Hero.Ref == default)
            {
                return false;
            }

            if (moveTargetPosition == GetHeroCords(fightingHero))
            {
                SetHeroTarget(f, fightingHero, target.Hero.Ref);
                return true;
            }

            SetHeroTarget(f, fightingHero, target.Hero.Ref, moveTargetPosition);
            return false;
        }

        public static FightingHero GetHeroTarget(Frame f, FightingHero fightingHero, Board board, out Vector2Int moveTargetPosition)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero targetHero))
            {
                moveTargetPosition = GetHeroCords(fightingHero);
                return targetHero;
            }

            FightingHero hero = HeroAttack.FindClosestTargetOutOfAttackRange(f, fightingHero, board, out moveTargetPosition, out bool inRange);

            return hero;
        }

        public static void SetHeroTarget(Frame f, FightingHero fightingHero, EntityRef attackTarget, Vector2Int targetPosition)
        {
            QList<FightingHero> heroes = f.ResolveList(GetBoard(f, fightingHero).FightingHeroesMap);

            if (TryConvertCordsToIndex(targetPosition, out int heroNewIndex))
            {
                if (fightingHero.Index < 0 || heroes[heroNewIndex].Hero.Ref != default) return;

                fightingHero.AttackTarget = attackTarget;
                fightingHero.TargetPositionX = targetPosition.x;
                fightingHero.TargetPositionY = targetPosition.y;
                Hero.SetNewBoardPosision(heroes, fightingHero, heroNewIndex);
            }
        }

        public static void SetHeroTarget(Frame f, FightingHero fighingHero, EntityRef attackTarget)
        {
            QList<FightingHero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            FightingHero fightingHero = heroes[fighingHero.Index];
            fightingHero.AttackTarget = attackTarget;
            heroes[fighingHero.Index] = fightingHero;
        }

        public static FightingHero GetClosestTarget(Frame f, List<FightingHero> heroes, FightingHero hero)
        {
            FP minDistance = FP.MaxValue;
            FightingHero closestHero = default;

            foreach (var target in heroes)
            {
                if (target.IsAlive == false || target.Hero.Ref == hero.Hero.Ref)
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

        public static bool TryGetRandomTarget(Frame f, FightingHero fightingHero, Board board, out FightingHero target)
        {
            List<FightingHero> targets = GetAllTargets(f, fightingHero, board);

            if (targets.Count == 0)
            {
                target = default;
                return false;
            }

            target = targets[f.RNG->Next(0, targets.Count)];
            return true;
        }

        public static List<FightingHero> GetAllTargetsInRange(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<Vector2Int> closeTiles = new();
            List<FightingHero> heroesList = new();

            GetCloseCords(fightingHero.Index, ref closeTiles, fightingHero.Hero.Range);

            foreach (var tile in closeTiles)
            {
                if (TryConvertCordsToIndex(tile, out int index) == false)
                {
                    continue;
                }

                if (heroes[index].IsAlive == false)
                {
                    continue;
                }

                if (fightingHero.TeamNumber == heroes[index].TeamNumber || heroes[index].IsAlive == false)
                {
                    continue;
                }

                heroesList.Add(heroes[index]);
            }

            return heroesList;
        }

        public static List<FightingHero> GetAllTargets(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<FightingHero> heroesList = new();

            foreach (FightingHero target in heroes)
            {
                if (target.IsAlive == false)
                {
                    continue;
                }

                if (fightingHero.TeamNumber == target.TeamNumber || target.IsAlive == false)
                {
                    continue;
                }

                heroesList.Add(target);
            }

            return heroesList;
        }
    }
}
