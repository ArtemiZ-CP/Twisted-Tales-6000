using System;
using Photon.Deterministic;

namespace Quantum.Game
{
    public class DefDisplayer : StatDisplayer
    {
        protected override void GetDisplaySettings(out string header, out Func<FightingHero, FP> statSelector)
        {
            header = "Taken Damage";
            statSelector = h => h.TakenDamage;
        }
    }
}