using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class RoundTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roundTimerText;
        [SerializeField] private string _buyPhaseText;
        [SerializeField] private string _pvpPhaseText;
        [SerializeField] private string _pvePhaseText;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventGetRoundTime>(listener: this, handler: UpdateTime);
        }

        private void Start()
        {
            _roundTimerText.text = "Waiting for another players...";
        }

        private void UpdateTime(EventGetRoundTime eventRoundTime)
        {
            if (eventRoundTime.IsBuyPhase)
            {
                _roundTimerText.text = $"{_buyPhaseText} {(int)eventRoundTime.RemainingTime}";
            }
            else
            {
                if (eventRoundTime.IsPVPRound)
                {
                    _roundTimerText.text = $"{_pvpPhaseText} {(int)eventRoundTime.RemainingTime}";
                }
                else
                {
                    _roundTimerText.text = $"{_pvePhaseText} {(int)eventRoundTime.RemainingTime}";
                }
            }
        }
    }
}