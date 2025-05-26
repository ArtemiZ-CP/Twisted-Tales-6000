using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class AladdinAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            // QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            // fightingHero = heroes[fightingHero.Index];
            // PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            // int heroLevel = fightingHero.Hero.Level;
            // SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            // FP damage1;
            // FP armor2;
            // FP heal3;

            // if (heroLevel == Hero.Level1)
            // {
            //     damage1 = 150;
            //     armor2 = 100;
            //     heal3 = 80;
            // }
            // else if (heroLevel == Hero.Level2)
            // {
            //     damage1 = 225;
            //     armor2 = 150;
            //     heal3 = 120;
            // }
            // else if (heroLevel == Hero.Level3)
            // {
            //     damage1 = 340;
            //     armor2 = 225;
            //     heal3 = 180;
            // }
            // else
            // {
            //     return false;
            // }

            // return f.RNG->Next(0, 3) switch
            // {
            //     0 => TryCastV1(f, fightingHero, board, damage1),
            //     1 => TryCastV2(f, fightingHero, board, armor2),
            //     2 => TryCastV3(f, fightingHero, board, heal3),
            //     _ => false,
            // };

            return false;
        }
    }
}