namespace Quantum
{
    using System.Collections.Generic;
    using Photon.Deterministic;

    public static partial class DeterministicCommandSetup
    {
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            factories.Add(new Game.CommandNextRound());
            factories.Add(new Game.CommandReloadShop());
            factories.Add(new Game.CommandBuyHero());
            factories.Add(new Game.CommandMoveHero());
            factories.Add(new Game.CommandUpgradeShop());
            factories.Add(new Game.CommandSellHero());
            factories.Add(new Game.CommandFreezeShop());
            factories.Add(new Game.CommandGetHeroInfo());
        }
    }
}