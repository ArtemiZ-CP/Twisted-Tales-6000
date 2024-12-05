using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe class RangedHeroSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            BaseHeroFightingSystem.UpdateHeroes<RangedHero>(f, HeroAttack.ProjectileAttack, false);
        }
    }
}