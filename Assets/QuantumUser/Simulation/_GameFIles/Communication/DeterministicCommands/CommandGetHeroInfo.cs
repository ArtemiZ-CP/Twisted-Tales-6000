using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandGetHeroInfo : DeterministicCommand
    {
        public EntityRef EntityRef;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref EntityRef);
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.GetHeroInfo(playerLink, EntityRef);
        }
    }
}