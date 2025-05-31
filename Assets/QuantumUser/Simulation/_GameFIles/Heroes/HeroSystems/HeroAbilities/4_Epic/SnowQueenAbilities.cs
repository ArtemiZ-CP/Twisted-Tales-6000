using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class SnowQueenAbilities : IHeroAbility
    {
        private static readonly FP ReduceAttackSpeed = FP._0_20 + FP._0_10;

        public FP GetDamageMultiplier(Frame f, FightingHero fightingHero, Board board, FightingHero target, QList<FightingHero> heroes)
        {
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

        public HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo)
        {
            return heroInfo.Stats;
        }

        public void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
        }

        public void ProcessAbilityOnKill(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
        }

        public unsafe void ProcessPassiveAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
        }

        public unsafe (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            return (false, 0);
        }
    }
}
