using Quantum.Collections;

namespace Quantum
{
    public partial class EventGetShopHeroes
    {
        public QList<int> HeroIDList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetShopHeroes GetShopHeroes(Frame f, PlayerRef playerRef, QList<int> HeroIDList)
            {
                var ev = f.Events.GetShopHeroes(playerRef);
                ev.HeroIDList = HeroIDList;
                return ev;
            }
        }
    }
}