using UnityEngine;

namespace Quantum.Game
{
    public class HeroRangeDisplay : MonoBehaviour
    {
        [SerializeField] private float _cellSize;
        [SerializeField] private RectTransform _rectTransform;

        private void Awake()
        {
            SetActive(false);
        }

        public void Setup(int range)
        {
            int diameter = range * 2 + 1;
            float size = diameter * _cellSize;
            _rectTransform.sizeDelta = new Vector2(size, size);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}