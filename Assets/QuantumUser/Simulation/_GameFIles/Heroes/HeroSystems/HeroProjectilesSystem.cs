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

            QList<Board> boards = BoardSystem.GetBoards(f);

            foreach (Board board in boards)
            {
                ProcessProjectiles(f, board);
            }
        }

        public static void SpawnProjectile(Frame f, FightingHero fighingHero, Board board, FightingHero targetHero, FP damage,
            HeroEffects.Effect[] effects, HeroEffects.GlobalEffect[] globalEffects,
            HeroAttack.DamageType damageType, HeroAttack.AttackType attackType)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            var projectilePrototype = attackType switch
            {
                HeroAttack.AttackType.Base => gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).ProjectilePrototype,
                HeroAttack.AttackType.Ability => gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).AbilityProjectilePrototype,
                _ => gameConfig.GetHeroInfo(f, fighingHero.Hero.ID).ProjectilePrototype,
            };

            QList<EffectQnt> effectsList;

            if (effects != null)
            {
                effectsList = f.AllocateList<EffectQnt>(effects.Length);

                foreach (HeroEffects.Effect effect in effects)
                {
                    EffectQnt effectQnt = new()
                    {
                        Owner = effect.Owner,
                        Index = (int)effect.Type,
                        Value = effect.Value,
                        Duration = effect.Duration,
                        Size = effect.Size,
                    };

                    effectsList.Add(effectQnt);
                }
            }
            else
            {
                effectsList = f.AllocateList<EffectQnt>();
            }

            QList<GlobalEffectQnt> globalEffectsList;

            if (globalEffects != null)
            {
                globalEffectsList = f.AllocateList<GlobalEffectQnt>(globalEffects.Length);

                foreach (HeroEffects.GlobalEffect effect in globalEffects)
                {
                    GlobalEffectQnt effectQnt = new()
                    {
                        Center = effect.Center,
                        Owner = effect.Owner,
                        Index = (int)effect.Type,
                        Value = effect.Value,
                        Duration = effect.Duration,
                        Size = effect.Size,
                    };

                    globalEffectsList.Add(effectQnt);
                }
            }
            else
            {
                globalEffectsList = f.AllocateList<GlobalEffectQnt>();
            }

            HeroProjectile projectile = new()
            {
                Ref = f.Create(projectilePrototype),
                Guid = f.FindAsset(projectilePrototype).Guid.Value,
                Target = targetHero.Hero.Ref,
                Owner = fighingHero.Hero.Ref,
                Damage = damage,
                TargetPosition = f.Get<Transform3D>(targetHero.Hero.Ref).Position,
                DamageType = (int)damageType,
                Speed = fighingHero.Hero.ProjectileSpeed,
                Level = fighingHero.Hero.Level,
                AttackType = (int)attackType,
                IsActive = true,
                Effects = effectsList,
                GlobalEffects = globalEffectsList,
            };

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

            if (f.Exists(projectile.Target))
            {
                Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(projectile.Target);
                projectile.TargetPosition = targetTransform->Position;
            }

            FP moveOffset = projectile.Speed * f.DeltaTime;
            projectileTransform->Position = FPVector3.MoveTowards(projectileTransform->Position, projectile.TargetPosition, moveOffset);

            if (projectileTransform->Position == projectile.TargetPosition)
            {
                FightingHero ownerHero = HeroBoard.GetFightingHero(f, projectile.Owner, board);
                FightingHero targetHero = HeroBoard.GetFightingHero(f, projectile.Target, board);
                HeroAttack.DamageHero(f, ownerHero, board, targetHero, projectile.Damage, f.ResolveList(projectile.Effects),
                    (HeroAttack.DamageType)projectile.DamageType, (HeroAttack.AttackType)projectile.AttackType);

                HeroEffects.AddGlobalEffects(f, board, f.ResolveList(projectile.GlobalEffects));

                f.FreeList(projectile.Effects);
                projectile.Effects = default;
                f.FreeList(projectile.GlobalEffects);
                projectile.GlobalEffects = default;

                f.Destroy(projectile.Ref);

                return true;
            }

            return false;
        }
    }
}
