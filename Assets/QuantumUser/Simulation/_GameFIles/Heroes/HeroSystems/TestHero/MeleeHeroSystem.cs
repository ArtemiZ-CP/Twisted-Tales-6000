using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe static class MeleeHeroSystem
    {
        public static void Update(Frame f)
        {
            BaseHeroFightingSystem.UpdateHeroes<MeleeHero>(f, HeroAttack.InstantAttack, false);
        }
    }
}