using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandUpgradeShop : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.OnUpgradeShop(playerLink);
        }
    }
}
