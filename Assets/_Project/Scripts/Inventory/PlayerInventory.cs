using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private Transform _inventoryParent;
        [SerializeField] private PlayerInventorySlot _playerInventorySlotPrefab;

        private PlayerInventorySlot[] _inventorySlots;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventBuyHero>(listener: this, handler: BuyHero);
            QuantumEvent.Subscribe<EventGetPlayerInfo>(listener: this, handler: LoadInventory);
        }

        public int GetSlotIndex(PlayerInventorySlot inventorySlot)
        {
            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                if (_inventorySlots[i] == inventorySlot)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool TryGetInventoryPoint(Vector3 cursorPoint, float distanceToStartMove, out PlayerInventorySlot inventorySlot)
        {
            PlayerInventorySlot closestSlot = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                float distance = Vector3.Distance(cursorPoint, _inventorySlots[i].SlotPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = _inventorySlots[i];
                }
            }

            if (closestDistance < distanceToStartMove)
            {
                inventorySlot = closestSlot;
                return true;
            }

            inventorySlot = null;
            return false;
        }

        public void BuyHero(int heroID, int inventoryIndex)
        {
            _inventorySlots[inventoryIndex].SetInventoryItem(heroID, 0);
        }

        private void LoadInventory(EventGetPlayerInfo eventGetPlayerInfo)
        {
            if (QuantumConnection.IsPlayerMe(eventGetPlayerInfo.PlayerRef))
            {
                ClearInventory();
                InitializeInventory(eventGetPlayerInfo.Frame, eventGetPlayerInfo.PlayerInfo);
            }
        }

        private void InitializeInventory(Frame frame, PlayerInfo playerInfo)
        {
            _inventorySlots = new PlayerInventorySlot[QuantumConnection.GameConfig.InventorySize];
            QList<int> inventory = frame.ResolveList(playerInfo.Inventory.HeroesID);
            QList<int> levels = frame.ResolveList(playerInfo.Inventory.HeroesLevel);

            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                PlayerInventorySlot slot = Instantiate(_playerInventorySlotPrefab, _inventoryParent);
                slot.SetInventoryItem(heroId: inventory[i], levels[i]);
                _inventorySlots[i] = slot;
            }
        }

        private void BuyHero(EventBuyHero eventBuyHero)
        {
            if (QuantumConnection.IsPlayerMe(eventBuyHero.PlayerRef) && eventBuyHero.InventoryIndex >= 0)
            {
                _inventorySlots[eventBuyHero.InventoryIndex].SetInventoryItem(eventBuyHero.HeroID, 0);
            }
        }

        private void ClearInventory()
        {
            foreach (Transform child in _inventoryParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}