using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class SnowQueenAbilities : IHeroAbility
    {
        private static readonly FP ReduceAttackSpeed = FP._0_20 + FP._0_10;
        private static readonly FP AbilityDuration = 2;
        private static readonly FP AbilityDamageMultiplier = FP._1_20;
        private static readonly FP IncreasedAbilityDamageMultiplier = 2 + FP._0_50;
        private static readonly FP AbilityCooldown = 9;
        private static readonly FP AbilituReduceAttackSpeed = FP._0_25;
        private static readonly FP AbilituDoTDamageMultiplier = FP._0_50;

        public override FP GetDamageMultiplier(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero target, QList<FightingHero> heroes)
        {
            if (fightingHero.Hero.NameIndex != (int)HeroNameEnum.SnowQueen)
            {
                return 1;
            }
            
            HeroEffects.Effect effect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                Value = 1 - ReduceAttackSpeed,
                Duration = 1 / fightingHero.Hero.AttackSpeed
            };

            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref target, effect);

            return 1;
        }

        public override unsafe (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect stun = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.Stun,
                    Duration = AbilityDuration
                };

                HeroEffects.Effect[] effects = new[] { stun };

                if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
                {
                    HeroEffects.Effect slowEffect = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                        Value = 1 - AbilituReduceAttackSpeed,
                        Duration = AbilityDuration
                    };

                    effects = new[] { stun, slowEffect };
                }
                else if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
                {
                    HeroEffects.Effect dotEffect = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.Bleeding,
                        Value = AbilituDoTDamageMultiplier * fightingHero.Hero.AttackDamage,
                        Duration = AbilityDuration
                    };

                    effects = new[] { stun, dotEffect };
                }

                FP damage;

                if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
                {
                    damage = IncreasedAbilityDamageMultiplier * fightingHero.Hero.AttackDamage;
                }
                else
                {
                    damage = AbilityDamageMultiplier * fightingHero.Hero.AttackDamage;
                }

                if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1)
                {
                    HeroAttack.DamageHeroByBlast(f, fightingHero, fightingHero.Index, board, damage, fightingHero.Hero.Range, includeCenter: false, effects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                }
                else
                {
                    HeroAttack.DamageHero(f, ref fightingHero, board, ref target, damage, effects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                }

                return (true, AbilityCooldown);
            }

            return (false, 0);
        }
    }
}
