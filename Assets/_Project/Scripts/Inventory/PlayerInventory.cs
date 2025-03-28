using System.Collections.Generic;
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
            QuantumEvent.Subscribe<EventGetInventoryHeroes>(listener: this, handler: LoadInventory);
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
                float distance = cursorPoint.SqrDistance(_inventorySlots[i].SlotPosition);

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

        private void LoadInventory(EventGetInventoryHeroes eventGetInventoryHeroes)
        {
            if (QuantumConnection.IsPlayerMe(eventGetInventoryHeroes.PlayerRef))
            {
                ClearInventory();
                InitializeInventory(eventGetInventoryHeroes);
            }
        }

        private void InitializeInventory(EventGetInventoryHeroes eventGetInventoryHeroes)
        {
            _inventorySlots = new PlayerInventorySlot[QuantumConnection.GameConfig.InventorySize];
            QList<int> inventory = eventGetInventoryHeroes.HeroIDList;
            QList<int> levels = eventGetInventoryHeroes.HeroLevelList;

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