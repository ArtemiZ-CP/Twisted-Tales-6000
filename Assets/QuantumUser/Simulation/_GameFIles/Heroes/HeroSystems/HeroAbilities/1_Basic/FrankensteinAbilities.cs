using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class FrankensteinAbilities : IHeroAbility
    {
        private static readonly FP PassiveDamage = 15;

        public override unsafe void ProcessPassiveAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            if (fightingHero.IsPassiveAbilityActivated)
            {
                return;
            }

            fightingHero = heroes[fightingHero.Index];
            fightingHero.IsPassiveAbilityActivated = true;
            heroes[fightingHero.Index] = fightingHero;

            HeroEffects.Effect effect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.Bleeding,
                Value = PassiveDamage,
                Duration = FP.MaxValue
            };

            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effect);
        }

        public override unsafe (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            return (false, 1);
        }
    }
}
