using UnityEngine;

namespace Quantum.Game
{
    public class GameSettings : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private Board _board;
        [SerializeField] private float _uiSize = 0.096f;
        [SerializeField] private float _boardSize = 1f;

        private static GameSettings _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static float GetHeroSize(bool isUIPosition)
        {
            return isUIPosition ? _instance._uiSize : _instance._boardSize;
        }

        public static Quaternion GetHeroRotation(bool isUIPosition)
        {
            Quaternion uiRotation = _instance._playerInventory.transform.rotation * Quaternion.Euler(0, 180, 0);

            return isUIPosition ? uiRotation : _instance._board.transform.rotation;
        }

        public static void ArrayIndexToCords(int arraySize, int index, out int x, out int y)
        {
            x = index % arraySize;
            y = index / arraySize;
        }

        public static void ArrayCordsToIndex(int arraySize, int x, int y, out int index)
        {
            index = y * arraySize + x;
        }
    }
}