using System.Collections.Generic;

namespace Quantum
{
    public partial class EventGetCurrentPlayers
    {
        public List<PlayerLink> Players;
        public List<Board> Boards;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetCurrentPlayers GetCurrentPlayers(Frame f, List<PlayerLink> players, List<Board> boards)
            {
                var ev = f.Events.GetCurrentPlayers();
                ev.Players = players;
                ev.Boards = boards;
                return ev;
            }
        }
    }
}
