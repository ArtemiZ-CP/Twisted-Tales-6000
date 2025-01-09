using UnityEngine.Scripting;
using Photon.Deterministic;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerCommandsSystem : SystemMainThreadFilter<PlayerCommandsSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* Player;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            PlayerLink* player = filter.Player;
            DeterministicCommand command = f.GetPlayerCommand(player->Ref);

            var commandStartRound = command as CommandStartRound;
            commandStartRound?.Execute(f);

            var commandEndRound = command as CommandEndRound;
            commandEndRound?.Execute(f);

            var commandReloadShop = command as CommandReloadShop;
            commandReloadShop?.Execute(f, player);

            var commandBuyHero = command as CommandBuyHero;
            commandBuyHero?.Execute(f, player);

            var commandMoveHero = command as CommandMoveHero;
            commandMoveHero?.Execute(f, player);

            var commandGetPlayerInfo = command as CommandGetPlayerInfo;
            commandGetPlayerInfo?.Execute(f, player);

            var commandUpgradeShop = command as CommandUpgradeShop;
            commandUpgradeShop?.Execute(f, player);
        }
    }
}