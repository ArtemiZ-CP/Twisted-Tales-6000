using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class StatsDisplayer
    {
        public static void UpdateStats(Frame f)
        {
            BoardSystem.GetBoards(f).ForEach(board =>
            {
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
                f.Events.DisplayStats(f, board.Player1.Ref, board.Player2.Ref, heroes);
            });
        }

        public static void UpdateStats(Frame f, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            f.Events.DisplayStats(f, board.Player1.Ref, board.Player2.Ref, heroes);
        }
    }
}
