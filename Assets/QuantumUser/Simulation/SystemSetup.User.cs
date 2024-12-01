namespace Quantum
{
    using System.Collections.Generic;

    public static partial class DeterministicSystemSetup
    {
        static partial void AddSystemsUser(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig, SystemsConfig systemsConfig)
        {
            systems.Add(new Game.BaseHeroFightingSystem(string.Empty, new Game.Heroes.MeleeHeroSystem()));
            systems.Add(new Game.BaseHeroFightingSystem(string.Empty, new Game.Heroes.RangedHeroSystem()));
        }
    }
}