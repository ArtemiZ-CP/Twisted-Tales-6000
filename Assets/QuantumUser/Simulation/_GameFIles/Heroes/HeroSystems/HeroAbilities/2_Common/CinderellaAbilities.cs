using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class CinderellaAbilities
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
            //     return TryCast(f, fightingHero, board, 90);
            // }
            // else if (heroLevel == Hero.Level2)
            // {
            //     return TryCast(f, fightingHero, board, 135);
            // }
            // else if (heroLevel == Hero.Level3)
            // {
            //     return TryCast(f, fightingHero, board, 200);
            // }

            return false;
        }
    }
}