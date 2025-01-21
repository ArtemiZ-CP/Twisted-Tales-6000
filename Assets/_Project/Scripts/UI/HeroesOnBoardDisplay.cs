using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public class HeroesOnBoardDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventShowHeroesOnBoardCount>(listener: this, handler: DisplayHeroesCount);
        }

        private void DisplayHeroesCount(EventShowHeroesOnBoardCount eventShowHeroesOnBoardCount)
        {
            if (QuantumConnection.IsPlayerMe(eventShowHeroesOnBoardCount.PlayerRef))
            {
                _text.text = $"{eventShowHeroesOnBoardCount.HeroesOnBoard}/{eventShowHeroesOnBoardCount.MaxHeroesOnBoard}";
            }
        }
    }
}