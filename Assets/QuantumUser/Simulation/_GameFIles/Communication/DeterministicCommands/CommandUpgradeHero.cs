using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandUpgradeHero : DeterministicCommand
    {
        public int HeroID;
        public int HeroLevel;
        public int UpgradeLevel;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref HeroID);
            stream.Serialize(ref HeroLevel);
            stream.Serialize(ref UpgradeLevel);
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.LevelUpHero(playerLink, HeroID, HeroLevel, UpgradeLevel);
        }
    }
}
