using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    using UnityEngine.UI;
    
    public class ShopUpgrader : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _progress;
        [SerializeField] private TMP_Text _xp;
        [SerializeField] private TMP_Text _level;
        [SerializeField] private TMP_Text _heroesChance;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventGetShopUpgradeInfo>(listener: this, handler: UpdateInfo);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(SendUpgradeShopCommand);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(SendUpgradeShopCommand);
        }

        private void SendUpgradeShopCommand()
        {
            QuantumConnection.OnConnectedToQuantum -= SendUpgradeShopCommand;

            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                QuantumRunner.DefaultGame.SendCommand(new CommandUpgradeShop());
            }
            else
            {
                QuantumConnection.OnConnectedToQuantum += SendUpgradeShopCommand;
            }
        }

        private void UpdateInfo(EventGetShopUpgradeInfo eventGetShopUpgradeCost)
        {
            if (QuantumConnection.IsPlayerMe(eventGetShopUpgradeCost.PlayerRef))
            {
                _level.text = $"Level: {eventGetShopUpgradeCost.CurrentLevel + 1}";

                if (eventGetShopUpgradeCost.CurrentXP < 0)
                {
                    _xp.text = string.Empty;
                    _progress.fillAmount = 1;
                }
                else
                {
                    _xp.text = $"{eventGetShopUpgradeCost.CurrentXP}/{eventGetShopUpgradeCost.MaxXPCost}";
                    _progress.fillAmount = (float)eventGetShopUpgradeCost.CurrentXP / eventGetShopUpgradeCost.MaxXPCost;
                }

                string text = string.Empty;

                for (int i = 0; i < eventGetShopUpgradeCost.HeroChanceList.Count; i++)
                {
                    int chance = Mathf.RoundToInt(eventGetShopUpgradeCost.HeroChanceList[i] * 100);
                    Color color = QuantumConnection.GameConfig.GetRareColor((HeroRare)i);

                    text += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{(HeroRare)i}</color>: {chance}%\n";
                }

                _heroesChance.text = text;
            }
        }
    }
}