using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class TinManAbilities
    {
        public static readonly FP AbilityReloadTime = 10;
        private const int IncreaseDefenseInPassiveAbility = 20;
        private const int AbilityDuration = 2;
        private const int TauntRangeEffect = 1;
        private static readonly FP HealthPercentageToActivePassiveAbility = FP._0_25;

        public static void ProcessPassiveAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
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

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            return TryCast(f, fightingHero, board);
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board)
        {
            HeroEffects.Effect effect1 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.Stun,
                Duration = AbilityDuration,
            };
            HeroEffects.Effect effect2 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.Immortal,
                Duration = AbilityDuration,
            };
            HeroEffects.Effect[] effects = new[] { effect1, effect2 };
            HeroAttack.ApplyEffectsToTarget(f, ref fightingHero, board, ref fightingHero, effects);
            HeroEffects.GlobalEffect globalEffect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.GlobalEffectType.TauntedArea,
                Duration = AbilityDuration,
                Size = TauntRangeEffect,
            };
            HeroEffects.AddGlobalEffect(f, board, globalEffect);
            return true;
        }
    }
}
