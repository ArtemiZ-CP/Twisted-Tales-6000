using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class CommandSellHero : DeterministicCommand
    {
        public int HeroState;
        public int HeroPositionX;
        public int HeroPositionY;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref HeroState);
            stream.Serialize(ref HeroPositionX);
            stream.Serialize(ref HeroPositionY);
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.SellHero(playerLink, (HeroState)HeroState, HeroPositionX, HeroPositionY);
        }
    }
}