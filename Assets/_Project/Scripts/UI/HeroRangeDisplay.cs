using UnityEngine;

namespace Quantum.Game
{
    public class HeroRangeDisplay : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        private float _cellSize;

        private void Awake()
        {
            _cellSize = QuantumConnection.GameConfig.TileSize;
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