using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe class MeleeHeroSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            BaseHeroFightingSystem.UpdateHeroes<MeleeHero>(f, HeroAttack.ProcessInstantAttack, false);
        }
    }
}