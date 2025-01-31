using Photon.Deterministic;

namespace Quantum.Game
{
    public class AbilityDisplayer : StatDisplayer
    {
        protected override void GetDisplaySettings(out string header, out System.Func<FightingHero, FP> statSelector)
        {
            header = "Ability Damage";
            statSelector = h => h.DealedAbilityDamage;
        }
    }
}