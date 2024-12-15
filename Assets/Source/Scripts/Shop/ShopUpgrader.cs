using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public class ShopUpgrader : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button _button;
        [SerializeField] private TMP_Text _upgradeCost;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventGetShopUpgradeCost>(listener: this, handler: UpdateCost);
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

        private void UpdateCost(EventGetShopUpgradeCost eventGetShopUpgradeCost)
        {
            if (QuantumConnection.IsPlayerMe(eventGetShopUpgradeCost.PlayerRef))
            {
                if (eventGetShopUpgradeCost.UpgradeCost < 0)
                {
                    _upgradeCost.text = string.Empty;
                }
                else
                {
                    _upgradeCost.text = eventGetShopUpgradeCost.UpgradeCost.ToString();
                }
            }
        }
    }
}