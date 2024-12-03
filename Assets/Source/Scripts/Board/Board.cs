using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public class Board : MonoBehaviour
    {
        public class Tile
        {
            private Vector3 _position;
            private HeroObject _hero;

            public Vector3 Position => _position;
            public HeroObject Hero => _hero;

            public Tile(Vector3 position, HeroObject heroPrefab, Transform parent)
            {
                _position = position;
                _hero = Instantiate(heroPrefab, _position, Quaternion.identity, parent);
                _hero.transform.localScale = GameSettings.GetSize(isUIPosition: false) * Vector3.one;
                _hero.SetHeroState(this, heroId: -1);
            }

            public void SetNewHero(int heroId, int level)
            {
                _hero.SetHeroState(this, heroId);
                _hero.SetLevel(level);
                _hero.SetBasePosition();
            }
        }

        [SerializeField] private LayerMask _boardLayerMask;
        [SerializeField] private HeroObject _boardHeroPrefab;
        [Header("Board Settings")]
        [SerializeField] private bool _drawGizmos;

        private Tile[,] _tiles;
        private int _boardSize;
        private float _tileSize = 1.2f;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventGetPlayerInfo>(listener: this, handler: LoadBoard);
        }

        public bool TryGetBoardTile(out Tile tile)
        {
            if (TryGetCursorPosition(out Vector3 position) && _tiles != null)
            {
                tile = GetClosestTile(position);

                if (TryGetTileIndex(tile, out int x, out int y) && y < _boardSize / 2)
                {
                    return true;
                }
            }

            tile = null;
            return false;
        }

        public bool TryGetTileIndex(Tile tile, out int x, out int y)
        {
            for (x = 0; x < _boardSize; x++)
            {
                for (y = 0; y < _boardSize; y++)
                {
                    if (_tiles[x, y] == tile)
                    {
                        return true;
                    }
                }
            }

            x = -1;
            y = -1;
            return false;
        }

        public Vector2Int GetTileIndex(Tile tile)
        {
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize / 2; y++)
                {
                    if (_tiles[x, y] == tile)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return new Vector2Int(-1, -1);
        }

        private void LoadBoard(EventGetPlayerInfo eventGetPlayerInfo)
        {
            if (QuantumConnection.IsPlayerMe(eventGetPlayerInfo.PlayerRef))
            {
                InitializeBoard();

                QList<int> heroesID = eventGetPlayerInfo.Frame.ResolveList(eventGetPlayerInfo.PlayerInfo.Board.HeroesID);
                QList<int> heroesLevel = eventGetPlayerInfo.Frame.ResolveList(eventGetPlayerInfo.PlayerInfo.Board.HeroesLevel);

                for (int x = 0; x < _boardSize; x++)
                {
                    for (int y = 0; y < _boardSize / 2; y++)
                    {
                        GameSettings.ArrayCordsToIndex(_boardSize, x, y, out int index);
                        _tiles[x, y].SetNewHero(heroesID[index], heroesLevel[index]);
                    }
                }
            }
        }

        private void InitializeBoard()
        {
            ClearBoard();

            _boardSize = GameConfig.BoardSize;
            _tileSize = QuantumConnection.GameConfig.TileSize;

            _tiles = new Tile[_boardSize, _boardSize];

            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    Vector3 position = new Vector3(x, 0, y) * _tileSize;
                    position -= _tileSize * new Vector3(_boardSize, 0, _boardSize) / 2;
                    position += new Vector3(_tileSize, 0, _tileSize) / 2;
                    _tiles[x, y] = new Tile(position, _boardHeroPrefab, transform);
                }
            }
        }

        private bool TryGetCursorPosition(out Vector3 position)
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _boardLayerMask))
            {
                position = hit.point;
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private Tile GetClosestTile(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);

            Tile closestTile = null;
            float closestDistance = float.MaxValue;

            foreach (Tile tile in _tiles)
            {
                float distance = Vector3.Distance(tile.Position, localPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTile = tile;
                }
            }

            return closestTile;
        }

        private void ClearBoard()
        {
            _tiles = null;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            if (_drawGizmos == false)
            {
                return;
            }

            Gizmos.color = Color.white;

            if (Application.isPlaying)
            {
                for (int x = 0; x < _boardSize; x++)
                {
                    for (int y = 0; y < _boardSize; y++)
                    {
                        Gizmos.DrawWireCube(_tiles[x, y].Position, Vector3.one * _tileSize);
                    }
                }
            }
        }
    }
}