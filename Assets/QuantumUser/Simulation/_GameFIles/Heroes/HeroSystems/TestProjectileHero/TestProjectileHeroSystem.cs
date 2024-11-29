using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe class TestProjectileHeroSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            if (f.Global->IsBuyPhase || f.Global->IsDelayPassed == false || f.Global->IsFighting == false) return;

            List<BaseHeroFightingSystem.FighingHero> heroesPtr = new();
            BaseHeroFightingSystem.GetHeroes<TestProjectileHero>(f, ref heroesPtr);

            if (heroesPtr.Count > 0)
            {
                foreach (var fighingHero in heroesPtr)
                {
                    UpdateHero(f, fighingHero);
                }
            }
        }

        private void UpdateHero(Frame f, BaseHeroFightingSystem.FighingHero fighingHero)
        {
            fighingHero = BaseHeroFightingSystem.ProcessReload(f, fighingHero);

            if (BaseHeroFightingSystem.IsHeroMoving(f, fighingHero.Hero))
            {
                MoveHero(f, fighingHero, BoardPosition.GetHeroPosition(f, fighingHero.Hero));
                return;
            }

            if (TrySetTarget(f, fighingHero))
            {
                Attack(f, fighingHero);
                return;
            }
        }

        private bool TrySetTarget(Frame f, BaseHeroFightingSystem.FighingHero fighingHero)
        {
            Hero target = BaseHeroFightingSystem.GetHeroTarget(f, fighingHero, out Vector2Int moveTargetPosition);

            if (target.Ref == default)
            {
                return false;
            }

            if (moveTargetPosition == BoardPosition.GetHeroCords(fighingHero.Hero))
            {
                BaseHeroFightingSystem.SetHeroTarget(f, fighingHero, target.Ref);
                return true;
            }

            BaseHeroFightingSystem.SetHeroTarget(f, fighingHero, target.Ref, moveTargetPosition);
            return false;
        }

        private void MoveHero(Frame f, BaseHeroFightingSystem.FighingHero fighingHero, FPVector3 movePosition)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(fighingHero.Hero.Ref);
            FP moveOffset = gameConfig.HeroMoveSpeed * f.DeltaTime;

            transform->Position = FPVector3.MoveTowards(transform->Position, movePosition, moveOffset);
        }

        private void Attack(Frame f, BaseHeroFightingSystem.FighingHero fighingHero)
        {
            BaseHeroFightingSystem.ProcessProjectileAttack(f, fighingHero);
        }
    }
}