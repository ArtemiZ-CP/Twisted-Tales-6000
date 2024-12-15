using System.Collections.Generic;

namespace Quantum
{
    public partial class EventReloadShop
    {
        public List<int> HeroIDList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventReloadShop ReloadShop(Frame f, PlayerRef playerRef, List<int> HeroIDList)
            {
                var ev = f.Events.ReloadShop(playerRef);
                ev.HeroIDList = HeroIDList;
                return ev;
            }
        }
    }
}