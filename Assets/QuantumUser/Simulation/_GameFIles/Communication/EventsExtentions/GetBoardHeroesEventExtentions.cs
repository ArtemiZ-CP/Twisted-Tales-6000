using Quantum.Collections;

namespace Quantum
{
    public partial class EventGetBoardHeroes
    {
        public QList<HeroIdLevel> HeroIDList;
    }

    public partial class Frame
    {
        public partial struct FrameEvents
        {
            public readonly EventGetBoardHeroes GetBoardHeroes(Frame f, PlayerRef playerRef, QList<HeroIdLevel> HeroList)
            {
                var ev = f.Events.GetBoardHeroes(playerRef);

                if (ev == null)
                {
                    return null;
                }

                ev.HeroIDList = HeroList;
                return ev;
            }
        }
    }
}