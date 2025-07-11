using System;
using UnityEngine;

namespace Quantum.Game
{
    using UnityEngine.UI;

    public class ShopItemSlot : MonoBehaviour
    {
        [SerializeField] private HeroObject _hero;
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _heroParent;
        [SerializeField] private Color _baseColor;

        public event Action<ShopItemSlot> ItemClicked;

        public Vector3 HeroParentPosition => _heroParent.transform.position;
        public Vector3 SlotPosition => _background.transform.position;

        private void Awake()
        {
            SetShopItem(heroId: -1);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(ClickItem);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ClickItem);
        }

        public void SetShopItem(int heroId)
        {
            if (heroId < 0)
            {
                _heroParent.SetActive(false);
                _hero.SetHeroState(this, heroId: -1);
                _background.color = _baseColor;
            }
            else
            {
                _heroParent.SetActive(true);
                _hero.SetHeroState(this, heroId);
                Color newColor = QuantumConnection.GameConfig.GetRareColor(QuantumConnection.GetHeroInfo(heroId, out _).Rare);
                newColor.a = _baseColor.a;
                _background.color = newColor;
            }
        }

        private void ClickItem()
        {
            ItemClicked?.Invoke(this);
        }
    }
}