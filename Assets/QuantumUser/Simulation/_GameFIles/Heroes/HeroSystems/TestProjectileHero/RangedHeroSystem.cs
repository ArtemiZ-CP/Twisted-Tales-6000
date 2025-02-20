using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe static class RangedHeroSystem
    {
        public static void Update(Frame f)
        {
            BaseHeroFightingSystem.UpdateHeroes<RangedHero>(f, HeroAttack.ProjectileAttack, false);
        }
    }
}