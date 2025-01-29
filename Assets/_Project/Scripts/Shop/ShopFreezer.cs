using UnityEngine;

namespace Quantum.Game
{
    public class ShopFreezer : MonoBehaviour
    {
        [SerializeField] private GameObject _open;
        [SerializeField] private GameObject _closed;
        [SerializeField] private UnityEngine.UI.Button _button;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventFreezeShop>(listener: this, handler: FreezeShop);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(SendFreezeShopCommand);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(SendFreezeShopCommand);
        }

        private void SendFreezeShopCommand()
        {
            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                QuantumRunner.DefaultGame.SendCommand(new CommandFreezeShop());
            }
        }

        private void FreezeShop(EventFreezeShop eventFreezeShop)
        {
            if (QuantumConnection.IsPlayerMe(eventFreezeShop.PlayerRef))
            {
                _open.SetActive(eventFreezeShop.Freezed == false);
                _closed.SetActive(eventFreezeShop.Freezed);
            }
        }
    }
}