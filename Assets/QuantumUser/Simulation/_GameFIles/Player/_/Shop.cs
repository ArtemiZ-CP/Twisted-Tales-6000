using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
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

            if (playerLink->Info.Shop.Level + 1 >= gameConfig.ShopUpdrageSettings.Length)
            {
                return false;
            }

            ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[playerLink->Info.Shop.Level];

            if (Player.TryRemoveCoins(f, playerLink, shopUpdrageSettings.Cost))
            {
                playerLink->Info.Shop.Level++;
                int shopLevel = playerLink->Info.Shop.Level;
                Reload(f, playerLink);

                if (shopLevel + 1 >= gameConfig.ShopUpdrageSettings.Length)
                {
                    f.Events.GetShopUpgradeInfo(f, playerLink->Ref, -1, GetHeroChances(f, shopLevel));
                }
                else
                {
                    int upgradeCost = gameConfig.ShopUpdrageSettings[shopLevel].Cost;
                    f.Events.GetShopUpgradeInfo(f, playerLink->Ref, upgradeCost, GetHeroChances(f, shopLevel));
                }

                return true;
            }

            return false;
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