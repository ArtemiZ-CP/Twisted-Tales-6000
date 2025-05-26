using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class TinManAbilities : IHeroAbility
    {
        private const int IncreaseDefenseInPassiveAbility = 20;
        private const int AbilityDuration = 2;
        private const int UpgradedAbilityDuration = 4;
        private const int IncreaseAttackSpeedDuration = 4;
        private const int AbilityRangeEffect = 1;
        private const int UpgradedAbilityRangeEffect = 2;
        private static readonly FP AbilityReloadTime = 10;
        private static readonly FP UpgradedAbilityReloadTime = 7;
        private static readonly FP HealthPercentageToActivePassiveAbility = FP._0_25;
        private static readonly FP HealPercentage = FP._0_75;
        private static readonly FP IncreaseAttackSpeed = FP._1_20;

        public HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo)
        {
            return heroInfo.Stats;
        }

        public void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
        }

        public void ProcessPassiveAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            if (fightingHero.IsPassiveAbilityActivated)
            {
                return;
            }

            if (fightingHero.CurrentHealth < HealthPercentageToActivePassiveAbility * fightingHero.Hero.Health)
            {
                fightingHero = heroes[fightingHero.Index];
                fightingHero.IsPassiveAbilityActivated = true;
                fightingHero.Hero.Defense = FPMath.Min(fightingHero.Hero.Defense + IncreaseDefenseInPassiveAbility, HeroAttack.MaxDefense);
                heroes[fightingHero.Index] = fightingHero;
            }
        }

        public (bool, FP) TryCastAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            int abilityRange = AbilityRangeEffect;
            FP reloadTime = AbilityReloadTime;
            int abilityDuration = AbilityDuration;

            if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
            {
                CastLevel2Variant1(f, fightingHero, board);
            }
            else if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
            {
                CastLevel2Variant2(f, fightingHero, board);
            }

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1)
            {
                abilityRange = UpgradedAbilityRangeEffect;
            }
            else if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
            {
                reloadTime = UpgradedAbilityReloadTime;
                abilityDuration = UpgradedAbilityDuration;
            }

            CastMainAbility(f, fightingHero, board, abilityRange, abilityDuration);

            return (true, reloadTime);
        }

        private bool CastMainAbility(Frame f, FightingHero fightingHero, Board board, int abilityRange, int abilityDuration)
        {
            HeroEffects.Effect effect1 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.Stun,
                Duration = abilityDuration,
            };
            HeroEffects.Effect effect2 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.Immortal,
                Duration = abilityDuration,
            };
            HeroEffects.Effect[] effects = new[] { effect1, effect2 };
            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effects);
            HeroEffects.GlobalEffect globalEffect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.GlobalEffectType.TauntedArea,
                Duration = abilityDuration,
                Size = abilityRange,
            };
            HeroEffects.AddGlobalEffect(f, board, globalEffect);
            return true;
        }

        private void CastLevel2Variant1(Frame f, FightingHero fightingHero, Board board)
        {
            HeroEffects.GlobalEffect globalEffect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.GlobalEffectType.HealArea,
                Value = fightingHero.Hero.AttackDamage * HealPercentage,
                Center = fightingHero.Index,
                Duration = AbilityDuration,
                Size = AbilityRangeEffect,
            };

            HeroEffects.AddGlobalEffect(f, board, globalEffect);
        }

        private void CastLevel2Variant2(Frame f, FightingHero fightingHero, Board board)
        {
            HeroEffects.Effect effect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.IncreaseAttackSpeed,
                Value = IncreaseAttackSpeed,
                Duration = IncreaseAttackSpeedDuration,
            };

            var alies = HeroBoard.GetAllAliesInRange(f, fightingHero, board, includeSelf: false);

            for (int i = 0; i < alies.Count; i++)
            {
                FightingHero ally = alies[i];
                HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref ally, effect);
            }
        }
    }
}
