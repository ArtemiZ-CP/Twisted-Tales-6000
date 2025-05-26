using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class PussInBootsAbilities
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
            //     return TryCast(f, fightingHero, board, 140, 60);
            // }
            // else if (heroLevel == Hero.Level2)
            // {
            //     return TryCast(f, fightingHero, board, 210, 90);
            // }
            // else if (heroLevel == Hero.Level3)
            // {
            //     return TryCast(f, fightingHero, board, 315, 135);
            // }

            return false;
        }
    }
}