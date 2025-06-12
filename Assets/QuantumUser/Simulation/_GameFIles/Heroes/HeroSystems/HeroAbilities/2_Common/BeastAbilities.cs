using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class BeastAbilities : IHeroAbility
    {
        private readonly static FP ThornsPercentage = FP._0_10;
        private readonly static FP AbilityDuration = 2;
        private readonly static FP ReduceDamage = FP._0_20 + FP._0_10;
        private readonly static FP ReduceManaIncome = FP._0_50;
        private readonly static FP Cooldown = 6;
        private readonly static FP AbilityDurationIncrease = 1;

        public override void ProcessPassiveAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
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
                Type = HeroEffects.EffectType.Thorns,
                Value = ThornsPercentage,
                Duration = FP.MaxValue,
            };
            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effect);
        }

        public override (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP duration = AbilityDuration;

                if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
                {
                    duration += AbilityDurationIncrease;
                }

                if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
                {
                    duration += AbilityDurationIncrease;
                }


                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.IncreaseOutgoingDamage,
                    Value = 1 - ReduceDamage,
                    Duration = duration,
                };

                HeroEffects.Effect[] effects = new HeroEffects.Effect[]
                {
                    effect
                };

                if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
                {
                    HeroEffects.Effect effect2 = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.IncreaseManaIncome,
                        Value = 1 - ReduceManaIncome,
                        Duration = duration,
                    };

                    effects = new HeroEffects.Effect[]
                    {
                        effect,
                        effect2
                    };
                }

                if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1)
                {
                    HeroEffects.Effect effect3 = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.Stun,
                        Duration = duration,
                    };

                    HeroEffects.Effect[] newEffects = new HeroEffects.Effect[effects.Length + 1];
                    effects.CopyTo(newEffects, 0);
                    newEffects[effects.Length] = effect3;
                    effects = newEffects;
                }

                HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref target, effects);

                return (true, Cooldown);
            }

            return (false, 0);
        }
    }
}
