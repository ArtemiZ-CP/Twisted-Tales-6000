using System;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum.Game
{
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

    public class GameConfig : AssetObject
    {
        public const int BoardSize = 8;

        [Header("Player")]
        public int PlayerHealth = 100;
        public int CoinsPerRound = 10;
        public bool ResetCoinsOnEndRound;
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
        public int ShopSize = 8;
        [Header("Inventory settings")]
        public int InventorySize = 8;
        [Header("Round settings")]
        public List<RoundInfo> RoundInfos = new();
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

        public HeroInfo GetHeroInfo(Frame f, int heroID)
        {
            return f.FindAsset(HeroInfos[heroID]);
        }

        public int GetHeroBuyCost(HeroRare rare)
        {
            return HeroShopSettings[(int)rare].BuyCost;
        }

        public Color GetHeroBackgroundColor(HeroRare rare)
        {
            return HeroShopSettings[(int)rare].BackgroundColor;
        }
    }
}
