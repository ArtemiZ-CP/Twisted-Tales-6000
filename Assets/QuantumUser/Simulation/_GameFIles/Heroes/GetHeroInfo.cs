using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public class GetHeroInfo : SystemSignalsOnly, ISignalGetHeroInfo
    {
        unsafe void ISignalGetHeroInfo.GetHeroInfo(Frame f, PlayerLink* playerLink, EntityRef entityRef)
        {
            playerLink->Info.SpectatingHero = entityRef;
        }
    }
}
