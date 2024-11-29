using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandEndRound : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {
        }

        public void Execute(Frame f)
        {
            f.Signals.OnEndRound();
        }
    }
}
