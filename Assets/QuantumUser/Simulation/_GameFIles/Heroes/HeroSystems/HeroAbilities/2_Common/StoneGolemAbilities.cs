using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class StoneGolemAbilities : IHeroAbility
    {
        private readonly static FP ReduceDamageMultiplier = FP._0_10;
        private readonly static FP AbilityDamageMultiplier = FP._1_50;
        private readonly static FP AbilityCooldown = 8;
        private readonly static FP ReduceDefenceDuration = 4;
        private readonly static FP ReduceDefence = 15;
        private readonly static FP ReduceAttackSpeedDuration = 2;
        private readonly static FP ReduceAttackSpeed = FP._0_20 + FP._0_10;
        private readonly static FP IncreaseDefence = 15;
        private readonly static FP IncreaesHealth = FP._1_10 + FP._0_05;

        private const int IncreaseDamageMultiply = 2;

        public FP GetDamageMultiplier(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            int defendersCount = 0;

            foreach (FightingHero hero in heroes)
            {
                if (hero.Hero.Ref == default || hero.Hero.ID == 0 ||
                    hero.TeamNumber != fightingHero.TeamNumber || hero.Hero.Ref == fightingHero.Hero.Ref)
                {
                    continue;
                }

                if (hero.Hero.TypeIndex == (int)HeroType.Tank)
                {
                    defendersCount++;
                }
            }

            return 1 - (ReduceDamageMultiplier * defendersCount);
        }

        public HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, gameConfig.GetHeroID(f, heroInfo.Name), out int _);
            HeroStats heroStats = heroInfo.Stats;

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1)
            {
                HeroLevelStats[] heroLevelStats = new HeroLevelStats[heroStats.LevelStats.Length];

                for (int i = 0; i < heroStats.LevelStats.Length; i++)
                {
                    heroLevelStats[i] = heroStats.LevelStats[i];
                    heroLevelStats[i].Defense += IncreaseDefence;
                    heroLevelStats[i].Health *= IncreaesHealth;
                }

                heroStats.LevelStats = heroLevelStats;
            }

            return heroStats;
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
            FP damage = AbilityDamageMultiplier * fightingHero.Hero.AttackDamage;
            HeroEffects.Effect effect = null;

            if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
            {
                effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.ReduceDefense,
                    Value = ReduceDefence,
                    Duration = ReduceDefenceDuration,
                };
            }
            else if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
            {
                effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                    Value = 1 - ReduceAttackSpeed,
                    Duration = ReduceAttackSpeedDuration,
                };
            }

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
            {
                damage *= IncreaseDamageMultiply;
            }

            HeroAttack.DamageHeroByBlastWithoutAddMana(f, ref fightingHero, fightingHero.Index, board, damage, fightingHero.Hero.Range, includeSelf: false, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);

            return (true, AbilityCooldown);
        }
    }
}
