using UnityEngine;

namespace Quantum.Game
{
    using UnityEngine.UI;

    public class ShopVisibilityChanger : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _shop;
        [SerializeField] private GameObject _activatedIcon;
        [SerializeField] private GameObject _deactivatedIcon;

        private void Start()
        {
            SetActiveShop(true);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            SetActiveShop(_shop.activeSelf == false);
        }

        private void SetActiveShop(bool isActive)
        {
            _shop.SetActive(isActive);
            _activatedIcon.SetActive(isActive);
            _deactivatedIcon.SetActive(isActive == false);
        }
    }
}