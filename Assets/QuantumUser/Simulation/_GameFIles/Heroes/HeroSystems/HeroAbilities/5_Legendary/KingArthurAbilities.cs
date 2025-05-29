using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class KingArthurAbilities : IHeroAbility
    {
        public readonly static FP ReduceDamagePercentage = FP._0_10 + FP._0_05;

        private readonly static FP IncreaseDamage = FP._1_20;
        private readonly static FP IncreaseAttackSpeed = FP._1_20;
        private readonly static FP AbilityDuration = 5;
        private readonly static FP AbilityCooldown = 10;
        private readonly static FP IncreaseCurrentManaPercentage = FP._0_20;
        private readonly static FP HealPercentage = FP._0_10 + FP._0_05;
        private readonly static FP HealthPercentage = FP._0_50;
        private readonly static FP IncreasedAbilityDuration = 10;

        public FP GetDamageMultiplier(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            foreach (FightingHero hero in heroes)
            {
                if (hero.Hero.Ref == default || hero.Hero.ID == 0)
                {
                    continue;
                }

                HeroNameEnum heroName = (HeroNameEnum)hero.Hero.NameIndex;;

                if (heroName == HeroNameEnum.KingArthur && hero.TeamNumber == fightingHero.TeamNumber)
                {
                    return 1 - ReduceDamagePercentage;
                }
            }

            return 1;
        }

        public HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo)
        {
            return heroInfo.Stats;
        }

        public void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
        }

        public void ProcessPassiveAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
        }

        public (bool, FP) TryCastAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            var alies = HeroBoard.GetAllTeamHeroesInRange(f, fightingHero.Index, fightingHero.TeamNumber, board, range: GameplayConstants.BoardSize, includeSelf: true);

            FP duration = AbilityDuration;

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1 && fightingHero.CurrentHealth > fightingHero.Hero.MaxMana * HealthPercentage)
            {
                duration = IncreasedAbilityDuration;
            }

            HeroEffects.Effect effect1 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.IncreaseOutgoingDamage,
                Value = IncreaseDamage,
                Duration = duration,
            };

            HeroEffects.Effect effect2 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                Value = IncreaseAttackSpeed,
                Duration = duration,
            };

            HeroEffects.Effect effect4 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.Delayed,
                Value = IncreaseAttackSpeed,
                Duration = duration,
            };

            HeroEffects.Effect[] effects = { effect1, effect2 };

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
            {
                effects = new[] { effect1, effect2, effect4 };
            }

            for (int i = 0; i < alies.Count; i++)
            {
                FightingHero aly = alies[i];

                if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
                {
                    HeroEffects.Effect effect3 = new()
                    {
                        Owner = fightingHero.Hero.Ref,
                        Type = HeroEffects.EffectType.IncreaseCurrentMana,
                        Value = IncreaseCurrentManaPercentage * aly.Hero.MaxMana,
                    };

                    HeroEffects.Effect[] newEffects = new HeroEffects.Effect[effects.Length + 1];
                    effects.CopyTo(newEffects, 0);
                    newEffects[effects.Length] = effect3;
                    effects = newEffects;
                }
                else if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
                {
                    HeroAttack.HealHero(f, ref fightingHero, board, aly, HealPercentage * aly.Hero.Health);
                }

                HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref aly, effects);
            }

            return (true, AbilityCooldown);
        }
    }
}
