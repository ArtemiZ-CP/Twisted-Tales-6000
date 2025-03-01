using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace Quantum.Game
{
    public class HeroObject : MonoBehaviour
    {
        private const float BoardMoveSpeed = 20f;

        private HeroState _heroState = HeroState.None;
        private PlayerInventorySlot _playerInventorySlot;
        private ShopItemSlot _shopItemSlot;
        private Board.Tile _boardTile;
        private int _id = -1;
        private int _level = 0;
        private MeshRenderer[] _meshRenderers;
        private Transform _heroTransform;
        private Coroutine _moveCoroutine;
        private Vector3 _targetPosition;
        private bool _isUI;

        public HeroState State => _heroState;
        public PlayerInventorySlot PlayerInventorySlot => _playerInventorySlot;
        public ShopItemSlot ShopItemSlot => _shopItemSlot;
        public Board.Tile BoardTile => _boardTile;
        public int Id => _id;
        public int Level => _level;
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

        public void Move(Vector3 targetPos)
        {
            Move(targetPos, BoardMoveSpeed * GameSettings.GetHeroSize(_isUI));
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

        private void Move(Vector3 targetPos, float speed)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _targetPosition = targetPos;

            if (gameObject.activeInHierarchy)
            {
                // transform.position = targetPos;
                _moveCoroutine = StartCoroutine(MoveToPositionCoroutine(targetPos, speed));
            }
            else
            {
                transform.position = targetPos;
            }
        }

        private IEnumerator MoveToPositionCoroutine(Vector3 targetPos, float speed)
        {
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;
        }

        private void SpawnHero()
        {
            ClearHero();

            if (_id < 0)
            {
                return;
            }

            HeroInfo heroInfos = QuantumConnection.GetAssetsList(QuantumConnection.GameConfig.HeroInfos)[_id];

            HeroMesh mesh = Instantiate(heroInfos.HeroPrefab);
            mesh.SetMesh(_level, _id);
            bool isUIPosition = _heroState == HeroState.Inventory || _heroState == HeroState.Shop;
            mesh.transform.localScale = GameSettings.GetHeroSize(isUIPosition) * Vector3.one;
            mesh.transform.rotation = GameSettings.GetHeroRotation(isUIPosition);
            mesh.transform.SetParent(transform);
            _targetPosition = GetBasePosition();
            mesh.transform.position = _targetPosition;
            _heroTransform = mesh.transform;
            _meshRenderers = mesh.GetComponentsInChildren<MeshRenderer>();
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