using Quantum.Collections;

namespace Quantum
{
    public partial class EventGetInventoryHeroes
    {
        public QList<int> HeroIDList;
        public QList<int> HeroLevelList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetInventoryHeroes GetInventoryHeroes(Frame f, PlayerRef playerRef, QList<int> HeroIDList, QList<int> HeroLevelList)
            {
                var ev = f.Events.GetInventoryHeroes(playerRef);
                ev.HeroIDList = HeroIDList;
                ev.HeroLevelList = HeroLevelList;
                return ev;
            }
        }
    }
}