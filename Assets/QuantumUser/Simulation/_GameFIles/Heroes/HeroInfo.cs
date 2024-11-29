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

    public class HeroInfo : AssetObject
    {
        public string Name;
        public HeroRare Rare;
        public HeroMesh HeroPrefab;
        public AssetRef<EntityPrototype> HeroPrototype;
        public AssetRef<EntityPrototype> ProjectilePrototype;
        [Header("Stats")]
        [Min(0)] public int Health;
        [Min(0)] public int Defense;
        [Min(0)] public int Damage;
        [Min(0)] public float AttackSpeed;
        [Min(0)] public float ProjectileSpeed;
        [Min(1)] public int Range;
        [Range(0, 1)] public float RangePercentage;

        public int GetCost(Frame frame)
        {
            return frame.FindAsset(frame.RuntimeConfig.GameConfig).GetHeroBuyCost(Rare);
        }
    }
}
