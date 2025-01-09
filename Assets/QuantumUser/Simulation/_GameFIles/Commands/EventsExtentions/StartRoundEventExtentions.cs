using System.Collections.Generic;

namespace Quantum
{
    public struct EntityLevelData
    {
        public EntityRef Ref;
        public int Level;
        public int ID;
    }

    public partial class EventStartRound
    {
        public List<EntityLevelData> Heroes;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventStartRound StartRound(Frame f, PlayerRef playerRef1, PlayerRef playerRef2, List<EntityLevelData> Heroes)
            {
                var ev = f.Events.StartRound(playerRef1, playerRef2);
                ev.Heroes = Heroes;
                return ev;
            }
        }
    }
}
