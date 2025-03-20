using System;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class GameConfig : AssetObject
    {
        public const int BoardSize = 8;

        [Header("Player")]
        public int PlayerHealth = 100;
        public int MaxPlayers = 8;
        [Header("Coins settings")]
        public int CoinsPerWin = 1;
        public int CoinsPerLose = 0;
        public List<int> CoinsPerRound;
        public bool ResetCoinsOnEndRound;
        public List<int> WinStreakCoins;
        public bool ResetWinStreakOnEnd;
        public List<int> LoseStreakCoins;
        public bool ResetLoseStreakOnEnd;
        [Header("Hero settings")]
        public AssetRef<HeroInfo>[] HeroInfos;
        [Space(20)]
        public FP ManaRegen;
        public bool AddManaByPercentage;
        public FP ManaDealDamageRegen;
        public FP ManaTakeDamageRegen;
        public FP HeroMoveSpeed = 1;
        public FP HeroRotationSpeed = 1;
        [Tooltip("Half damage on current ratio")]
        public FP HeroDefenseRatio = 100;
        public FP RangePercentage;
        [Space(20)]
        public int HeroesCountToUpgrade = 3;
        public int MaxLevel = 3;
        [Header("Shop settings")]
        public HeroShopSettings[] HeroShopSettings;
        public ShopUpdrageSettings[] ShopUpdrageSettings;
        public int ShopSize = 8;
        public int ShopUpgrageCost = 5;
        public int ShopRollCost = 2;
        public bool ReloadOnUpgrade = false;
        [Header("Inventory settings")]
        public int InventorySize = 8;
        [Header("Round settings")]
        public List<RoundInfo> RoundInfos = new();
        public int XPByRound = 1;
        [Header("Board settings")]
        public float TileSize = 1.2f;
        [Header("Phase settings")]
        public int BuyPhaseTime = 30;
        public int FightPhaseTime = 60;
        public int StartFightingPhaseDelay = 1;
        public int EndFightingPhaseDelay = 1;
        public int PVPStreak = 3;
        [Header("Bot")]
        public AssetRef<EntityPrototype> BotPrototype;
        [Header("Base")]
        public AssetRef<EntityPrototype> EmptyPrototype;

        public GameConfig()
        {
            HeroShopSettings = new HeroShopSettings[Enum.GetValues(typeof(HeroRare)).Length];

            for (int i = 0; i < HeroShopSettings.Length; i++)
            {
                HeroShopSettings[i] = new HeroShopSettings()
                {
                    Rare = (HeroRare)i,
                };
            }
        }

        public RoundInfo GetRoundInfo(int round)
        {
            if (round >= RoundInfos.Count)
            {
                return new RoundInfo() { IsPVE = false };
            }

            return RoundInfos[round];
        }

        public int GetRandomHero(Frame f, HeroRare heroRare)
        {
            List<int> heroes = new();

            for (int i = 0; i < HeroInfos.Length; i++)
            {
                HeroInfo heroInfo = f.FindAsset(HeroInfos[i]);

                if (heroInfo.Rare == heroRare)
                {
                    heroes.Add(i);
                }
            }

            return heroes[f.RNG->Next(0, heroes.Count)];
        }

        public HeroInfo GetHeroInfo(Frame f, int heroID)
        {
            return f.FindAsset(HeroInfos[heroID]);
        }

        public int GetHeroBuyCost(HeroRare rare)
        {
            return HeroShopSettings[(int)rare].BuyCost;
        }

        public Color GetRareColor(HeroRare rare)
        {
            return HeroShopSettings[(int)rare].BackgroundColor;
        }

        public int GetHeroSellCost(Frame f, int heroID, int level)
        {
            HeroInfo heroInfo = GetHeroInfo(f, heroID);
            return GetHeroSellCost(heroInfo.Rare, level);
        }

        public int GetHeroSellCost(HeroRare rare, int level)
        {
            return HeroShopSettings[(int)rare].SellCosts[level];
        }

        public static void ArrayIndexToCords(int index, out int x, out int y)
        {
            x = index % BoardSize;
            y = index / BoardSize;
        }

        public static void ArrayCordsToIndex(int x, int y, out int index)
        {
            index = y * BoardSize + x;
        }
    }

    [Serializable]
    public class ShopUpdrageSettings
    {
        public int Cost;
        public int MaxCharactersOnBoard;
        public ShopHeroChance[] ShopHeroChances;
    }

    [Serializable]
    public class ShopHeroChance
    {
        public HeroRare Rare;
        [Min(0)] public int Chance;
    }

    [Serializable]
    public class HeroShopSettings
    {
        public HeroRare Rare;
        public Color BackgroundColor;
        [Min(0)] public int BuyCost;
        [Min(0)] public int[] SellCosts;
        [Min(0)] public int MaxCount;
    }

    [Serializable]
    public class RoundInfo
    {
        public bool IsPVE;
        public List<BoardRow> PVEBoard = new();

        public RoundInfo()
        {
            for (int i = 0; i < GameConfig.BoardSize; i++)
            {
                PVEBoard.Add(new BoardRow());
            }
        }
    }

    [Serializable]
    public class BoardRow
    {
        public List<int> Cells = new();

        public BoardRow()
        {
            for (int i = 0; i < GameConfig.BoardSize; i++)
            {
                Cells.Add(-1);
            }
        }
    }
}
