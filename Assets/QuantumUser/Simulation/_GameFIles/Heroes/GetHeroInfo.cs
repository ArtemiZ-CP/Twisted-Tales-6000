using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public class GetHeroInfo : SystemSignalsOnly, ISignalGetHeroInfo
    {
        unsafe void ISignalGetHeroInfo.GetHeroInfo(Frame f, PlayerLink* playerLink, EntityRef entityRef)
        {
            if (f.Exists(entityRef) == false)
            {
                return;
            }

            Board board = BoardSystem.GetBoard(f, playerLink->Ref);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            foreach (FightingHero hero in heroes)
            {
                if (hero.Hero.Ref == entityRef)
                {
                    f.Events.GetFightingHero(playerLink->Ref, hero);
                }
            }
        }
    }
}
