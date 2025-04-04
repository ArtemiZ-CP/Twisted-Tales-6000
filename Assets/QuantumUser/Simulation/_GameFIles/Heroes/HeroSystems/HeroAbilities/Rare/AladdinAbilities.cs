using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class AladdinAbilities
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
                return TryCastLevel1(f, fightingHero, board);
            }
            else if (heroLevel == 1)
            {
                return TryCastLevel2(f, fightingHero, board);
            }
            else if (heroLevel == 2)
            {
                return TryCastLevel3(f, fightingHero, board);
            }

            return false;
        }

        private static bool TryCastLevel1(Frame f, FightingHero fightingHero, Board board)
        {
            return f.RNG->Next(0, 3) switch
            {
                0 => TryCastV1(f, fightingHero, board, 150),
                1 => TryCastV2(f, fightingHero, board, 100),
                2 => TryCastV3(f, fightingHero, board, 80),
                _ => false,
            };
        }

        private static bool TryCastLevel2(Frame f, FightingHero fightingHero, Board board)
        {
            return f.RNG->Next(0, 3) switch
            {
                0 => TryCastV1(f, fightingHero, board, 225),
                1 => TryCastV2(f, fightingHero, board, 150),
                2 => TryCastV3(f, fightingHero, board, 120),
                _ => false,
            };
        }

        private static bool TryCastLevel3(Frame f, FightingHero fightingHero, Board board)
        {
            return f.RNG->Next(0, 3) switch
            {
                0 => TryCastV1(f, fightingHero, board, 340),
                1 => TryCastV2(f, fightingHero, board, 225),
                2 => TryCastV3(f, fightingHero, board, 180),
                _ => false,
            };
        }

        private static bool TryCastV1(Frame f, FightingHero fightingHero, Board board, FP damage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroAttack.DamageHero(f, fightingHero, board, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastV2(Frame f, FightingHero fightingHero, Board board, FP amount)
        {
            HeroAttack.AddArmorToHero(f, fightingHero, board, fightingHero, amount);
            return true;
        }

        private static bool TryCastV3(Frame f, FightingHero fightingHero, Board board, FP amount)
        {
            List<FightingHero> alies = HeroBoard.GetAllAliesInRange(f, fightingHero, board);

            FightingHero alyWithMinHealth = default;
            FP minHealth = FP.MaxValue;

            foreach (FightingHero aly in alies)
            {
                FP health = aly.CurrentHealth;
                
                if (health < minHealth)
                {
                    minHealth = health;
                    alyWithMinHealth = aly;
                }
            }

            HeroAttack.HealHero(f, fightingHero, board, alyWithMinHealth, amount);

            return false;
        }
    }
}