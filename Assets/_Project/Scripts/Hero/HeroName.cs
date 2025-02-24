using TMPro;
using UnityEngine;

namespace Quantum.Game
{
    public class HeroName : MonoBehaviour
    {
        [SerializeField] private TMP_Text _heroNameText;
        [SerializeField] private HeroMesh _heroMesh;

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
            string name = $"{id}";
            // string name = $"{QuantumConnection.GetHeroInfo(id).Name}";

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
    }
}