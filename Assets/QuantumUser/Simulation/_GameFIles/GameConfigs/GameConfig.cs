using System;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum.Game
{
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
        [Min(0)] public int SellCost1;
        [Min(0)] public int SellCost2;
        [Min(0)] public int SellCost3;
    }

    [Serializable]
    public class RoundInfo
    {
        public bool IsPVE;
        public List<BoardRow> PVEBoard = new();
    }

    [Serializable]
    public class BoardRow
    {
        public List<int> Cells = new();
    }

    public unsafe class GameConfig : AssetObject
    {
        public const int BoardSize = 8;

        [Header("Player")]
        public int PlayerHealth = 100;
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
        public FP ManaDamageRegenPersent;
        public FP HeroMoveSpeed = 1;
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

        public RoundInfo GetRoundInfo(Frame f, int round)
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
    }
}
