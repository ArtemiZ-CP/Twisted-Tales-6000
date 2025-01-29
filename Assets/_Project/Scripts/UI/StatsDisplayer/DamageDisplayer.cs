using Photon.Deterministic;

namespace Quantum.Game
{
    public class DamageDisplayer : StatDisplayer
    {
        protected override void GetDisplaySettings(out string header, out System.Func<FightingHero, FP> statSelector)
        {
            header = "Dealt Damage";
            statSelector = h => h.DealedDamage;
        }
    }
}