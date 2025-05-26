using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public class HeroName : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TMP_Text _heroNameText;
        
        private HeroMesh _heroMesh;

        private void Awake()
        {
            _canvas.worldCamera = Camera.main;
            _heroMesh = FindComponentInParents<HeroMesh>();
        }

        private void OnEnable()
        {
            _heroMesh.OnSetHero += SetHeroName;
            SetHeroName(_heroMesh.ID, _heroMesh.Level);
        }

        private void OnDisable()
        {
            _heroMesh.OnSetHero -= SetHeroName;
        }

        public static string GetHeroName(int id, int level)
        {
            if (id < 0)
            {
                return string.Empty;
            }

            string name = $"{QuantumConnection.GetHeroInfo(id, out _).Name}";

            if (level != 0)
            {
               name += $" ({level + 1})";
            }

            return name;
        }

        private void SetHeroName(int id, int level)
        {
            _heroNameText.text = GetHeroName(id, level);
        }

        private T FindComponentInParents<T>() where T : Component
        {
            Transform parent = transform.parent;

            while (parent != null)
            {
                if (parent.TryGetComponent<T>(out var component))
                {
                    return component;
                }

                parent = parent.parent;
            }

            return null;
        }
    }
}