using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandGetPlayerInfo : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.GetPlayersList();
            f.Events.GetPlayerInfo(f, playerLink->Ref, playerLink->Info);
        }
    }
}
