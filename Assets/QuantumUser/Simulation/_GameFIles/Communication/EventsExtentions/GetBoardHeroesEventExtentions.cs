using Quantum.Collections;

namespace Quantum
{
    public partial class EventGetBoardHeroes
    {
        public QList<int> HeroIDList;
        public QList<int> HeroLevelList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetBoardHeroes GetBoardHeroes(Frame f, PlayerRef playerRef, QList<int> HeroIDList, QList<int> HeroLevelList)
            {
                var ev = f.Events.GetBoardHeroes(playerRef);

                if (ev == null)
                {
                    return null;
                }

                ev.HeroIDList = HeroIDList;
                ev.HeroLevelList = HeroLevelList;
                return ev;
            }
        }
    }
}