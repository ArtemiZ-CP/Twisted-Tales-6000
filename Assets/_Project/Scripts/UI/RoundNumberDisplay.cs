using Quantum;
using TMPro;
using UnityEngine;

public class RoundNumberDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _roundNumberText;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventDisplayRoundNumber>(listener: this, handler: DisplayRoundNumber);
    }

    private void DisplayRoundNumber(EventDisplayRoundNumber eventDisplayRoundNumber)
    {
        _roundNumberText.text = $"Round {eventDisplayRoundNumber.RoundNumber + 1}";
    }
}
