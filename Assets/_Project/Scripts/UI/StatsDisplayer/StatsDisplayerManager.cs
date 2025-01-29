using UnityEngine;

namespace Quantum.Game
{
    public class StatsDisplayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject _statsArea;

        private void Awake()
        {
            _statsArea.SetActive(false);

            QuantumEvent.Subscribe<EventDisplayStats>(listener: this, handler: DisplayStats);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: HideDamageArea);
        }

        private void HideDamageArea(EventEndRound eventEndRound)
        {
            _statsArea.SetActive(false);
        }

        private void DisplayStats(EventDisplayStats eventDisplayDamage)
        {
            if (QuantumConnection.IsPlayerMe(eventDisplayDamage.Player1)
                || QuantumConnection.IsPlayerMe(eventDisplayDamage.Player2))
            {
                _statsArea.SetActive(true);
            }
        }
    }
}