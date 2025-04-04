using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class StatsDisplayer
    {
        public static void UpdateStats(Frame f)
        {
            QList<Board> boards = BoardSystem.GetBoards(f);

            foreach (Board board in boards)
            {
                UpdateStats(f, board);
            }
        }

        public static void UpdateStats(Frame f, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            f.Events.DisplayStats(f, board.Player1.Ref, board.Player2.Ref, heroes);
        }
    }
}
