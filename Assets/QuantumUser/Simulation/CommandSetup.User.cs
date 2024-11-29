namespace Quantum
{
    using System.Collections.Generic;
    using Photon.Deterministic;

    public static partial class DeterministicCommandSetup
    {
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            factories.Add(new Game.CommandStartRound());
            factories.Add(new Game.CommandEndRound());
            factories.Add(new Game.CommandReloadShop());
            factories.Add(new Game.CommandBuyHero());
            factories.Add(new Game.CommandMoveHero());
            factories.Add(new Game.CommandGetPlayerInfo());
        }
    }
}