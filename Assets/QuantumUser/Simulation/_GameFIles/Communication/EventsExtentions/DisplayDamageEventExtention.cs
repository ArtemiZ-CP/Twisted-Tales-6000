using Quantum.Collections;

namespace Quantum
{
    public partial class EventDisplayStats
    {
        public QList<FightingHero> Heroes;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventDisplayStats DisplayStats(Frame f, PlayerRef player1, PlayerRef player2, QList<FightingHero> heroes)
            {
                var ev = f.Events.DisplayStats(player1, player2);
                ev.Heroes = heroes;
                return ev;
            }
        }
    }
}