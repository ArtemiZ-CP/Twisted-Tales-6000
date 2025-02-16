using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BotSystem : SystemSignalsOnly, ISignalBotStartRound
    {
        public void BotStartRound(Frame f)
        {
            List<EntityRef> players = Bot.GetAllPlayerLinks(f);

            for (int i = 0; i < players.Count; i++)
            {
                EntityRef player = players[i];
                PlayerLink* playerLink = Bot.GetPlayerPointer(f, player);
                Bot.ProcessStartRound(f, playerLink);
            }
        }
    }
}