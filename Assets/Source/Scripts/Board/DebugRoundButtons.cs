using UnityEngine;

namespace Quantum.Game
{
    using UnityEngine.UI;

    public class DebugRoundButtons : MonoBehaviour
    {
        [SerializeField] private Button _startRoundButton;
        [SerializeField] private Button _endRoundButton;

        private void OnEnable()
        {
            _startRoundButton.onClick.AddListener(StartRound);
            _endRoundButton.onClick.AddListener(EndRound);
        }

        private void OnDisable()
        {
            _startRoundButton.onClick.RemoveListener(StartRound);
            _endRoundButton.onClick.RemoveListener(EndRound);
        }

        private void StartRound()
        {
            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                CommandStartRound commandStartRound = new();
                QuantumRunner.DefaultGame.SendCommand(commandStartRound);
            }
        }

        private void EndRound()
        {
            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                CommandEndRound commandEndRound = new();
                QuantumRunner.DefaultGame.SendCommand(commandEndRound);
            }
        }
    }
}