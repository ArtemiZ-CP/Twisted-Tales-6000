using Quantum;
using Quantum.Collections;
using Quantum.Game;
using TMPro;
using UnityEngine;

public class PlayersDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _playersText;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventGetCurrentPlayers>(listener: this, handler: UpdatePlayers);
    }

    private void UpdatePlayers(EventGetCurrentPlayers eventPlayers)
    {
        var players = eventPlayers.Players;
        var boards = eventPlayers.Boards;
        players.Sort((a, b) => b.Info.Health.CompareTo(a.Info.Health));
        _playersText.text = string.Empty;

        foreach (PlayerLink player in eventPlayers.Players)
        {
            if (player.Ref._index > 100)
            {
                continue;
            }

            if (QuantumConnection.IsPlayerMe(player.Ref))
                {
                    _playersText.text += GetPlayerText(Color.green, player);
                }
                else if (boards.Count > 0 && IsPlayerMyEnemy(boards, player))
                {
                    _playersText.text += GetPlayerText(Color.red, player);
                }
                else
                {
                    _playersText.text += GetPlayerText(Color.white, player);
                }
        }
    }

    private bool IsPlayerMyEnemy(QList<Quantum.Board> boards, PlayerLink enemy)
    {
        foreach (var board in boards)
        {
            if (board.Player1.Ref == enemy.Ref && QuantumConnection.IsPlayerMe(board.Player2.Ref))
            {
                return true;
            }
            else if (board.Player2.Ref == enemy.Ref && QuantumConnection.IsPlayerMe(board.Player1.Ref))
            {
                return true;
            }
        }

        return false;
    }

    private string GetPlayerText(Color textColor, PlayerLink player)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{player.Info.Nickname}: {player.Info.Health} HP</color>\n\n";
    }
}
