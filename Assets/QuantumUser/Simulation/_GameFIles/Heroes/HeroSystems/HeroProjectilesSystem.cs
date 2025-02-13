using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroProjectilesSystem : SystemMainThread, ISignalOnEndRound
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

        public void OnEndRound(Frame f)
        {
            QList<HeroProjectile> heroProjectiles = f.ResolveList(f.Global->ProjectilesPool);

            for (int i = 0; i < heroProjectiles.Count; i++)
            {
                HeroProjectile projectile = heroProjectiles[i];
                projectile.IsActive = false;
                heroProjectiles[i] = projectile;

                if (f.Exists(projectile.Target.Hero.Ref))
                {
                    Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Target.Hero.Ref);
                    targetTransform->Position = FPVector3.MaxValue;
                    targetTransform->Teleport(f, FPVector3.MaxValue);
                }
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

            QList<HeroProjectile> projectilePool = f.ResolveList(f.Global->ProjectilesPool);
            HeroProjectile projectile = default;
            int projectileIndex = -1;

            for (int i = 0; i < projectilePool.Count; i++)
            {
                HeroProjectile pooledProjectile = projectilePool[i];

                if (pooledProjectile.IsActive == false &&
                    pooledProjectile.Guid == f.FindAsset(projectilePrototype).Guid.Value)
                {
                    projectile = pooledProjectile;
                    projectileIndex = i;
                    break;
                }
            }

            if (projectileIndex >= 0)
            {
                projectile.Target = targetHero;
                projectile.Owner = fighingHero;
                projectile.TargetPosition = f.Get<Transform3D>(targetHero.Hero.Ref).Position;
                projectile.DamageType = (int)damageType;
                projectile.Speed = fighingHero.Hero.ProjectileSpeed;
                projectile.Level = fighingHero.Hero.Level;
                projectile.AttackType = (int)attackType;
                projectile.IsActive = true;
                projectilePool[projectileIndex] = projectile;
            }
            else
            {
                projectile = new()
                {
                    Ref = f.Create(projectilePrototype),
                    Guid = f.FindAsset(projectilePrototype).Guid.Value,
                    Target = targetHero,
                    Owner = fighingHero,
                    TargetPosition = f.Get<Transform3D>(targetHero.Hero.Ref).Position,
                    DamageType = (int)damageType,
                    Speed = fighingHero.Hero.ProjectileSpeed,
                    Level = fighingHero.Hero.Level,
                    AttackType = (int)attackType,
                    IsActive = true,
                };

                projectilePool.Add(projectile);
            }

            Transform3D* projectileTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Ref);
            FPVector3 projectilePosition = f.Get<Transform3D>(fighingHero.Hero.Ref).Position;
            projectileTransform->Position = projectilePosition;
            projectileTransform->Teleport(f, projectilePosition);
            QList<HeroProjectile> heroProjectiles = f.ResolveList(board.HeroProjectiles);
            heroProjectiles.Add(projectile);

            BoardSystem.DisactiveEntity(f, projectile.Ref);
            Events.ActiveEntity(f, board, projectile.Ref, new EntityLevelData() { Ref = projectile.Ref, Level = projectile.Level });
        }

        public static void ProcessProjectiles(Frame f, Board board)
        {
            QList<HeroProjectile> projectiles = f.ResolveList(board.HeroProjectiles);

            for (int i = 0; i < projectiles.Count; i++)
            {
                if (ProcessProjectile(f, board, projectiles[i]))
                {
                    projectiles.Remove(projectiles[i]);
                    i--;
                }
            }
        }

        private static bool ProcessProjectile(Frame f, Board board, HeroProjectile projectile)
        {
            if (f.Exists(projectile.Ref) == false) return false;

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
                Events.DisactiveEntity(f, board, projectile.Ref);

                QList<HeroProjectile> projectilePool = f.ResolveList(f.Global->ProjectilesPool);

                for (int i = 0; i < projectilePool.Count; i++)
                {
                    HeroProjectile pooledProjectile = projectilePool[i];

                    if (pooledProjectile.Ref == projectile.Ref)
                    {
                        pooledProjectile.IsActive = false;
                        projectilePool[i] = pooledProjectile;
                        break;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
