using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe static class MeleeHeroSystem
    {
        public static void Update(Frame f, QList<Board> boards)
        {
            BaseHeroFightingSystem.UpdateHeroes<MeleeHero>(f, boards, HeroAttack.InstantAttack, false);
        }
    }
}