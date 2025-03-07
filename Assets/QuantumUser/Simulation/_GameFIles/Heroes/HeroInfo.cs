using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum.Game
{
    public enum HeroRare
    {
        Basic,
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum HeroType
    {
        Ranged,
        Melee
    }

    [Serializable]
    public struct HeroLevelStats
    {
        public FP Health;
        public FP Defense;
        public FP MagicDefense;
        public FP AttackDamage;
        public FP AttackSpeed;
        public FP ProjectileSpeed;
        public int Range;
    }

    public class HeroInfo : AssetObject
    {
        public HeroMesh HeroPrefab;
        public AssetRef<EntityPrototype> HeroPrototype;
        public AssetRef<EntityPrototype> ProjectilePrototype;
        public AssetRef<EntityPrototype> AbilityProjectilePrototype;
        [Header("Hero Info")]
        public string Name;
        public HeroRare Rare;
        public HeroType HeroType;
        [Header("Stats")]
        public HeroLevelStats[] HeroStats;
        public HeroAttack.DamageType AttackDamageType;
        public FP Mana;
        public FP StartMana;

        public int GetBuyCost(Frame frame)
        {
            return frame.FindAsset(frame.RuntimeConfig.GameConfig).GetHeroBuyCost(Rare);
        }

        public int GetSellCost(Frame frame, int level)
        {
            return frame.FindAsset(frame.RuntimeConfig.GameConfig).GetHeroSellCost(Rare, level);
        }
    }
}
