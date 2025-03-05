using System;
using UnityEngine;

namespace Quantum.Game
{
    public class HeroMesh : MonoBehaviour
    {
        [SerializeField] private GameObject[] _meshes;
        [SerializeField] private HeroRangeDisplay _heroRangeDisplay;

        public Action<int, int> OnSetHero;

        private int _id;
        private int _level;

        public int ID => _id;
        public int Level => _level;

        public void SetMesh(int level, int id)
        {
            _id = id;
            _level = level;
            OnSetHero?.Invoke(id, level);

            foreach (var mesh in _meshes)
            {
                mesh.SetActive(false);
            }

            if (level < 0 || level >= _meshes.Length)
            {
                return;
            }

            _meshes[level].SetActive(true);
        }

        public void SetRange(int range)
        {
            _heroRangeDisplay.Setup(range);
        }

        public void SetActiveRange(bool isActive)
        {
            _heroRangeDisplay.SetActive(isActive);
        }
    }
}
