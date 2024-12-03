using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

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
            QuantumEvent.Subscribe<EventGetPlayerInfo>(listener: this, handler: LoadShop);
            QuantumEvent.Subscribe<EventStartRound>(listener: this, handler: HideShop);
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

        public void ReloadShop(List<int> shopItemsID)
        {
            _shopPanel.SetActive(true);
            SpawnShopItems(shopItemsID.Count);

            for (int i = 0; i < shopItemsID.Count; i++)
            {
                if (shopItemsID[i] < 0)
                {
                    continue;
                }

                _shopItemSlots[i].SetShopItem(shopItemsID[i]);
            }
        }

        public void BuyHero(ShopItemSlot shopItemSlot)
        {
            int index = _shopItemSlots.ToList().IndexOf(shopItemSlot);
            _shopItemSlots[index].SetShopItem(heroId: -1);
        }

        private void HideShop(EventStartRound eventStartRound)
        {
            if (QuantumConnection.IsPlayerMe(eventStartRound.Player1) || QuantumConnection.IsPlayerMe(eventStartRound.Player2))
            {
                _shopPanel.SetActive(false);
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

        private void LoadShop(EventGetPlayerInfo eventGetPlayerInfo)
        {
            if (QuantumConnection.IsPlayerMe(eventGetPlayerInfo.PlayerRef))
            {
                ReloadShop(eventGetPlayerInfo.Frame.ResolveList(eventGetPlayerInfo.PlayerInfo.Shop.HeroesID).ToList());
                SetCoins(eventGetPlayerInfo.PlayerInfo.Coins);
            }
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
                CommandBuyHero commandBuyHero = new()
                {
                    ShopIndex = _shopItemSlots.ToList().IndexOf(shopItem),
                };

                QuantumRunner.DefaultGame.SendCommand(commandBuyHero);
            }
        }
    }
}