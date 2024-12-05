using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEditor.VersionControl;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroProjectilesSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            if (f.Global->IsBuyPhase || f.Global->IsDelayPassed == false) return;

            QList<Board> boards = f.ResolveList(f.Global->Boards);

            foreach (Board board in boards)
            {
                ProcessProjectiles(f, board);
            }
        }

        public static void SpawnProjectile(Frame f, FightingHero fighingHero, HeroEntity targetHero, FP damage, 
            HeroAttack.DamageType damageType, HeroAttack.ProjectileType projectileType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Board board = HeroBoard.GetBoard(f, fighingHero);

            AssetRef<EntityPrototype> projectilePrototype;

            switch (projectileType)
            {
                case HeroAttack.ProjectileType.Attack:
                    projectilePrototype = gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).ProjectilePrototype;
                    break;
                case HeroAttack.ProjectileType.Ability:
                    projectilePrototype = gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).AbilityProjectilePrototype;
                    break;
                default:
                    projectilePrototype = gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).ProjectilePrototype;
                    break;
            }

            HeroProjectile projectile = new()
            {
                Ref = f.Create(projectilePrototype),
                Target = targetHero,
                Damage = damage,
                DamageType = (int)damageType,
                Speed = fighingHero.Hero.ProjectileSpeed,
                Level = fighingHero.Hero.Level
            };

            Transform3D* projectileTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Ref);
            projectileTransform->Position = f.Get<Transform3D>(fighingHero.Hero.Ref).Position;
            QList<HeroProjectile> heroProjectiles = f.ResolveList(board.HeroProjectiles);
            heroProjectiles.Add(projectile);

            BoardSystem.DisactiveEntity(f, projectile.Ref);

            List<EntityLevelData> projectilesData = heroProjectiles.Select(p => new EntityLevelData { Ref = p.Ref, Level = p.Level }).ToList();
            f.Events.GetProjectiles(f, board.Player1.Ref, board.Player2.Ref, projectilesData);
        }

        public static void ProcessProjectiles(Frame f, Board board)
        {
            QList<HeroProjectile> projectiles = f.ResolveList(board.HeroProjectiles);

            foreach (HeroProjectile projectile in projectiles)
            {
                ProcessProjectile(f, board, projectile);
            }
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
                HeroAttack.DamageHero(f, board, projectile.Damage, projectile.Target, (HeroAttack.DamageType)projectile.DamageType);
                f.ResolveList(board.HeroProjectiles).Remove(projectile);
                f.Destroy(projectile.Ref);
            }

            List<EntityLevelData> projectilesData = f.ResolveList(board.HeroProjectiles).Select(p => new EntityLevelData { Ref = p.Ref, Level = p.Level }).ToList();
            f.Events.GetProjectiles(f, board.Player1.Ref, board.Player2.Ref, projectilesData);
        }
    }
}
