using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerShopSystem : SystemSignalsOnly, ISignalOnReloadShop, ISignalOnBuyHero, ISignalOnUpgradeShop, ISignalChangeFreezeShop
    {
        public void OnBuyHero(Frame f, PlayerLink* playerLink, int shopIndex)
        {
            if (TryGetHeroesInShop(f, playerLink, shopIndex, out QList<int> heroesInPlayerShop) == false)
            {
                return;
            }

            if (TryGetHeroID(f, heroesInPlayerShop, shopIndex, out int heroID) == false)
            {
                return;
            }

            if (TryGetHeroInfo(f, heroID, out HeroInfo heroInfo) == false)
            {
                return;
            }

            if (TryGetHeroInventoryIndex(f, playerLink, out QList<HeroIdLevel> heroesInPlayerInventory, out int inventoryIndex))
            {
                if (Player.TryRemoveCoins(f, playerLink, heroInfo.GetBuyCost(f)) == false)
                {
                    return;
                }

                HeroIdLevel newHero = heroesInPlayerInventory[inventoryIndex];
                newHero.ID = heroID;
                heroesInPlayerInventory[inventoryIndex] = newHero;
                heroesInPlayerShop[shopIndex] = -1;

                f.Events.BuyHero(playerLink->Ref, shopIndex, inventoryIndex, heroID);
            }
            else
            {
                if (TryUpgradeHero(f, playerLink, heroID, heroInfo))
                {
                    f.Events.BuyHero(playerLink->Ref, shopIndex, inventoryIndex, heroID);
                }
            }

            f.Signals.TryUpgradeHero(playerLink);
            Events.GetInventoryHeroes(f, playerLink->Ref);
        }

        public void ChangeFreezeShop(Frame f, PlayerLink* playerLink)
        {
            Shop.SetFreezeShop(f, playerLink, playerLink->Info.Shop.IsLocked == false);
        }

        public void OnReloadShop(Frame f, PlayerLink* playerLink)
        {
            if (Player.TryRemoveCoins(f, playerLink, playerLink->Info.Shop.RollCost))
            {
                Shop.Reload(f, playerLink);
                Shop.SetRollCost(f, playerLink, freeRoll: false);
            }
        }

        public void OnUpgradeShop(Frame f, PlayerLink* playerLink)
        {
            Shop.TryUpgradeShop(f, playerLink);
        }

        private bool TryGetHeroesInShop(Frame f, PlayerLink* playerLink, int shopIndex, out QList<int> heroesInShop)
        {
            heroesInShop = f.ResolveList(playerLink->Info.Shop.HeroesID);

            if (heroesInShop.Count == 0)
            {
                return false;
            }

            if (shopIndex > heroesInShop.Count - 1 || shopIndex < 0)
            {
                return false;
            }

            return true;
        }

        private bool TryGetHeroID(Frame f, QList<int> heroesInPlayerShop, int shopIndex, out int heroID)
        {
            heroID = heroesInPlayerShop[shopIndex];

            if (heroID < 0)
            {
                return false;
            }

            return true;
        }

        private bool TryGetHeroInfo(Frame f, int heroID, out HeroInfo heroInfo)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            AssetRef<HeroInfo>[] heroInfos = gameConfig.HeroInfos;
            heroInfo = null;

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
                return false;
            }

            return true;
        }

        private bool TryGetHeroInventoryIndex(Frame f, PlayerLink* playerLink, out QList<HeroIdLevel> heroesInPlayerInventory, out int index)
        {
            heroesInPlayerInventory = f.ResolveList(playerLink->Info.Inventory.Heroes);

            for (index = 0; index < heroesInPlayerInventory.Count; index++)
            {
                if (heroesInPlayerInventory[index].ID < 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryUpgradeHero(Frame f, PlayerLink* playerLink, int id, HeroInfo heroInfo)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            int level = 0;

            if (HeroLevelProgression.GetHeroCount(f, playerLink, id, level) == gameConfig.HeroesCountToUpgrade - 1)
            {
                if (Player.TryRemoveCoins(f, playerLink, heroInfo.GetBuyCost(f)) == false)
                {
                    return false;
                }

                HeroLevelProgression.UpgradeHero(f, playerLink, id, level, gameConfig.HeroesCountToUpgrade - 1);
                return true;
            }

            return false;
        }
    }
}