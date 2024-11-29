using System.Collections.Generic;

namespace Quantum
{
    public partial class EventGetProjectiles
    {
        public List<EntityLevelData> ProjectileList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetProjectiles GetProjectiles(Frame f, PlayerRef player1, PlayerRef player2, List<EntityLevelData> projectileList)
            {
                var ev = f.Events.GetProjectiles(player1, player2);
                ev.ProjectileList = projectileList;
                return ev;
            }
        }
    }
}
