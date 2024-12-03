using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerShopSystem : SystemSignalsOnly, ISignalOnReloadShop, ISignalOnBuyHero, ISignalOnEndRound
    {
        public void OnBuyHero(Frame f, PlayerLink* playerLink, int shopIndex)
        {
            QList<int> heroesInPlayerShop = f.ResolveList(playerLink->Info.Shop.HeroesID);

            if (shopIndex > heroesInPlayerShop.Count - 1 || shopIndex < 0)
            {
                return;
            }

            int heroID = heroesInPlayerShop[shopIndex];

            if (heroID < 0)
            {
                return;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            AssetRef<HeroInfo>[] heroInfos = gameConfig.HeroInfos;
            HeroInfo heroInfo = null;

            for (int i = 0; i < heroInfos.Length; i++)
            {
                heroInfo = f.FindAsset(heroInfos[i]);

                if (i == heroID)
                {
                    break;
                }
            }

            if (heroInfo == null)
            {
                return;
            }

            QList<int> heroesInPlayerInventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);

            if (heroesInPlayerInventory.Contains(-1) == false)
            {
                return;
            }

            int inventoryIndex = heroesInPlayerInventory.IndexOf(-1);

            if (Player.TryRemoveCoins(f, playerLink, heroInfo.GetCost(f)) == false)
            {
                return;
            }

            heroesInPlayerInventory[inventoryIndex] = heroID;
            heroesInPlayerShop[shopIndex] = -1;

            f.Events.BuyHero(playerLink->Ref, shopIndex, inventoryIndex, heroID);
        }

        public void OnReloadShop(Frame f, PlayerLink* playerLink)
        {
            if (Player.TryRemoveCoins(f, playerLink, 1))
            {
                f.Events.ReloadShop(f, playerLink->Ref, ReloadShop(f, playerLink->Info.Shop));
            }
        }

        public void OnEndRound(Frame f)
        {
            var filter = f.Filter<PlayerLink>();

            while (filter.NextUnsafe(out var playerEntity, out PlayerLink* playerLink))
            {
                f.Events.ReloadShop(f, playerLink->Ref, ReloadShop(f, playerLink->Info.Shop));
            }
        }

        private List<int> ReloadShop(Frame f, PlayerShop playerShop)
        {
            QList<int> list = f.ResolveList(playerShop.HeroesID);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            for (int i = 0; i < gameConfig.ShopSize; i++)
            {
                list[i] = f.RNG->Next(0, gameConfig.HeroInfos.Length);
            }

            return list.ToList();
        }
    }
}