using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;

public static class PlayersQueue
{
    private static readonly List<RealtimeClient> _queue = new();
    private static readonly List<string> _clientsToConnect = new();

    public static async Task WaitForPlayerCountAsync(RealtimeClient client, int maxPlayerCount, CancellationToken cancellationToken)
    {
        _queue.Add(client);

        while (client.CurrentRoom.PlayerCount < maxPlayerCount)
        {
            Log.Debug(client.LocalPlayer.UserId);

            for (int i = 0; i < _clientsToConnect.Count; i++)
            {
                Log.Debug($"Connecting player {_clientsToConnect[i]}");
            }

            if (_clientsToConnect.Contains(client.LocalPlayer.UserId))
            {
                _clientsToConnect.Remove(client.LocalPlayer.UserId);
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Task.Yield();
        }

        Log.Debug("All players connected");

        foreach (var player in client.CurrentRoom.Players)
        {
            Log.Debug($"Player {player.Value.UserId} is connected");
            _clientsToConnect.Add(player.Value.UserId);
        }
    }
}
