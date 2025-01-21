using System.Collections;
using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public class CoinsRoundRewardDisplay : MonoBehaviour
    {
        [SerializeField] private float _timeToShow;
        [SerializeField] private GameObject _textParent;
        [SerializeField] private TMP_Text _coinsRewardText;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventShowCoinsReward>(listener: this, handler: ShowCoinsReward);
        }

        private void Start()
        {
            _textParent.SetActive(false);
        }

        private void ShowCoinsReward(EventShowCoinsReward eventShowCoinsReward)
        {
            if (QuantumConnection.IsPlayerMe(eventShowCoinsReward.PlayerRef))
            {
                StartCoroutine(ShowCoinsRewardCoroutine(eventShowCoinsReward));
            }
        }

        private IEnumerator ShowCoinsRewardCoroutine(EventShowCoinsReward eventShowCoinsReward)
        {
            _coinsRewardText.text = GetRewardText(eventShowCoinsReward);
            _textParent.SetActive(true);
            yield return new WaitForSeconds(_timeToShow);
            _textParent.SetActive(false);
        }

        private string GetRewardText(EventShowCoinsReward eventShowCoinsReward)
        {
            string text;
            string resultText;

            if (eventShowCoinsReward.RoundResult < 0)
            {
                text = "You lost";
                resultText = "lose";
            }
            else if (eventShowCoinsReward.RoundResult > 0)
            {
                text = "You won";
                resultText = "win";
            }
            else
            {
                text = "Draw";
                resultText = "draw";
            }

            text += $"\n\nBase coins:: {eventShowCoinsReward.BaseCoins}\n";

            if (eventShowCoinsReward.RoundResultCoins > 0)
            {
                text += $"Coins for {resultText}: {eventShowCoinsReward.RoundResultCoins}\n";
            }

            if (eventShowCoinsReward.StreakCoins > 0)
            {
                text += $"Coins for {resultText} streak: {eventShowCoinsReward.StreakCoins}\n";
            }

            return text;
        }
    }
}