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
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
                UpdateStats(f, heroes, board);
            }
        }

        public static void UpdateStats(Frame f, QList<FightingHero> heroes, Board board)
        {
            f.Events.DisplayStats(f, board.Player1.Ref, board.Player2.Ref, heroes);
        }
    }
}
