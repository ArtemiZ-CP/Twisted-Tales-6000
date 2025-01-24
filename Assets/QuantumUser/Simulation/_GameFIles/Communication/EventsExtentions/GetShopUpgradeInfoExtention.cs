using System.Collections.Generic;

namespace Quantum
{
    public partial class EventGetShopUpgradeInfo
    {
        public List<float> HeroChanceList = new();
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetShopUpgradeInfo GetShopUpgradeInfo(Frame f, PlayerRef playerRef, int CurrentXP, int MaxXPCost, int CurrentLevel, List<float> HeroChanceList)
            {
                var ev = f.Events.GetShopUpgradeInfo(playerRef, CurrentXP, MaxXPCost, CurrentLevel);
                ev.HeroChanceList = HeroChanceList;
                return ev;
            }
        }
    }
}