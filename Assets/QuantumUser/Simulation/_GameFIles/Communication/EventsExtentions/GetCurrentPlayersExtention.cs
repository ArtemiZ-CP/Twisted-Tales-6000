using System.Collections.Generic;
using Quantum.Collections;

namespace Quantum
{
    public partial class EventGetCurrentPlayers
    {
        public List<PlayerLink> Players;
        public QList<Board> Boards;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetCurrentPlayers GetCurrentPlayers(Frame f, List<PlayerLink> players, QList<Board> boards)
            {
                var ev = f.Events.GetCurrentPlayers();
                ev.Players = players;
                ev.Boards = boards;
                return ev;
            }
        }
    }
}
