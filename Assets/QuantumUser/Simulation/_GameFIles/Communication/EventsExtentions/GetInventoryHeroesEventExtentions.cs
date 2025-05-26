using Quantum.Collections;

namespace Quantum
{
    public partial class EventGetInventoryHeroes
    {
        public QList<HeroIdLevel> HeroIDList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetInventoryHeroes GetInventoryHeroes(Frame f, PlayerRef playerRef, QList<HeroIdLevel> HeroList)
            {
                var ev = f.Events.GetInventoryHeroes(playerRef);
                ev.HeroIDList = HeroList;
                return ev;
            }
        }
    }
}