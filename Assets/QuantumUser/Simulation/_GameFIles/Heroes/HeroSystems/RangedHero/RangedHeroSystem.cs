using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game.Heroes
{
    [Preserve]
    public unsafe static class RangedHeroSystem
    {
        public static void Update(Frame f, QList<Board> boards)
        {
            BaseHeroFightingSystem.UpdateHeroes<RangedHero>(f, boards, HeroAttack.ProjectileAttack, false);
        }
    }
}