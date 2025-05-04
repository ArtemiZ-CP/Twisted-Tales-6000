using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class NutcrackerAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            int secondHeroAbilityIndex = HeroAbility.GetSecondHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);
            int thirdHeroAbilityIndex = HeroAbility.GetThirdHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);

            if (heroLevel == 0)
            {
                return TryCast(f, fightingHero, board, 120, 5, 150);
            }
            else if (heroLevel == 1)
            {
                return TryCast(f, fightingHero, board, 180, 8, 225);
            }
            else if (heroLevel == 2)
            {
                return TryCast(f, fightingHero, board, 270, 12, 340);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP damage, FP reduceAmount, FP armorAmount)
        {
            List<FightingHero> targets = HeroBoard.GetAllTargetsInRange(f, fightingHero, board);

            HeroEffects.Effect reduceEffect1 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.ReduceDefense,
                Value = reduceAmount,
                Duration = 3,
            };

            HeroEffects.Effect reduceEffect2 = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.ReduceMagicDefense,
                Value = reduceAmount,
                Duration = 3,
            };

            HeroEffects.Effect[] effects = new HeroEffects.Effect[] { reduceEffect1, reduceEffect2 };
            
            foreach (FightingHero target in targets)
            {
                HeroAttack.DamageHero(f, fightingHero, board, target, damage, effects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            }

            HeroEffects.Effect armorEffect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.TemporaryArmor,
                Value = armorAmount,
                Duration = 3,
            };

            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, armorEffect);

            return true;
        }
    }
}