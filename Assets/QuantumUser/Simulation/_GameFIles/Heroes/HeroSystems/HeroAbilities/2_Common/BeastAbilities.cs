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
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            if (heroLevel == Hero.Level1)
            {
                return TryCast(f, fightingHero, board, 100, 50);
            }
            else if (heroLevel == Hero.Level2)
            {
                return TryCast(f, fightingHero, board, 150, 75);
            }
            else if (heroLevel == Hero.Level3)
            {
                return TryCast(f, fightingHero, board, 225, 110);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP damage, FP heal)
        {
            HeroAttack.AddArmorToHero(f, fightingHero, board, fightingHero, heal);
            HeroAttack.DamageHeroByBlast(f, fightingHero, fightingHero.Index, board, damage, fightingHero.Hero.Range, includeSelf: true, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            return true;
        }
    }
}