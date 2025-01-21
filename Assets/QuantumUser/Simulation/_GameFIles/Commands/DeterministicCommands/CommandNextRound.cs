using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandNextRound : DeterministicCommand
    {
        public override void Serialize(BitStream stream)
        {
        }

        public void Execute(Frame f)
        {
            if (f.Global->IsBuyPhase == false)
            {
                f.Signals.OnEndRound();
            }
            else
            {
                f.Signals.OnStartRound();
            }
        }
    }
}
