using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandReloadShop : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.OnReloadShop(playerLink);
        }
    }
}