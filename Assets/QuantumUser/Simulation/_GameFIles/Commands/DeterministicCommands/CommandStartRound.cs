using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandStartRound : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {
        }

        public void Execute(Frame f)
        {
            f.Signals.OnStartRound();
        }
    }
}
