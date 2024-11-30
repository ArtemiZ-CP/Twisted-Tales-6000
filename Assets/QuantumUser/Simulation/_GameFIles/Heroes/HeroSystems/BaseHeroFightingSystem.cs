using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BaseHeroFightingSystem : SystemMainThreadGroup
    {
        public BaseHeroFightingSystem() : base(nameof(BaseHeroFightingSystem))
        {
        }

        public BaseHeroFightingSystem(string name, params SystemMainThread[] children) : base(name, children)
        {
        }

        public static void ProcessReload(Frame f, FightingHero fighingHero)
        {
            QList<FightingHero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            FightingHero fightingHero = heroes[fighingHero.Index];
            fightingHero.Hero.AttackTimer -= f.DeltaTime;
            heroes[fighingHero.Index] = fightingHero;
        }

        public static void ProcessInstantAttack(Frame f, FightingHero fighingHero)
        {
            if (BoardPosition.IsAbleToAttack(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            DamageHero(f, GetBoard(f, fighingHero), fighingHero.Hero.Damage, targetHero.Hero);
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

        public static void ProcessProjectileAttack(Frame f, FightingHero fighingHero)
        {
            if (BoardPosition.IsAbleToAttack(f, fighingHero, out FightingHero targetHero) == false)
            {
                return;
            }

            SpawnProjectile(f, fighingHero, targetHero.Hero);
            ResetAttackTimer(f, fighingHero);
        }

        public static void SetHeroTarget(Frame f, FightingHero fighingHero, EntityRef attackTarget, Vector2Int targetPosition)
        {
            QList<FightingHero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            if (BoardPosition.TryConvertCordsToIndex(targetPosition, out int heroNewIndex))
            {
                if (fighingHero.Index < 0 || heroes[heroNewIndex].Hero.Ref != default) return;

                FightingHero fightingHero = heroes[fighingHero.Index];
                fightingHero.Hero.AttackTarget = attackTarget;
                fightingHero.Hero.TargetPositionX = targetPosition.x;
                fightingHero.Hero.TargetPositionY = targetPosition.y;
                FightingHero empty = heroes[heroNewIndex];
                empty.Hero.ID = -1;
                empty.Index = fighingHero.Index;
                heroes[fighingHero.Index] = empty;
                fightingHero.Index = heroNewIndex;
                heroes[heroNewIndex] = fightingHero;
            }
        }

        public static void SetHeroTarget(Frame f, FightingHero fighingHero, EntityRef attackTarget)
        {
            QList<FightingHero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            FightingHero fightingHero = heroes[fighingHero.Index];
            fightingHero.Hero.AttackTarget = attackTarget;
            heroes[fighingHero.Index] = fightingHero;
        }

        public static void UpdateHeroes<T>(Frame f, Action<Frame, FightingHero> UpdateHero) where T : unmanaged, IComponent
        {
            if (f.Global->IsBuyPhase || f.Global->IsDelayPassed == false || f.Global->IsFighting == false) return;

            List<FightingHero> heroesPtr = new();
            
            if (TryGetHeroes<T>(f, ref heroesPtr))
            {
                foreach (var fightingHero in heroesPtr)
                {
                    UpdateHero(f, fightingHero);
                }
            }
        }

        public static bool IsHeroMoving(Frame f, Hero hero)
        {
            Transform3D transform = f.Get<Transform3D>(hero.Ref);
            FPVector3 position = BoardPosition.GetHeroPosition(f, hero);

            return transform.Position != position;
        }

        public static FightingHero GetHeroTarget(Frame f, FightingHero fighingHero, out Vector2Int moveTargetPosition)
        {
            QList<FightingHero> heroes = f.ResolveList(GetBoard(f, fighingHero).FightingHeroesMap);

            if (BoardPosition.TryFindClosestTargetInAttackRange(f, heroes, fighingHero, out FightingHero targetHero))
            {
                moveTargetPosition = BoardPosition.GetHeroCords(fighingHero.Hero);
                return targetHero;
            }

            FightingHero hero = BoardPosition.FindClosestTargetOutOfAttackRange(f, heroes, fighingHero, out moveTargetPosition, out bool inRange);

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

        public static Board GetBoard(Frame f, FightingHero fighingHero)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);
            return boards[fighingHero.BoardIndex];
        }

        private static void SpawnProjectile(Frame f, FightingHero fighingHero, Hero targetHero)
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

        private static bool TryGetHeroes<T>(Frame f, ref List<FightingHero> heroes) where T : unmanaged, IComponent
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);

            foreach (Board board in boards)
            {
                QList<FightingHero> fightingHeroes = f.ResolveList(board.FightingHeroesMap);

                foreach (FightingHero fightingHero in fightingHeroes)
                {
                    if (fightingHero.Hero.Ref == default || fightingHero.Hero.IsAlive == false)
                    {
                        continue;
                    }

                    if (f.Unsafe.TryGetPointer(fightingHero.Hero.Ref, out T* _))
                    {
                        heroes.Add(fightingHero);
                    }
                }
            }

            if (heroes.Count > 0)
            {
                return true;
            }

            return false;
        }

        private static void ProcessProjectile(Frame f, Board board, HeroProjectile projectile)
        {
            if (f.Exists(projectile.Ref) == false) return;

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

        private static void ResetAttackTimer(Frame f, FightingHero fighingHero)
        {
            Board board = GetBoard(f, fighingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero hero = heroes[fighingHero.Index];
            hero.Hero.AttackTimer = 1 / hero.Hero.AttackSpeed;
            heroes[fighingHero.Index] = hero;
        }

        private static void DamageHero(Frame f, Board board, int damage, Hero targetHero)
        {
            int targetHeroIndex = f.ResolveList(board.FightingHeroesMap).ToList().FindIndex(hero => hero.Hero.Ref == targetHero.Ref);

            if (targetHeroIndex < 0)
            {
                return;
            }

            DamageHero(f, board, damage, targetHeroIndex);
        }

        private static void DamageHero(Frame f, Board board, int damage, int targetHeroIndex)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero target = heroes[targetHeroIndex];

            target.Hero.CurrentHealth -= damage;

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
