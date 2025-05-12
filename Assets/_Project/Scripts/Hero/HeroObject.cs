using UnityEngine;
using UnityEngine.Rendering;

namespace Quantum.Game
{
    public class HeroObject : MonoBehaviour
    {
        [SerializeField] private Collider _UICollider;
        [SerializeField] private Collider _boardCollider;

        private HeroState _heroState = HeroState.None;
        private PlayerInventorySlot _playerInventorySlot;
        private ShopItemSlot _shopItemSlot;
        private HeroMesh _heroMesh;
        private Board.Tile _boardTile;
        private int _id = -1;
        private int _level = 0;
        private MeshRenderer[] _meshRenderers;
        private Transform _heroTransform;
        private Coroutine _moveCoroutine;
        private Vector3 _targetPosition;
        private int _range;
        private bool _isUI;

        public HeroState State => _heroState;
        public PlayerInventorySlot PlayerInventorySlot => _playerInventorySlot;
        public ShopItemSlot ShopItemSlot => _shopItemSlot;
        public Board.Tile BoardTile => _boardTile;
        public int Id => _id;
        public int Level => _level;
        public int Range => _range;
        public bool IsUI => _heroState == HeroState.Inventory || _heroState == HeroState.Shop;

        public void SetLevel(int level)
        {
            _level = level;
            SpawnHero();
        }

        public void SetHeroState(ShopItemSlot shopItemSlot, int heroId)
        {
            _heroState = HeroState.Shop;
            _shopItemSlot = shopItemSlot;
            _boardTile = default;
            _id = heroId;
            _isUI = IsUI;
            SpawnHero();
        }

        public void SetHeroState(PlayerInventorySlot playerInventorySlot, int heroId)
        {
            _heroState = HeroState.Inventory;
            _playerInventorySlot = playerInventorySlot;
            _boardTile = default;
            _id = heroId;
            _isUI = IsUI;
            SpawnHero();
        }

        public void SetHeroState(Board.Tile boardTile, int heroId)
        {
            _heroState = HeroState.Board;
            _boardTile = boardTile;
            _playerInventorySlot = null;
            _id = heroId;
            _isUI = IsUI;
            SpawnHero();
        }

        public void SetHeroScale(bool isUI, bool moveInstantly = false)
        {
            transform.localScale = GameSettings.GetHeroSize(isUI) * Vector3.one;

            if (_isUI != isUI || moveInstantly)
            {
                if (_moveCoroutine != null)
                {
                    StopCoroutine(_moveCoroutine);
                }

                transform.position = _targetPosition;
            }

            _isUI = isUI;
        }

        public void SetBaseTransform()
        {
            Move(GetBasePosition());
            SetRotation(_heroState == HeroState.Inventory || _heroState == HeroState.Shop);
        }

        public void SellHero()
        {
            _id = -1;
            _level = 0;
            ClearHero();
        }

        public Vector3 GetBasePosition()
        {
            return _heroState switch
            {
                HeroState.Inventory => _playerInventorySlot.HeroParentPosition,
                HeroState.Shop => _shopItemSlot.HeroParentPosition,
                HeroState.Board => _boardTile.Position,
                _ => Vector3.zero
            };
        }

        public Vector3 GetSlotPosition()
        {
            return _heroState switch
            {
                HeroState.Inventory => _playerInventorySlot.SlotPosition,
                HeroState.Shop => _shopItemSlot.SlotPosition,
                HeroState.Board => _boardTile.Position,
                _ => Vector3.zero
            };
        }

        public void SetActiveShadows(bool isActive)
        {
            if (_meshRenderers == null)
            {
                return;
            }

            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                if (meshRenderer == null) continue;

                meshRenderer.shadowCastingMode = isActive ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }

        public void SetRotation(bool isUIPosition)
        {
            if (_heroTransform != null)
            {
                _heroTransform.rotation = GameSettings.GetHeroRotation(isUIPosition);
            }
        }

        public void Move(Vector3 targetPos)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _targetPosition = targetPos;

            transform.position = targetPos;
        }

        private void SpawnHero()
        {
            ClearHero();
            
            bool isUIPosition = _heroState == HeroState.Inventory || _heroState == HeroState.Shop;

            _UICollider.enabled = isUIPosition;
            _boardCollider.enabled = isUIPosition == false;

            if (_id < 0)
            {
                return;
            }

            HeroInfo heroInfo = QuantumConnection.GetAssetsList(QuantumConnection.GameConfig.HeroInfos)[_id];

            _heroMesh = Instantiate(heroInfo.HeroPrefab);
            _heroMesh.SetMesh(_level, _id);
            _range = heroInfo.HeroStats[_level].Range;
            _heroMesh.transform.localScale = GameSettings.GetHeroSize(isUIPosition) * Vector3.one;
            _heroMesh.transform.rotation = GameSettings.GetHeroRotation(isUIPosition);
            _heroMesh.transform.SetParent(transform);
            _targetPosition = GetBasePosition();
            _heroMesh.transform.position = _targetPosition;
            _heroTransform = _heroMesh.transform;
            _meshRenderers = _heroMesh.GetComponentsInChildren<MeshRenderer>();
            SetActiveShadows(isUIPosition == false);
        }

        private void ClearHero()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}