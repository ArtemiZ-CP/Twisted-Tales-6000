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
            SetHeroName(_heroMesh.ID);
            _heroMesh.OnSetHero += SetHeroName;
        }

        private void OnDisable()
        {
            _heroMesh.OnSetHero -= SetHeroName;
        }

        private void SetHeroName(int id)
        {
            _heroNameText.text = QuantumConnection.GetHeroInfo(id).Name;
        }
    }
}