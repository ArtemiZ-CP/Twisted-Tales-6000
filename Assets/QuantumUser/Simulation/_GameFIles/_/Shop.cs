using System;
using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class Shop
    {
        public static void SetRollCost(Frame f, PlayerLink* playerLink, bool freeRoll)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            playerLink->Info.Shop.RollCost = freeRoll ? 0 : gameConfig.ShopRollCost;
            Events.SetShopRollCost(f, *playerLink);
        }

        public static void SetFreezeShop(Frame f, bool isLocked)
        {
            foreach (EntityRef player in Player.GetAllPlayersEntity(f))
            {
                SetFreezeShop(f, Player.GetPlayerPointer(f, player), isLocked);
            }
        }

        public static void SetFreezeShop(Frame f, PlayerLink* playerLink, bool isLocked)
        {
            playerLink->Info.Shop.IsLocked = isLocked;
            Events.SetFreezeShop(f, *playerLink);
        }

        public static void ReloadOnEndRound(Frame f)
        {
            var players = Player.GetAllPlayersEntity(f);

            foreach (var entity in players)
            {
                PlayerLink* playerLink = Player.GetPlayerPointer(f, entity);

                if (playerLink->Info.Shop.IsLocked == false)
                {
                    Reload(f, Player.GetPlayerPointer(f, entity));
                }
            }
        }

        public static void Reload(Frame f, PlayerLink* playerLink)
        {
            QList<int> list = f.ResolveList(playerLink->Info.Shop.HeroesID);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            for (int i = 0; i < gameConfig.ShopSize; i++)
            {
                list[i] = -1;
            }

            for (int i = 0; i < gameConfig.ShopSize; i++)
            {
                HeroRare heroRare = GetHeroRare(f, playerLink->Info.Shop.Level);
                int heroID = GetRandomHeroWeighted(f, playerLink, heroRare);
                list[i] = heroID;
            }

            SetFreezeShop(f, playerLink, false);
            f.Events.GetShopHeroes(f, playerLink->Ref, list);
        }

        private static int GetRandomHeroWeighted(Frame f, PlayerLink* playerLink, HeroRare heroRare)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            List<(int heroId, float weight)> availableHeroes = new();
            List<(int heroId, float weight)> availableHeroesWithRare = new();

            for (int i = 0; i < gameConfig.HeroInfos.Length; i++)
            {
                HeroInfo heroInfo = f.FindAsset(gameConfig.HeroInfos[i]);

                int heroMaxCount = gameConfig.HeroShopSettings[(int)heroInfo.Rare].MaxCount;
                int currentCount = GetCountHeroInGame(f, i);
                int remainingCount = heroMaxCount - currentCount;

                if (remainingCount > 0 && GetHeroCountByPlayer(f, *playerLink, i) < GetHeroCount(f, gameConfig.MaxLevel - 1))
                {
                    float weight = (float)remainingCount / heroMaxCount;
                    availableHeroes.Add((i, weight));

                    if (heroInfo.Rare == heroRare)
                    {
                        availableHeroesWithRare.Add((i, weight));
                    }
                }
            }

            if (availableHeroesWithRare.Count == 0)
            {
                if (availableHeroes.Count == 0)
                {
                    return gameConfig.GetRandomHero(f, heroRare);
                }
                else
                {
                    availableHeroesWithRare = availableHeroes;
                }
            }

            float totalWeight = availableHeroesWithRare.Sum(h => h.weight);
            float randomValue = f.RNG->Next().AsFloat * totalWeight;

            float currentWeight = 0;

            foreach (var (heroId, weight) in availableHeroesWithRare)
            {
                currentWeight += weight;

                if (randomValue <= currentWeight)
                {
                    return heroId;
                }
            }

            return availableHeroesWithRare[0].heroId;
        }

        public static bool TryUpgradeShop(Frame f, PlayerLink* playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (IsShopMaxLevel(f, *playerLink))
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

            if (IsShopMaxLevel(f, *playerLink))
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

                if (IsShopMaxLevel(f, *playerLink))
                {
                    playerLink->Info.Shop.XP = -1;
                    break;
                }
            }

            if (isUpgrade)
            {
                if (gameConfig.ReloadOnUpgrade)
                {
                    Reload(f, playerLink);
                }

                HeroMovingSystem.ShowHeroesOnBoardCount(f, *playerLink);
            }

            SendShopUpgradeInfo(f, *playerLink);
        }

        public static void SendShopUpgradeInfo(Frame f, PlayerLink playerLink)
        {
            int CurrentXP = playerLink.Info.Shop.XP;
            int MaxXPCost = -1;
            int CurrentLevel = playerLink.Info.Shop.Level;

            if (IsShopMaxLevel(f, playerLink) == false)
            {
                GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
                ShopUpdrageSettings shopUpdrageSettings = gameConfig.ShopUpdrageSettings[playerLink.Info.Shop.Level];
                MaxXPCost = shopUpdrageSettings.Cost;
            }

            f.Events.GetShopUpgradeInfo(f, playerLink.Ref, CurrentXP, MaxXPCost, CurrentLevel, GetHeroChances(f, playerLink.Info.Shop.Level));
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

        private static bool IsShopMaxLevel(Frame f, PlayerLink playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            return playerLink.Info.Shop.Level + 1 >= gameConfig.ShopUpdrageSettings.Length;
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

        private static int GetRandomHero(Frame f, PlayerLink* playerLink, HeroRare heroRare)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            QList<int> shop = f.ResolveList(playerLink->Info.Shop.HeroesID);

            List<int> heroes = new();

            for (int i = 0; i < gameConfig.HeroInfos.Length; i++)
            {
                HeroInfo heroInfo = f.FindAsset(gameConfig.HeroInfos[i]);

                if (heroInfo.Rare == heroRare)
                {
                    if (GetHeroCountByPlayer(f, *playerLink, i) < GetHeroCount(f, gameConfig.MaxLevel - 1))
                    {
                        if (GetCountHeroInGame(f, i) < gameConfig.HeroShopSettings[(int)heroInfo.Rare].MaxCount)
                        {
                            heroes.Add(i);
                        }
                    }
                }
            }

            if (heroes.Count == 0)
            {
                return -1;
            }

            return heroes[f.RNG->Next(0, heroes.Count)];
        }

        private static int GetCountHeroInGame(Frame f, int heroID)
        {
            int count = 0;

            foreach (PlayerLink player in Player.GetAllPlayerLinks(f))
            {
                count += GetHeroCountByPlayer(f, player, heroID);
            }

            return count;
        }

        private static int GetHeroCountByPlayer(Frame f, PlayerLink playerLink, int heroID)
        {
            int count = 0;

            QList<int> inventoryHeroesID = f.ResolveList(playerLink.Info.Inventory.HeroesID);
            QList<int> inventoryHeroesLevel = f.ResolveList(playerLink.Info.Inventory.HeroesLevel);

            for (int i = 0; i < inventoryHeroesID.Count; i++)
            {
                if (inventoryHeroesID[i] == heroID)
                {
                    count += GetHeroCount(f, inventoryHeroesLevel[i]);
                }
            }

            QList<int> shopHeroesID = f.ResolveList(playerLink.Info.Shop.HeroesID);

            for (int i = 0; i < shopHeroesID.Count; i++)
            {
                if (shopHeroesID[i] == heroID)
                {
                    count++;
                }
            }

            QList<int> boardHeroesID = f.ResolveList(playerLink.Info.Board.HeroesID);
            QList<int> boardHeroesLevel = f.ResolveList(playerLink.Info.Board.HeroesLevel);

            for (int i = 0; i < boardHeroesID.Count; i++)
            {
                if (boardHeroesID[i] == heroID)
                {
                    count += GetHeroCount(f, boardHeroesLevel[i]);
                }
            }

            return count;
        }

        private static int GetHeroCount(Frame f, int level)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (level == 0)
            {
                return 1;
            }
            else if (level == 1)
            {
                return gameConfig.HeroesCountToUpgrade;
            }
            else if (level == 2)
            {
                return gameConfig.HeroesCountToUpgrade * gameConfig.HeroesCountToUpgrade;
            }

            throw new Exception("GetHeroCount failed");
        }
    }
}