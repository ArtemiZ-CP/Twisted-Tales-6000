using Photon.Deterministic;

namespace Quantum.Game
{
    public enum HeroState
    {
        None,
        Shop,
        Board,
        Inventory
    }

    public unsafe class CommandMoveHero : DeterministicCommand
    {
        public int HeroFromState;
        public int HeroToState;
        public int HeroPositionFromX;
        public int HeroPositionFromY;
        public int HeroPositionToX;
        public int HeroPositionToY;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref HeroFromState);
            stream.Serialize(ref HeroToState);
            stream.Serialize(ref HeroPositionFromX);
            stream.Serialize(ref HeroPositionFromY);
            stream.Serialize(ref HeroPositionToX);
            stream.Serialize(ref HeroPositionToY);
        }

        public void Execute(Frame f, PlayerLink* playerLink)
        {
            f.Signals.OnMoveHero(playerLink, (HeroState)HeroFromState, (HeroState)HeroToState, HeroPositionFromX, HeroPositionFromY, HeroPositionToX, HeroPositionToY);
        }
    }
}