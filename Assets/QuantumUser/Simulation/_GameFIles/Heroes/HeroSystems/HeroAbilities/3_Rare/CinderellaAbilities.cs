using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class CinderellaAbilities : IHeroAbility
    {
        private static readonly FP CriticalDamageMultiplier = 2;
        private static readonly FP CriticalChance = FP._0_25;
        private static readonly FP AbilityIncreaseAttackSpeed = FP._1_20 + FP._0_10;
        private static readonly FP IncreasedAbilityIncreaseAttackSpeed = FP._1_50 + FP._0_10;
        private static readonly FP AbilityDuration = 3;
        private static readonly FP AbilityCooldown = 10;
        private static readonly FP DecreasedAbilityCooldown = 8;

        public override FP GetDamageMultiplier(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero target, QList<FightingHero> heroes)
        {
            if (fightingHero.Hero.NameIndex != (int)HeroNameEnum.Cinderella)
            {
                return 1;
            }

            PlayerLink playerLink = Player.GetPlayerLink(fightingHero, board);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);
            FP criticalChance = CriticalChance;

            if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
            {
                criticalChance += criticalChance;
            }

            if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
            {
                criticalChance += criticalChance;
            }

            if (f.RNG->Next() < criticalChance)
            {
                return CriticalDamageMultiplier;
            }

            return 1;
        }

        public override unsafe (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);
            FP abilityCooldown = AbilityCooldown;
            FP increasedAttackSpeed = AbilityIncreaseAttackSpeed;

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
            {
                increasedAttackSpeed = IncreasedAbilityIncreaseAttackSpeed;
            }

            HeroEffects.Effect effect1 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                Value = increasedAttackSpeed,
                Duration = AbilityDuration
            };

            HeroEffects.Effect effect2 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.TrueDamage,
                Duration = AbilityDuration
            };

            HeroEffects.Effect[] effects = new[] { effect1, effect2 };

            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effects);

            if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
            {
                abilityCooldown = DecreasedAbilityCooldown;
            }

            return (true, abilityCooldown);
        }
    }
}