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
            public readonly EventReloadShop ReloadShop(Frame f, PlayerRef playerRef, int coins, List<int> HeroIDList)
            {
                var ev = f.Events.ReloadShop(playerRef, coins);
                ev.HeroIDList = HeroIDList;
                return ev;
            }
        }
    }
}