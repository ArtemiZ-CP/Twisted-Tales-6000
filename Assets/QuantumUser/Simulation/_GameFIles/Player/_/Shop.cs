using System.Linq;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class Shop
    {
        public static void Reload(Frame f)
        {
            var players = Player.GetAllPlayersEntity(f);

            foreach (var entity in players)
            {
                Reload(f, Player.GetPlayerPointer(f, entity));
            }
        }

        public static void Reload(Frame f, PlayerLink* playerLink)
        {
            QList<int> list = f.ResolveList(playerLink->Info.Shop.HeroesID);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            for (int i = 0; i < gameConfig.ShopSize; i++)
            {
                list[i] = f.RNG->Next(0, gameConfig.HeroInfos.Length);
            }

            f.Events.ReloadShop(f, playerLink->Ref, list.ToList());
        }
    }
}