using Photon.Deterministic;

namespace Quantum.Game
{
    public class BaseDamageDisplayer : StatDisplayer
    {
        protected override void GetDisplaySettings(out string header, out System.Func<FightingHero, FP> statSelector)
        {
            header = "Auto Attack Damage";
            statSelector = h => h.DealedBaseDamage;
        }
    }
}