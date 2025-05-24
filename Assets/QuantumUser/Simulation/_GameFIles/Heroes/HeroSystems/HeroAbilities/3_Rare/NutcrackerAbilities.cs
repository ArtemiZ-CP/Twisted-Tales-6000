using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class NutcrackerAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            // QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            // fightingHero = heroes[fightingHero.Index];
            // PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            // int heroLevel = fightingHero.Hero.Level;
            // SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            // if (heroLevel == Hero.Level1)
            // {
            //     return TryCast(f, fightingHero, board, 120, 5, 150);
            // }
            // else if (heroLevel == Hero.Level2)
            // {
            //     return TryCast(f, fightingHero, board, 180, 8, 225);
            // }
            // else if (heroLevel == Hero.Level3)
            // {
            //     return TryCast(f, fightingHero, board, 270, 12, 340);
            // }

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

            for (int i = 0; i < targets.Count; i++)
            {
                FightingHero target = targets[i];
                HeroAttack.DamageHero(f, ref fightingHero, board, ref target, damage, effects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                targets[i] = target;
            }

            HeroEffects.Effect armorEffect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.TemporaryArmor,
                Value = armorAmount,
                Duration = 3,
            };

            HeroAttack.ApplyEffectsToTarget(f, ref fightingHero, board, ref fightingHero, armorEffect);

            return true;
        }
    }
}