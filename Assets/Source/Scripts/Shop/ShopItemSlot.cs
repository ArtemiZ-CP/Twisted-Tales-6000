using System;
using UnityEngine;

namespace Quantum.Game
{
    using UnityEngine.UI;

    public class ShopItemSlot : MonoBehaviour
    {
        [SerializeField] private Hero _hero;
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _heroParent;

        public event Action<ShopItemSlot> ItemClicked;

        public Vector3 HeroParentPosition => _heroParent.transform.position;

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
            }
            else
            {
                _heroParent.SetActive(true);
                _hero.SetHeroState(this, heroId);
                _background.color = QuantumConnection.GameConfig.GetHeroBackgroundColor(QuantumConnection.GetHeroInfo(heroId).Rare);
            }
        }

        private void ClickItem()
        {
            ItemClicked?.Invoke(this);
        }
    }
}