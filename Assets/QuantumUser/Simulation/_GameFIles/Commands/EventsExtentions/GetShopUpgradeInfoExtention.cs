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
            public readonly EventGetShopUpgradeInfo GetShopUpgradeInfo(Frame f, PlayerRef playerRef, int UpgradeCost, List<float> HeroChanceList)
            {
                var ev = f.Events.GetShopUpgradeInfo(playerRef, UpgradeCost);
                ev.HeroChanceList = HeroChanceList;
                return ev;
            }
        }
    }
}