using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public class ShopUpgrader : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button _button;
        [SerializeField] private TMP_Text _upgradeCost;
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
                if (eventGetShopUpgradeCost.UpgradeCost < 0)
                {
                    _upgradeCost.text = string.Empty;
                }
                else
                {
                    _upgradeCost.text = $"{eventGetShopUpgradeCost.UpgradeCost} coins";
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