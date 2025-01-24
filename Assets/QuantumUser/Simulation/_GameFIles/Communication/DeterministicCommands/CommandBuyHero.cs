using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandBuyHero : DeterministicCommand
    {
        public int ShopIndex;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref ShopIndex);
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.OnBuyHero(playerLink, ShopIndex);
        }
    }
}