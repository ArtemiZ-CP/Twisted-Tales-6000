using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using System.Linq;
using System.Collections.Generic;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BaseHeroFightingSystem : SystemMainThreadGroup
    {
        public struct FighingHero
        {
            public Hero Hero;
            public int BoardIndex;
        }

        public BaseHeroFightingSystem() : base(nameof(BaseHeroFightingSystem))
        {
        }

        public BaseHeroFightingSystem(string name, params SystemMainThread[] children) : base(name, children)
        {
        }

        public static FighingHero ProcessReload(Frame f, FighingHero fighingHero)
        {
            if (fighingHero.Hero.AttackTarget == default)
            {
                return fighingHero;
            }

            QList<Hero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);
            int index = heroes.IndexOf(fighingHero.Hero);

            if (index < 0)
            {
                return fighingHero;
            }

            Hero hero = heroes[index];
            hero.AttackTimer -= f.DeltaTime;
            heroes[index] = hero;
            fighingHero.Hero = hero;

            return fighingHero;
        }

        public static void ProcessInstantAttack(Frame f, FighingHero fighingHero)
        {
            if (IsAbleToAttack(f, fighingHero, out Hero targetHero) == false)
            {
                return;
            }

            DamageHero(f, GetBoard(f, fighingHero), fighingHero.Hero.Damage, targetHero);
            ResetAttackTimer(f, fighingHero);
        }

        public static void ProcessProjectiles(Frame f, Board board)
        {
            QList<HeroProjectile> projectiles = f.ResolveList(board.HeroProjectiles);

            foreach (HeroProjectile projectile in projectiles)
            {
                ProcessProjectile(f, board, projectile);
            }
        }

        public static void ProcessProjectileAttack(Frame f, FighingHero fighingHero)
        {
            if (IsAbleToAttack(f, fighingHero, out Hero targetHero) == false)
            {
                return;
            }

            SpawnProjectile(f, fighingHero, targetHero);
            ResetAttackTimer(f, fighingHero);
        }

        public static void SetHeroTarget(Frame f, FighingHero fighingHero, EntityRef attackTarget, Vector2Int targetPosition)
        {
            QList<Hero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            if (BoardPosition.TryConvertCordsToIndex(f, targetPosition, out int heroNewIndex))
            {
                int heroLastIndex = heroes.IndexOf(fighingHero.Hero);

                if (heroLastIndex < 0 || heroes[heroNewIndex].Ref != default) return;

                Hero hero = heroes[heroLastIndex];
                hero.AttackTarget = attackTarget;
                hero.TargetPositionX = targetPosition.x;
                hero.TargetPositionY = targetPosition.y;
                Hero empty = default;
                empty.ID = -1;
                heroes[heroLastIndex] = empty;
                heroes[heroNewIndex] = hero;
            }
        }

        public static void SetHeroTarget(Frame f, FighingHero fighingHero, EntityRef attackTarget)
        {
            QList<Hero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            int index = heroes.IndexOf(fighingHero.Hero);
            Hero hero = heroes[index];
            hero.AttackTarget = attackTarget;
            heroes[index] = hero;
        }

        public static void GetHeroes<T>(Frame f, ref List<FighingHero> heroes) where T : unmanaged, IComponent
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);

            foreach (Board board in boards)
            {
                QList<Hero> fightingHeroes = f.ResolveList(board.FightingHeroesMap);

                foreach (Hero hero in fightingHeroes)
                {
                    if (hero.Ref == default || hero.IsAlive == false)
                    {
                        continue;
                    }

                    if (f.Unsafe.TryGetPointer(hero.Ref, out T* _))
                    {
                        heroes.Add(new FighingHero
                        {
                            Hero = hero,
                            BoardIndex = boards.IndexOf(board)
                        });
                    }
                }
            }
        }

        public static bool IsHeroMoving(Frame f, Hero hero)
        {
            Transform3D transform = f.Get<Transform3D>(hero.Ref);
            FPVector3 position = BoardPosition.GetHeroPosition(f, hero);

            return transform.Position != position;
        }

        public static Hero GetHeroTarget(Frame f, FighingHero fighingHero, out Vector2Int moveTargetPosition)
        {
            QList<Hero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            if (TryFindClosestTargetInAttackRange(f, heroes, fighingHero.Hero, out Hero targetHero))
            {
                moveTargetPosition = BoardPosition.GetHeroCords(fighingHero.Hero);
                return targetHero;
            }

            Hero hero = FindClosestTargetOutOfAttackRange(f, heroes, fighingHero.Hero, out moveTargetPosition, out bool inRange);

            return hero;
        }

        public static FPVector3 GetTilePosition(Frame f, int index)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            int x = index % GameConfig.BoardSize;
            int y = index / GameConfig.BoardSize;
            FP tileSize = FP.FromFloat_UNSAFE(config.TileSize);

            FPVector3 position = new FPVector3(x, 0, y) * tileSize;
            position -= tileSize * new FPVector3(GameConfig.BoardSize, 0, GameConfig.BoardSize) / 2;
            position += new FPVector3(tileSize, 0, tileSize) / 2;

            return position;
        }

        public static Board GetBoard(Frame f, FighingHero fighingHero)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);
            return boards[fighingHero.BoardIndex];
        }

        private static void SpawnProjectile(Frame f, FighingHero fighingHero, Hero targetHero)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Hero hero = fighingHero.Hero;
            Board board = GetBoard(f, fighingHero);

            HeroProjectile projectile = new()
            {
                Ref = f.Create(gameConfig.GetHeroInfo(f, hero.ID).ProjectilePrototype),
                Target = targetHero,
                Damage = hero.Damage,
                Speed = hero.ProjectileSpeed,
                Level = hero.Level
            };

            Transform3D* projectileTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Ref);
            projectileTransform->Position = f.Get<Transform3D>(hero.Ref).Position;
            QList<HeroProjectile> heroProjectiles = f.ResolveList(board.HeroProjectiles);
            heroProjectiles.Add(projectile);

            BoardSystem.DisactiveEntity(f, projectile.Ref);

            List<EntityLevelData> projectilesData = heroProjectiles.Select(p => new EntityLevelData { Ref = p.Ref, Level = p.Level }).ToList();
            f.Events.GetProjectiles(f, board.Player1.Ref, board.Player2.Ref, projectilesData);
        }

        private static void ProcessProjectile(Frame f, Board board, HeroProjectile projectile)
        {
            Transform3D* projectileTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Ref);
            Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Target.Ref);
            FP moveOffset = projectile.Speed * f.DeltaTime;

            projectileTransform->Position = FPVector3.MoveTowards(projectileTransform->Position, targetTransform->Position, moveOffset);

            if (projectileTransform->Position == targetTransform->Position)
            {
                DamageHero(f, board, projectile.Damage, projectile.Target);
                f.ResolveList(board.HeroProjectiles).Remove(projectile);
                f.Destroy(projectile.Ref);
            }

            List<EntityLevelData> projectilesData = f.ResolveList(board.HeroProjectiles).Select(p => new EntityLevelData { Ref = p.Ref, Level = p.Level }).ToList();
            f.Events.GetProjectiles(f, board.Player1.Ref, board.Player2.Ref, projectilesData);
        }

        private static void DamageHero(Frame f, Board board, int damage, Hero targetHero)
        {
            int targetHeroIndex = f.ResolveList(board.FightingHeroesMap).ToList().FindIndex(hero => hero.Ref == targetHero.Ref);

            if (targetHeroIndex < 0)
            {
                return;
            }

            DamageHero(f, board, damage, targetHeroIndex);
        }

        private static void ResetAttackTimer(Frame f, FighingHero fighingHero)
        {
            Board board = GetBoard(f, fighingHero);
            QList<Hero> heroes = f.ResolveList(board.FightingHeroesMap);
            int heroIndex = heroes.ToList().FindIndex(hero => hero.Ref == fighingHero.Hero.Ref);
            Hero hero = heroes[heroIndex];
            hero.AttackTimer = 1 / hero.AttackSpeed;
            heroes[heroIndex] = hero;
        }

        private static void DamageHero(Frame f, Board board, int damage, int targetHeroIndex)
        {
            QList<Hero> heroes = f.ResolveList(board.FightingHeroesMap);
            Hero target = heroes[targetHeroIndex];

            target.CurrentHealth -= damage;

            if (target.CurrentHealth <= 0)
            {
                f.Events.DestroyHero(board.Player1.Ref, board.Player2.Ref, target.Ref);
                target.IsAlive = false;
                target.ID = -1;
                target.Ref = default;
            }
            else
            {
                f.Events.HeroHealthChanged(board.Player1.Ref, board.Player2.Ref, target.Ref, target.CurrentHealth, target.Health);
            }

            heroes[targetHeroIndex] = target;
        }

        private static bool IsAbleToAttack(Frame f, FighingHero fighingHero, out Hero targetHero)
        {
            targetHero = default;

            if (fighingHero.Hero.AttackTarget == default || fighingHero.Hero.AttackTimer > 0)
            {
                return false;
            }

            QList<Hero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            int heroIndex = heroes.ToList().FindIndex(hero => hero.Ref == fighingHero.Hero.Ref);

            if (heroIndex < 0)
            {
                return false;
            }

            targetHero = heroes.ToList().Find(hero => hero.Ref == fighingHero.Hero.AttackTarget);

            if (targetHero.Ref == default)
            {
                return false;
            }

            Hero target = targetHero;
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);
            Transform3D targetTransform = f.Get<Transform3D>(target.Ref);
            FP targetDistanceToCell = FPVector3.Distance(targetTransform.Position, BoardPosition.GetHeroPosition(f, target));

            if (fighingHero.Hero.RangePercentage * tileSize < targetDistanceToCell)
            {
                return false;
            }

            return true;
        }

        private static bool TryFindClosestTargetInAttackRange(Frame f, QList<Hero> heroes, Hero hero, out Hero targetHero)
        {
            List<Vector2Int> closeTiles = new();
            List<Hero> heroesList = new();

            GetCloseCords(f, heroes, hero, ref closeTiles, hero.Range);

            foreach (var tile in closeTiles)
            {
                if (BoardPosition.TryConvertCordsToIndex(f, tile, out int index) == false)
                {
                    continue;
                }

                if (heroes[index].ID < 0)
                {
                    continue;
                }

                if (hero.TeamNumber == heroes[index].TeamNumber || heroes[index].IsAlive == false)
                {
                    continue;
                }

                heroesList.Add(heroes[index]);
            }

            targetHero = GetClosestTarget(f, heroesList, hero);

            if (targetHero.Ref != default)
            {
                return true;
            }

            return false;
        }

        private static Hero FindClosestTargetOutOfAttackRange(Frame f, QList<Hero> heroes, Hero hero, out Vector2Int moveTargetPosition, out bool inRange)
        {
            List<Hero> heroesList = new();

            foreach (Hero target in heroes)
            {
                if (target.Ref == default)
                {
                    continue;
                }

                if (hero.TeamNumber == target.TeamNumber || target.IsAlive == false)
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
                Hero targetHero = GetClosestTarget(f, heroesList, hero);

                if (targetHero.Ref == default)
                {
                    continue;
                }

                int[,] board = new int[GameConfig.BoardSize, GameConfig.BoardSize];

                for (int x = 0; x < GameConfig.BoardSize; x++)
                {
                    for (int y = 0; y < GameConfig.BoardSize; y++)
                    {
                        if (BoardPosition.TryConvertCordsToIndex(f, new Vector2Int(x, y), out int index))
                        {
                            int heroID = -1;

                            if (heroes[index].IsAlive && heroes[index].Ref != default)
                            {
                                heroID = heroes[index].ID;
                            }

                            board[x, y] = heroID;
                        }
                    }
                }

                if (PathFinder.TryFindPath(board, BoardPosition.GetHeroCords(hero),
                    BoardPosition.GetHeroCords(targetHero), hero.Range, out moveTargetPosition, out inRange))
                {
                    return targetHero;
                }

                heroesList.Remove(targetHero);
            }

            moveTargetPosition = default;
            inRange = false;
            return default;
        }

        private static Hero GetClosestTarget(Frame f, List<Hero> heroes, Hero hero)
        {
            FP minDistance = FP.MaxValue;
            Hero closestHero = default;

            foreach (var target in heroes)
            {
                if (target.ID < 0 || target.Ref == hero.Ref)
                {
                    continue;
                }

                Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(target.Ref);
                Transform3D* heroTransform = f.Unsafe.GetPointer<Transform3D>(hero.Ref);

                FP distance = FPVector3.Distance(targetTransform->Position, heroTransform->Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHero = target;
                }
            }

            return closestHero;
        }

        private static void GetCloseCords(Frame f, QList<Hero> heroes, Hero hero, ref List<Vector2Int> closeTiles, int range = 1)
        {
            if (BoardPosition.TryGetHeroCords(f, heroes, hero, out Vector2Int cords) == false)
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
    }
}
