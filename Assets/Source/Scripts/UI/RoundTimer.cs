using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class RoundTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roundTimerText;
        [SerializeField] private string _buyPhaseText;
        [SerializeField] private string _combatPhaseText;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventGetRoundTime>(listener: this, handler: UpdateTime);
        }

        private void UpdateTime(EventGetRoundTime eventRoundTime)
        {
            if (eventRoundTime.IsBuyPhase)
            {
                _roundTimerText.text = $"{_buyPhaseText} {(int)eventRoundTime.RemainingTime}";
            }
            else
            {
                _roundTimerText.text = $"{_combatPhaseText} {(int)eventRoundTime.RemainingTime}";
            }
        }
    }
}