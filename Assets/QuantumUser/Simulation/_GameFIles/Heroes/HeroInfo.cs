using System;
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

    [Serializable]
    public struct HeroLevelStats
    {
        [Min(0)] public float Health;
        [Min(0)] public float Defense;
        [Min(0)] public float Damage;
        [Min(0)] public float AttackSpeed;
        [Min(0)] public float ProjectileSpeed;
        [Min(1)] public int Range;
    }

    public class HeroInfo : AssetObject
    {
        public string Name;
        public HeroRare Rare;
        public HeroMesh HeroPrefab;
        public AssetRef<EntityPrototype> HeroPrototype;
        public AssetRef<EntityPrototype> ProjectilePrototype;
        [Header("Stats")]
        public HeroLevelStats[] HeroStats;

        public int GetCost(Frame frame)
        {
            return frame.FindAsset(frame.RuntimeConfig.GameConfig).GetHeroBuyCost(Rare);
        }
    }
}
