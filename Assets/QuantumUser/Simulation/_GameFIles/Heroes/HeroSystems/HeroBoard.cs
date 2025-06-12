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
            if (f.Exists(entityRef) == false)
            {
                return default;
            }

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
            position -= tileSize * new FPVector3(GameplayConstants.BoardSize, 0, GameplayConstants.BoardSize) / 2;
            position += new FPVector3(tileSize, 0, tileSize) / 2;

            return position;
        }

        public static bool TryGetHeroCordsFromIndex(int index, out Vector2Int cords)
        {
            if (index < 0 || index >= GameplayConstants.BoardSize * GameplayConstants.BoardSize)
            {
                cords = default;
                return false;
            }

            cords = new Vector2Int(index % GameplayConstants.BoardSize, index / GameplayConstants.BoardSize);

            return cords.x >= 0 && cords.x < GameplayConstants.BoardSize && cords.y >= 0 && cords.y < GameplayConstants.BoardSize;
        }

        public static bool TryGetHeroIndexFromCords(Vector2Int cords, out int index)
        {
            if (cords.x < 0 || cords.x >= GameplayConstants.BoardSize ||
                cords.y < 0 || cords.y >= GameplayConstants.BoardSize)
            {
                index = -1;
                return false;
            }

            index = cords.x + cords.y * GameplayConstants.BoardSize;

            return index >= 0 && index < GameplayConstants.BoardSize * GameplayConstants.BoardSize;
        }

        public static List<Vector2Int> GetCloseCords(int heroIndex, int range = 1, bool includeSelf = false)
        {
            List<Vector2Int> closeTiles = new();

            if (TryGetHeroCordsFromIndex(heroIndex, out Vector2Int cords) == false)
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

                    if (newCords.x < 0 || newCords.x >= GameplayConstants.BoardSize)
                    {
                        continue;
                    }

                    if (newCords.y < 0 || newCords.y >= GameplayConstants.BoardSize)
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
            if (TryGetHeroCordsFromIndex(heroIndex, out Vector2Int cords) == false)
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

                if (newCords.x < 0 || newCords.x >= GameplayConstants.BoardSize)
                {
                    continue;
                }

                if (newCords.y < 0 || newCords.y >= GameplayConstants.BoardSize)
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
            if (HeroEffects.TryProcessTauntEffect(f, fightingHero, board, out FightingHero tauntHero, out moveTargetPosition))
            {
                return tauntHero;
            }

            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero targetHero))
            {
                moveTargetPosition = GetHeroCords(fightingHero);
                return targetHero;
            }

            FightingHero hero = HeroAttack.FindClosestTargetOutOfAttackRange(f, fightingHero, board, out moveTargetPosition);
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
            
            if (attackTarget != default && f.Exists(attackTarget))
            {
                Hero.Rotate(f, fightingHero.Hero, f.Get<Transform3D>(attackTarget).Position);
            }
        }

        public static int[,] GetBoardMap(QList<FightingHero> heroes)
        {
            int[,] board = new int[GameplayConstants.BoardSize, GameplayConstants.BoardSize];

            for (int x = 0; x < GameplayConstants.BoardSize; x++)
            {
                for (int y = 0; y < GameplayConstants.BoardSize; y++)
                {
                    if (TryGetHeroIndexFromCords(new Vector2Int(x, y), out int index))
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

            return board;
        }

        public static FightingHero GetAliyWithMinHealth(List<FightingHero> alies)
        {
            FightingHero alyWithMinHealth = default;
            FP minHealth = FP.MaxValue;

            foreach (FightingHero aly in alies)
            {
                FP health = aly.CurrentHealth;

                if (health < minHealth)
                {
                    minHealth = health;
                    alyWithMinHealth = aly;
                }
            }

            return alyWithMinHealth;
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

                // Проверяем существование сущностей перед обращением к ним
                if (!f.Exists(target.Hero.Ref) || !f.Exists(hero.Hero.Ref))
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

        public static bool IsHeroInRange(FightingHero fightingHero, int target, int range)
        {
            if (TryGetHeroCordsFromIndex(target, out Vector2Int targetCords) == false)
            {
                return false;
            }

            if (TryGetHeroCordsFromIndex(fightingHero.Index, out Vector2Int heroCords) == false)
            {
                return false;
            }

            return PathFinder.IsTargetPositionInRange(heroCords, targetCords, range);
        }

        public static List<FightingHero> GetAllTargetsInRange(Frame f, FightingHero fightingHero, Board board)
        {
            return GetAllTeamHeroesInRange(f, fightingHero.Index, GetEnemyTeamNumber(fightingHero.TeamNumber), board, fightingHero.Hero.Range);
        }

        public static List<FightingHero> GetAllAliesInRange(Frame f, FightingHero fightingHero, Board board, bool includeSelf = false)
        {
            return GetAllTeamHeroesInRange(f, fightingHero.Index, fightingHero.TeamNumber, board, fightingHero.Hero.Range, includeSelf);
        }

        public static List<FightingHero> GetAllTeamHeroesInRange(Frame f, int center, int hisTeam, Board board, int range, bool includeCenter = false)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<Vector2Int> closeTiles = GetCloseCords(center, range, includeCenter);
            List<FightingHero> heroesList = new();

            foreach (var tile in closeTiles)
            {
                if (TryGetHeroIndexFromCords(tile, out int index) == false)
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

            if (TryGetHeroCordsFromIndex(center, out Vector2Int startTile) == false)
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
                if (TryGetHeroIndexFromCords(tile, out int index) == false)
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
                if (TryGetHeroIndexFromCords(tile, out int index) == false)
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
