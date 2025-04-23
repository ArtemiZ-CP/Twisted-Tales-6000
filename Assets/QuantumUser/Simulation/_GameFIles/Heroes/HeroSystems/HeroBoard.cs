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
            QList<Board> boards = BoardSystem.GetBoards(f);
            return boards[fighingHero.BoardIndex];
        }

        public static FightingHero GetFightingHero(Frame f, EntityRef entityRef, Board board)
        {
            QList<FightingHero> fightingHeroes = f.ResolveList(board.FightingHeroesMap);

            foreach (FightingHero fightingHero in fightingHeroes)
            {
                if (fightingHero.Hero.Ref == entityRef)
                {
                    return fightingHero;
                }
            }

            return default;
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

        public static List<Vector2Int> GetCloseCords(int heroIndex, int range = 1, bool includeSelf = false)
        {
            List<Vector2Int> closeTiles = new();

            if (TryGetHeroCords(heroIndex, out Vector2Int cords) == false)
            {
                return closeTiles;
            }

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector2Int newCords = cords + new Vector2Int(x, y);

                    if (includeSelf == false && newCords == cords)
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

            return closeTiles;
        }

        public static void GetHorizontalCloseCords(int heroIndex, ref List<Vector2Int> closeTiles, int range = 1, bool includeSelf = false)
        {
            if (TryGetHeroCords(heroIndex, out Vector2Int cords) == false)
            {
                return;
            }

            for (int x = -range; x <= range; x++)
            {
                Vector2Int newCords = cords + new Vector2Int(x, 0);

                if (includeSelf == false && newCords == cords)
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

        public static bool TrySetTarget(Frame f, ref FightingHero fightingHero, Board board)
        {
            FightingHero target = GetHeroTarget(f, fightingHero, board, out Vector2Int moveTargetPosition);

            if (target.Hero.Ref == default)
            {
                return false;
            }

            if (moveTargetPosition == GetHeroCords(fightingHero))
            {
                SetHeroTarget(f, fightingHero, board, target.Hero.Ref);
                return true;
            }

            SetHeroTarget(f, ref fightingHero, board, target.Hero.Ref, moveTargetPosition);
            return false;
        }

        public static FightingHero GetHeroTarget(Frame f, FightingHero fightingHero, Board board, out Vector2Int moveTargetPosition)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero targetHero))
            {
                moveTargetPosition = GetHeroCords(fightingHero);
                return targetHero;
            }

            FightingHero hero = HeroAttack.FindClosestTargetOutOfAttackRange(f, fightingHero, board, out moveTargetPosition, out _);
            return hero;
        }

        public static void SetHeroTarget(Frame f, ref FightingHero fightingHero, Board board, EntityRef attackTarget, Vector2Int targetPosition)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            if (Hero.TrySetNewBoardPosition(heroes, ref fightingHero, targetPosition))
            {
                fightingHero.AttackTarget = attackTarget;
                heroes[fightingHero.Index] = fightingHero;
            }
        }

        public static void SetHeroTarget(Frame f, FightingHero fighingHero, Board board, EntityRef attackTarget)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

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

                FP distance = FPVector3.DistanceSquared(targetTransform->Position, heroTransform->Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHero = target;
                }
            }

            return closestHero;
        }

        public static List<FightingHero> GetAllTargetsInRange(Frame f, FightingHero fightingHero, Board board)
        {
            return GetAllTeamHeroesInRange(f, fightingHero.Index, GetEnemyTeamNumber(fightingHero.TeamNumber), board, fightingHero.Hero.Range);
        }

        public static List<FightingHero> GetAllAliesInRange(Frame f, FightingHero fightingHero, Board board)
        {
            return GetAllTeamHeroesInRange(f, fightingHero.Index, fightingHero.TeamNumber, board, fightingHero.Hero.Range);
        }

        public static List<FightingHero> GetAllTeamHeroesInRange(Frame f, int center, int hisTeam, Board board, int range, bool includeSelf = false)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<Vector2Int> closeTiles = GetCloseCords(center, range, includeSelf);
            List<FightingHero> heroesList = new();

            foreach (var tile in closeTiles)
            {
                if (TryConvertCordsToIndex(tile, out int index) == false)
                {
                    continue;
                }

                FightingHero hero = heroes[index];

                if (hero.IsAlive == false)
                {
                    continue;
                }

                if (hisTeam != hero.TeamNumber)
                {
                    continue;
                }

                heroesList.Add(hero);
            }

            return heroesList;
        }

        public static bool TryGetCloseEmptyTileInRange(Frame f, int center, Board board, int range, out Vector2Int emptyTile, bool includeSelf = false)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<Vector2Int> closeTiles = GetCloseCords(center, range, includeSelf);

            if (TryConvertIndexToCords(center, out Vector2Int startTile) == false)
            {
                emptyTile = default;
                return false;
            }
            
            closeTiles.Sort((a, b) =>
            {
                FP distanceA = Mathf.Abs(a.x - startTile.x) + Mathf.Abs(a.y - startTile.y);
                FP distanceB = Mathf.Abs(b.x - startTile.x) + Mathf.Abs(b.y - startTile.y);
                return distanceA.CompareTo(distanceB);
            });

            foreach (Vector2Int tile in closeTiles)
            {
                if (TryConvertCordsToIndex(tile, out int index) == false)
                {
                    continue;
                }

                FightingHero hero = heroes[index];

                if (hero.IsAlive == false)
                {
                    emptyTile = tile;
                    return true;
                }
            }

            emptyTile = default;
            return false;
        }

        public static List<FightingHero> GetAllTeamHeroesInHorizontalRange(Frame f, int center, int hisTeam, Board board, int range, bool includeSelf = false)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<Vector2Int> closeTiles = new();
            List<FightingHero> heroesList = new();

            GetHorizontalCloseCords(center, ref closeTiles, range, includeSelf);

            foreach (var tile in closeTiles)
            {
                if (TryConvertCordsToIndex(tile, out int index) == false)
                {
                    continue;
                }

                FightingHero hero = heroes[index];

                if (hero.IsAlive == false)
                {
                    continue;
                }

                if (hisTeam != hero.TeamNumber)
                {
                    continue;
                }

                heroesList.Add(hero);
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

                if (fightingHero.TeamNumber == target.TeamNumber)
                {
                    continue;
                }

                heroesList.Add(target);
            }

            return heroesList;
        }

        public static int GetEnemyTeamNumber(int teamNumber)
        {
            if (teamNumber == GameplayConstants.Team1)
            {
                return GameplayConstants.Team2;
            }
            else if (teamNumber == GameplayConstants.Team2)
            {
                return GameplayConstants.Team1;
            }

            return -1;
        }
    }
}
