using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe class TestHeroSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            BaseHeroFightingSystem.UpdateHeroes<TestHero>(f, UpdateHero);
        }

        private void UpdateHero(Frame f, FightingHero fightingHero)
        {
            BaseHeroFightingSystem.ProcessReload(f, fightingHero);

            if (BaseHeroFightingSystem.IsHeroMoving(f, fightingHero.Hero))
            {
                MoveHero(f, fightingHero, BoardPosition.GetHeroPosition(f, fightingHero.Hero));
                return;
            }

            if (TrySetTarget(f, fightingHero))
            {
                Attack(f, fightingHero);
                return;
            }
        }

        private bool TrySetTarget(Frame f, FightingHero fightingHero)
        {
            FightingHero target = BaseHeroFightingSystem.GetHeroTarget(f, fightingHero, out Vector2Int moveTargetPosition);

            if (target.Hero.Ref == default)
            {
                return false;
            }

            if (moveTargetPosition == BoardPosition.GetHeroCords(fightingHero.Hero))
            {
                BaseHeroFightingSystem.SetHeroTarget(f, fightingHero, target.Hero.Ref);
                return true;
            }

            BaseHeroFightingSystem.SetHeroTarget(f, fightingHero, target.Hero.Ref, moveTargetPosition);
            return false;
        }

        private void MoveHero(Frame f, FightingHero fightingHero, FPVector3 movePosition)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(fightingHero.Hero.Ref);
            FP moveOffset = gameConfig.HeroMoveSpeed * f.DeltaTime;

            transform->Position = FPVector3.MoveTowards(transform->Position, movePosition, moveOffset);
        }

        private void Attack(Frame f, FightingHero fightingHero)
        {
            BaseHeroFightingSystem.ProcessInstantAttack(f, fightingHero);
        }
    }
}