using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class StatsDisplayer
    {
        public static void UpdateAllStats(Frame f)
        {
            BoardSystem.GetBoards(f).ForEach(board =>
            {
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
                f.Events.DisplayStats(f, board.Player1.Ref, board.Player2.Ref, heroes);
            });
        }

        public static void UpdateDamageStats(Frame f, FightingHero fightingHero, FP damage)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            FightingHero hero = heroes[fightingHero.Index];
            hero.DealedDamage += damage;
            heroes[fightingHero.Index] = hero;

            f.Events.DisplayStats(f, board.Player1.Ref, board.Player2.Ref, heroes);
        }
    }
}
