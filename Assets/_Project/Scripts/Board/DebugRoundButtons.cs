using UnityEngine;

namespace Quantum.Game
{
    using UnityEngine.UI;

    public class DebugRoundButtons : MonoBehaviour
    {
        [SerializeField] private Button _nextRoundButton;

        private void OnEnable()
        {
            _nextRoundButton.onClick.AddListener(NextRound);
        }

        private void OnDisable()
        {
            _nextRoundButton.onClick.RemoveListener(NextRound);
        }

        private void NextRound()
        {
            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                QuantumRunner.DefaultGame.SendCommand(new CommandNextRound());
            }
        }
    }
}