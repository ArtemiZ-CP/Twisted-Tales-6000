using UnityEngine;

namespace Quantum.Game
{
    public class ShopReloader : MonoBehaviour
    {
        [SerializeField] private PlayerShop _shop;
        [SerializeField] private UnityEngine.UI.Button _button;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventInitPlayer>(listener: this, handler: InitPlayer);
            QuantumEvent.Subscribe<EventReloadShop>(listener: this, handler: ReloadShop);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(SendReloadShopCommand);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(SendReloadShopCommand);
        }

        private void SendReloadShopCommand()
        {
            QuantumConnection.OnConnectedToQuantum -= SendReloadShopCommand;

            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                CommandReloadShop commandReloadShop = new();
                QuantumRunner.DefaultGame.SendCommand(commandReloadShop);
            }
            else
            {
                QuantumConnection.OnConnectedToQuantum += SendReloadShopCommand;
            }
        }

        private void InitPlayer(EventInitPlayer eventInitPlayer)
        {
            if (QuantumConnection.IsPlayerMe(eventInitPlayer.PlayerRef))
            {
                SendReloadShopCommand();
            }
        }

        private void ReloadShop(EventReloadShop eventReloadShop)
        {
            if (QuantumConnection.IsPlayerMe(eventReloadShop.PlayerRef))
            {
                _shop.ReloadShop(eventReloadShop.HeroIDList);
                _shop.SetCoins(eventReloadShop.Coins);
            }
        }
    }
}