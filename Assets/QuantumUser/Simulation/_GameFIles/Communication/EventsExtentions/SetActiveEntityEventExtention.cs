namespace Quantum
{
    public partial class EventSetActiveEntity
    {
        public EntityLevelData EntityLevelData;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventSetActiveEntity SetActiveEntity(Frame f, PlayerRef playerRef, EntityRef entity, bool isActive, EntityLevelData entityLevelData)
            {
                var ev = f.Events.SetActiveEntity(playerRef, entity, isActive);
                ev.EntityLevelData = entityLevelData;
                return ev;
            }
        }
    }
}
