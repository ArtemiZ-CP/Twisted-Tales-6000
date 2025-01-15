using System;
using System.Collections.Generic;
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
                list[i] = gameConfig.GetRandomHero(f, GetHeroRare(f, playerLink->Info.Shop.Level));
            }

            f.Events.ReloadShop(f, playerLink->Ref, list.ToList());
        }

        public static bool TryUpgradeShop(Frame f, PlayerLink* playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (IsShopMaxLevel(f, playerLink))
            {
                return false;
            }

            ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[playerLink->Info.Shop.Level];

            int upgradeCostToNextLevel = shopUpdrageSettings.Cost - playerLink->Info.Shop.XP;

            if (playerLink->Info.Shop.Level + 2 >= gameConfig.ShopUpdrageSettings.Length)
            {
                if (upgradeCostToNextLevel < gameConfig.ShopUpgrageCost)
                {
                    if (Player.TryRemoveCoins(f, playerLink, upgradeCostToNextLevel))
                    {
                        AddXP(f, playerLink, upgradeCostToNextLevel);

                        return true;
                    }
                }
            }

            if (Player.TryRemoveCoins(f, playerLink, gameConfig.ShopUpgrageCost))
            {
                AddXP(f, playerLink, gameConfig.ShopUpgrageCost);

                return true;
            }

            return false;
        }

        public static void AddXP(Frame f, int xp)
        {
            List<EntityRef> playerEntities = Player.GetAllPlayersEntity(f);

            foreach (EntityRef playerEntitie in playerEntities)
            {
                AddXP(f, Player.GetPlayerPointer(f, playerEntitie), xp);
            }
        }

        public static void AddXP(Frame f, PlayerLink* playerLink, int xp)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[playerLink->Info.Shop.Level];

            if (IsShopMaxLevel(f, playerLink))
            {
                return;
            }

            playerLink->Info.Shop.XP += xp;
            bool isUpgrade = false;

            while (playerLink->Info.Shop.XP >= shopUpdrageSettings.Cost)
            {
                playerLink->Info.Shop.XP -= shopUpdrageSettings.Cost;
                playerLink->Info.Shop.Level++;
                shopUpdrageSettings = gameConfig.ShopUpdrageSettings[playerLink->Info.Shop.Level];
                isUpgrade = true;

                if (IsShopMaxLevel(f, playerLink))
                {
                    playerLink->Info.Shop.XP = -1;
                    break;
                }
            }

            if (isUpgrade && gameConfig.ReloadOnUpgrade)
            {
                Reload(f, playerLink);
            }

            SendShopUpgradeInfo(f, playerLink);
        }

        public static void SendShopUpgradeInfo(Frame f, PlayerLink* playerLink)
        {
            int CurrentXP = playerLink->Info.Shop.XP;
            int MaxXPCost = -1;
            int CurrentLevel = playerLink->Info.Shop.Level;

            if (IsShopMaxLevel(f, playerLink) == false)
            {
                GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
                ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[playerLink->Info.Shop.Level];
                MaxXPCost = shopUpdrageSettings.Cost;
            }

            f.Events.GetShopUpgradeInfo(f, playerLink->Ref, CurrentXP, MaxXPCost, CurrentLevel, GetHeroChances(f, playerLink->Info.Shop.Level));
        }

        public static HeroRare GetHeroRare(Frame f, int shopLevel)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[shopLevel];

            int ChanceSum = 0;

            foreach (var ShopHeroChance in shopUpdrageSettings.ShopHeroChances)
            {
                ChanceSum += ShopHeroChance.Chance;
            }

            int random = f.RNG->Next(0, ChanceSum);

            foreach (var ShopHeroChance in shopUpdrageSettings.ShopHeroChances)
            {
                random -= ShopHeroChance.Chance;

                if (random < 0)
                {
                    return ShopHeroChance.Rare;
                }
            }

            throw new Exception("GetHeroRare failed");
        }

        public static List<float> GetHeroChances(Frame f, int shopLevel)
        {
            List<float> chances = new();

            foreach (HeroRare heroRare in Enum.GetValues(typeof(HeroRare)).Cast<HeroRare>())
            {
                chances.Add(GetHeroChance(f, shopLevel, heroRare));
            }

            return chances;
        }

        private static bool IsShopMaxLevel(Frame f, PlayerLink* playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            return playerLink->Info.Shop.Level + 1 >= gameConfig.ShopUpdrageSettings.Length;
        }

        private static float GetHeroChance(Frame f, int shopLevel, HeroRare heroRare)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[shopLevel];

            int ChanceSum = 0;

            foreach (var ShopHeroChance in shopUpdrageSettings.ShopHeroChances)
            {
                ChanceSum += ShopHeroChance.Chance;
            }

            foreach (var ShopHeroChance in shopUpdrageSettings.ShopHeroChances)
            {
                if (ShopHeroChance.Rare == heroRare)
                {
                    return (float)ShopHeroChance.Chance / ChanceSum;
                }
            }

            throw new Exception("GetHeroChance failed");
        }
    }
}