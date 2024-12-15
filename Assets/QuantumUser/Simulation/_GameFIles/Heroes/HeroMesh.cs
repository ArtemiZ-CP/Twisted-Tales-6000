using System;
using UnityEngine;

namespace Quantum.Game
{
    public class HeroMesh : MonoBehaviour
    {
        [SerializeField] private GameObject[] _meshes;

        public Action<int> OnSetHero;

        private int _id;

        public int ID => _id;

        public void SetMesh(int level, int ID)
        {
            _id = ID;
            OnSetHero?.Invoke(ID);

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
    }
}