using UnityEngine;
using UnityEngine.Rendering;

namespace Quantum.Game
{
    public class HeroObject : MonoBehaviour
    {
        private HeroState _heroState = HeroState.None;
        private PlayerInventorySlot _playerInventorySlot;
        private ShopItemSlot _shopItemSlot;
        private Board.Tile _boardTile;
        private int _id = -1;
        private int _level = 0;
        private MeshRenderer _meshRenderer;

        public HeroState State => _heroState;
        public PlayerInventorySlot PlayerInventorySlot => _playerInventorySlot;
        public ShopItemSlot ShopItemSlot => _shopItemSlot;
        public Board.Tile BoardTile => _boardTile;
        public int Id => _id;
        public int Level => _level;

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
            SpawnHero();
        }

        public void SetHeroState(PlayerInventorySlot playerInventorySlot, int heroId)
        {
            _heroState = HeroState.Inventory;
            _playerInventorySlot = playerInventorySlot;
            _boardTile = default;
            _id = heroId;
            SpawnHero();
        }

        public void SetHeroState(Board.Tile boardTile, int heroId)
        {
            _heroState = HeroState.Board;
            _boardTile = boardTile;
            _playerInventorySlot = null;
            _id = heroId;
            SpawnHero();
        }

        public void SetBasePosition()
        {
            transform.position = GetBasePosition();
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

        public void SetActiveShadows(bool isActive)
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.shadowCastingMode = isActive ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
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
            mesh.SetMesh(_level);
            bool isUIPosition = _heroState == HeroState.Inventory || _heroState == HeroState.Shop;
            mesh.transform.localScale = GameSettings.GetSize(isUIPosition) * Vector3.one;
            mesh.transform.rotation = GameSettings.GetRotation(isUIPosition);
            mesh.transform.SetParent(transform);
            mesh.transform.position = GetBasePosition();
            _meshRenderer = mesh.GetComponentInChildren<MeshRenderer>();
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