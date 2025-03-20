using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class BeastAbilities
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
            FP damage = 100;
            FP heal = 50;
            HeroAttack.HealHero(f, fightingHero, board, fightingHero, heal, isAbleToOverHeal: true);
            HeroAttack.DamageHeroByBlast(f, fightingHero, fightingHero.Index, board, damage, fightingHero.Hero.Range, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            return true;
        }

        private static bool TryCastLevel2(Frame f, FightingHero fightingHero, Board board)
        {
            FP damage = 150;
            FP heal = 75;
            HeroAttack.HealHero(f, fightingHero, board, fightingHero, heal, isAbleToOverHeal: true);
            HeroAttack.DamageHeroByBlast(f, fightingHero, fightingHero.Index, board, damage, fightingHero.Hero.Range, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            return true;
        }

        private static bool TryCastLevel3(Frame f, FightingHero fightingHero, Board board)
        {
            FP damage = 225;
            FP heal = 110;
            HeroAttack.HealHero(f, fightingHero, board, fightingHero, heal, isAbleToOverHeal: true);
            HeroAttack.DamageHeroByBlast(f, fightingHero, fightingHero.Index, board, damage, fightingHero.Hero.Range, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            return true;
        }
    }
}