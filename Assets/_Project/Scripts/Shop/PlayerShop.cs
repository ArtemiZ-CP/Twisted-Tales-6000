using UnityEngine;
using TMPro;
using Quantum.Collections;

namespace Quantum.Game
{
    public class PlayerShop : MonoBehaviour
    {
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private ShopItemSlot _shopItemPrefab;
        [SerializeField] private TMP_Text _coinsText;

        private ShopItemSlot[] _shopItemSlots;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventBuyHero>(listener: this, handler: BuyHero);
            QuantumEvent.Subscribe<EventChangeCoins>(listener: this, handler: ChangeCoins);
        }

        public int GetSlotIndex(ShopItemSlot shopItemSlot)
        {
            for (int i = 0; i < _shopItemSlots.Length; i++)
            {
                if (_shopItemSlots[i] == shopItemSlot)
                {
                    return i;
                }
            }

            return -1;
        }

        public void ReloadShop(QList<int> shopItemsID)
        {
            _shopPanel.SetActive(true);
            SpawnShopItems(shopItemsID.Count);

            for (int i = 0; i < shopItemsID.Count; i++)
            {
                int itemID = shopItemsID[i];

                if (itemID < 0)
                {
                    continue;
                }

                _shopItemSlots[i].SetShopItem(itemID);
            }
        }

        public void BuyHero(ShopItemSlot shopItemSlot)
        {
            for (int i = 0; i < _shopItemSlots.Length; i++)
            {
                if (_shopItemSlots[i] == shopItemSlot)
                {
                    _shopItemSlots[i].SetShopItem(heroId: -1);
                    break;
                }
            }
        }

        private void ChangeCoins(EventChangeCoins eventChangeCoins)
        {
            if (QuantumConnection.IsPlayerMe(eventChangeCoins.PlayerRef))
            {
                SetCoins(eventChangeCoins.Coins);
            }
        }

        private void SetCoins(int coins)
        {
            _coinsText.text = $"Coins: {coins}";
        }

        private void BuyHero(EventBuyHero eventBuyHero)
        {
            if (QuantumConnection.IsPlayerMe(eventBuyHero.PlayerRef))
            {
                _shopItemSlots[eventBuyHero.ShopIndex].SetShopItem(heroId: -1);
            }
        }

        private void SpawnShopItems(int count)
        {
            ClearShopItems();

            _shopItemSlots = new ShopItemSlot[count];

            for (int i = 0; i < count; i++)
            {
                _shopItemSlots[i] = Instantiate(_shopItemPrefab, _shopPanel.transform);
            }

            foreach (var shopItem in _shopItemSlots)
            {
                shopItem.ItemClicked += TryBuyItem;
            }
        }

        private void ClearShopItems()
        {
            if (_shopItemSlots != null && _shopItemSlots.Length > 0)
            {
                foreach (var shopItem in _shopItemSlots)
                {
                    shopItem.ItemClicked -= TryBuyItem;
                }
            }

            foreach (Transform child in _shopPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void TryBuyItem(ShopItemSlot shopItem)
        {
            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                int shopIndex = -1;

                for (int i = 0; i < _shopItemSlots.Length; i++)
                {
                    if (_shopItemSlots[i] == shopItem)
                    {
                        shopIndex = i;
                        break;
                    }
                }

                CommandBuyHero commandBuyHero = new()
                {
                    ShopIndex = shopIndex,
                };

                QuantumRunner.DefaultGame.SendCommand(commandBuyHero);
            }
        }
    }
}