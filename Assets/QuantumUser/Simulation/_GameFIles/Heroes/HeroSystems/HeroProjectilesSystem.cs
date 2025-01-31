using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroProjectilesSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            if (f.Global->IsBuyPhase || f.Global->IsDelayPassed == false) return;

            List<Board> boards = BoardSystem.GetBoards(f);

            foreach (Board board in boards)
            {
                ProcessProjectiles(f, board);
            }
        }

        public static void SpawnProjectile(Frame f, FightingHero fighingHero, FightingHero targetHero, FP damage,
            HeroAttack.DamageType damageType, HeroAttack.AttackType attackType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Board board = HeroBoard.GetBoard(f, fighingHero);
            
            var projectilePrototype = attackType switch
            {
                HeroAttack.AttackType.Base => gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).ProjectilePrototype,
                HeroAttack.AttackType.Ability => gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).AbilityProjectilePrototype,
                _ => gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).ProjectilePrototype,
            };

            HeroProjectile projectile = new()
            {
                Ref = f.Create(projectilePrototype),
                Target = targetHero,
                Owner = fighingHero,
                TargetPosition = f.Get<Transform3D>(targetHero.Hero.Ref).Position,
                DamageType = (int)damageType,
                Speed = fighingHero.Hero.ProjectileSpeed,
                Level = fighingHero.Hero.Level,
                AttackType = (int)attackType
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

            if (f.Exists(projectile.Target.Hero.Ref))
            {
                Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Target.Hero.Ref);
                projectile.TargetPosition = targetTransform->Position;
            }

            FP moveOffset = projectile.Speed * f.DeltaTime;
            projectileTransform->Position = FPVector3.MoveTowards(projectileTransform->Position, projectile.TargetPosition, moveOffset);

            if (projectileTransform->Position == projectile.TargetPosition)
            {
                HeroAttack.DamageHero(f, projectile.Owner, projectile.Target, (HeroAttack.DamageType)projectile.DamageType, (HeroAttack.AttackType)projectile.AttackType);
                f.ResolveList(board.HeroProjectiles).Remove(projectile);
                f.Destroy(projectile.Ref);
            }

            List<EntityLevelData> projectilesData = f.ResolveList(board.HeroProjectiles).Select(p => new EntityLevelData { Ref = p.Ref, Level = p.Level }).ToList();
            f.Events.GetProjectiles(f, board.Player1.Ref, board.Player2.Ref, projectilesData);
        }
    }
}
