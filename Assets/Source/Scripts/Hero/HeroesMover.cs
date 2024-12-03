using UnityEngine;

namespace Quantum.Game
{
    public class HeroesMover : MonoBehaviour
    {
        [SerializeField] private LayerMask _heroLayerMask;
        [SerializeField] private float _moveDistanceToStartMove = 0.5f;
        [SerializeField] private float _moveDistanceFromCameraToUI = 1f;
        [SerializeField] private float _moveDistanceFromCameraToMove = 1f;
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private PlayerShop _playerShop;
        [SerializeField] private Board _playerBoard;

        private Camera _camera;
        private HeroObject _selectedHero;
        private HeroObject _newHeroPlace;
        private bool _isMovingHero = false;
        private bool _isCommandSended = false;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventMoveHero>(listener: this, handler: SwitchHeroes);
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_isCommandSended)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0) && _isMovingHero == false)
            {
                _isMovingHero = TryGetHero();
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) && _selectedHero != null)
            {
                EndMoveHero();
            }
        }

        private void FixedUpdate()
        {
            if (_isCommandSended)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButton(0) && _selectedHero != null)
            {
                MoveHero();
            }
        }

        private bool TryGetHero()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _heroLayerMask))
            {
                if (hit.collider.gameObject.TryGetComponent(out _selectedHero))
                {
                    return true;
                }
            }

            return false;
        }

        private void MoveHero()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            Vector3 planeNormal = _camera.transform.forward;
            Vector3 planePointUI = _camera.transform.position + planeNormal * _moveDistanceFromCameraToUI;
            Vector3 planePointMove = _camera.transform.position + planeNormal * _moveDistanceFromCameraToMove;

            Plane planeUI = new(planeNormal, planePointUI);
            Plane planeMove = new(planeNormal, planePointMove);

            if (planeUI.Raycast(ray, out float enterUI) && planeMove.Raycast(ray, out float enterMove))
            {
                MoveHero(ray.GetPoint(enterUI), ray.GetPoint(enterMove));
            }
        }

        private void MoveHero(Vector3 cursorPoint, Vector3 newObjectPosition)
        {
            if (_selectedHero.State == HeroState.Inventory)
            {
                MoveHeroFromInventory(cursorPoint, newObjectPosition);
            }
            else if (_selectedHero.State == HeroState.Shop)
            {
                MoveHeroFromShop(cursorPoint, newObjectPosition);
            }
            else if (_selectedHero.State == HeroState.Board)
            {
                MoveHeroFromBoard(cursorPoint, newObjectPosition);
            }
        }

        private void MoveHeroFromInventory(Vector3 cursorPoint, Vector3 newObjectPosition)
        {
            if (_playerInventory.TryGetInventoryPoint(cursorPoint, _moveDistanceToStartMove, out PlayerInventorySlot inventorySlot))
            {
                if (inventorySlot.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(inventorySlot.Hero, isUIPosition: true);
                    SetHeroPosition(inventorySlot.HeroParentPosition, isUIPosition: true);
                }
            }
            else if (_playerBoard.TryGetBoardTile(out Board.Tile boardTile))
            {
                if (boardTile.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(boardTile.Hero, isUIPosition: true);
                    SetHeroPosition(boardTile.Position, isUIPosition: false);
                }
            }
            else
            {
                SetNewHeroPlace(null, isUIPosition: true);
                SetHeroPosition(newObjectPosition, isUIPosition: true);
            }
        }

        private void MoveHeroFromShop(Vector3 cursorPoint, Vector3 newObjectPosition)
        {
            if (Vector3.Distance(cursorPoint, _selectedHero.GetBasePosition()) < _moveDistanceToStartMove)
            {
                SetNewHeroPlace(null, isUIPosition: true);
                SetHeroPosition(_selectedHero.GetBasePosition(), isUIPosition: true);
                return;
            }

            if (_playerInventory.TryGetInventoryPoint(cursorPoint, _moveDistanceToStartMove, out PlayerInventorySlot inventorySlot) && inventorySlot.IsSlotEmpty)
            {
                if (inventorySlot.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(inventorySlot.Hero, isUIPosition: true);
                    SetHeroPosition(inventorySlot.HeroParentPosition, isUIPosition: true);
                }
            }
            else if (_playerBoard.TryGetBoardTile(out Board.Tile boardTile) && boardTile.Hero.Id < 0)
            {
                if (boardTile.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(boardTile.Hero, isUIPosition: true);
                    SetHeroPosition(boardTile.Position, isUIPosition: false);
                }
            }
            else
            {
                SetNewHeroPlace(null, isUIPosition: true);
                SetHeroPosition(newObjectPosition, isUIPosition: true);
            }
        }

        private void MoveHeroFromBoard(Vector3 cursorPoint, Vector3 newObjectPosition)
        {
            if (_playerInventory.TryGetInventoryPoint(cursorPoint, _moveDistanceToStartMove, out PlayerInventorySlot inventorySlot))
            {
                if (inventorySlot.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(inventorySlot.Hero, isUIPosition: false);
                    SetHeroPosition(inventorySlot.HeroParentPosition, isUIPosition: true);
                }
            }
            else if (_playerBoard.TryGetBoardTile(out Board.Tile boardTile))
            {
                if (boardTile.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(boardTile.Hero, isUIPosition: false);
                    SetHeroPosition(boardTile.Position, isUIPosition: false);
                }
            }
            else
            {
                SetNewHeroPlace(null, isUIPosition: true);
                SetHeroPosition(newObjectPosition, isUIPosition: true);
            }
        }

        private void EndMoveHero()
        {
            if (_newHeroPlace != null && _selectedHero != _newHeroPlace)
            {
                SendMoveCommand();
            }
            else
            {
                _selectedHero.SetBasePosition();
                SetBaseHeroSize(_selectedHero);
                _selectedHero = null;
                _newHeroPlace = null;
            }

            _isMovingHero = false;
        }

        private void SendMoveCommand()
        {
            _isCommandSended = true;

            int HeroPositionFromX = -1;
            int HeroPositionFromY = -1;
            int HeroPositionToX = -1;
            int HeroPositionToY = -1;

            if (_selectedHero.State == HeroState.Inventory)
            {
                HeroPositionFromX = _playerInventory.GetSlotIndex(_selectedHero.PlayerInventorySlot);
            }
            else if (_selectedHero.State == HeroState.Shop)
            {
                HeroPositionFromX = _playerShop.GetSlotIndex(_selectedHero.ShopItemSlot);
            }
            else if (_selectedHero.State == HeroState.Board)
            {
                Vector2Int tileIndex = _playerBoard.GetTileIndex(_selectedHero.BoardTile);
                HeroPositionFromX = tileIndex.x;
                HeroPositionFromY = tileIndex.y;
            }

            if (_newHeroPlace.State == HeroState.Inventory)
            {
                HeroPositionToX = _playerInventory.GetSlotIndex(_newHeroPlace.PlayerInventorySlot);
            }
            else if (_newHeroPlace.State == HeroState.Shop)
            {
                HeroPositionToX = _playerShop.GetSlotIndex(_newHeroPlace.ShopItemSlot);
            }
            else if (_newHeroPlace.State == HeroState.Board)
            {
                Vector2Int tileIndex = _playerBoard.GetTileIndex(_newHeroPlace.BoardTile);
                HeroPositionToX = tileIndex.x;
                HeroPositionToY = tileIndex.y;
            }

            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                CommandMoveHero commandMoveHero = new()
                {
                    HeroFromState = (int)_selectedHero.State,
                    HeroToState = (int)_newHeroPlace.State,
                    HeroPositionFromX = HeroPositionFromX,
                    HeroPositionFromY = HeroPositionFromY,
                    HeroPositionToX = HeroPositionToX,
                    HeroPositionToY = HeroPositionToY
                };

                QuantumRunner.DefaultGame.SendCommand(commandMoveHero);
            }
            else
            {
                SwitchHeroes(false);
            }
        }

        private void SetNewHeroPlace(HeroObject hero, bool isUIPosition)
        {
            if (_newHeroPlace != null)
            {
                _newHeroPlace.SetBasePosition();
                SetBaseHeroSize(_newHeroPlace);
            }

            _newHeroPlace = hero;

            if (hero != null)
            {
                _newHeroPlace.transform.position = _selectedHero.GetBasePosition();
                SetupHero(_newHeroPlace, isUIPosition);
            }
        }

        private void SetHeroPosition(Vector3 position, bool isUIPosition)
        {
            _selectedHero.transform.position = position;
            SetupHero(_selectedHero, isUIPosition);
        }

        private void SetBaseHeroSize(HeroObject hero)
        {
            SetupHero(hero, isUIPosition: hero.State == HeroState.Inventory || hero.State == HeroState.Shop);
        }

        private void SetupHero(HeroObject hero, bool isUIPosition)
        {
            hero.SetActiveShadows(isUIPosition == false);

            Transform parent = hero.transform.parent;
            hero.transform.SetParent(null);

            hero.transform.localScale = GameSettings.GetSize(isUIPosition) * Vector3.one;
            hero.transform.rotation = GameSettings.GetRotation(isUIPosition);

            hero.transform.SetParent(parent);
        }

        private void SwitchHeroes(EventMoveHero eventMoveHero)
        {
            if (QuantumConnection.IsPlayerMe(eventMoveHero.PlayerRef))
            {
                SwitchHeroes(eventMoveHero.IsMoved);
            }
        }

        private void SwitchHeroes(bool isMoved)
        {
            _isCommandSended = false;

            _selectedHero.SetBasePosition();
            SetBaseHeroSize(_selectedHero);
            _newHeroPlace.SetBasePosition();
            SetBaseHeroSize(_newHeroPlace);

            if (isMoved == false)
            {
                _selectedHero = null;
                _newHeroPlace = null;
                return;
            }

            int heroId = _selectedHero.Id;
            int heroLevel = _selectedHero.Level;
            int newHeroId = _newHeroPlace.Id;
            int newHeroLevel = _newHeroPlace.Level;

            if (_selectedHero.State == HeroState.Inventory)
            {
                if (_newHeroPlace.State == HeroState.Inventory)
                {
                    _selectedHero.PlayerInventorySlot.SetInventoryItem(newHeroId, newHeroLevel);
                    _newHeroPlace.PlayerInventorySlot.SetInventoryItem(heroId, heroLevel);
                }
                else if (_newHeroPlace.State == HeroState.Board)
                {
                    _selectedHero.PlayerInventorySlot.SetInventoryItem(newHeroId, newHeroLevel);
                    _newHeroPlace.BoardTile.SetNewHero(heroId, heroLevel);
                }
            }
            else if (_selectedHero.State == HeroState.Shop)
            {
                if (_newHeroPlace.State == HeroState.Inventory)
                {
                    _playerShop.BuyHero(_selectedHero.ShopItemSlot);
                    _playerInventory.BuyHero(heroId, _playerInventory.GetSlotIndex(_newHeroPlace.PlayerInventorySlot));
                }
                else if (_newHeroPlace.State == HeroState.Board)
                {
                    _playerShop.BuyHero(_selectedHero.ShopItemSlot);
                    _newHeroPlace.BoardTile.SetNewHero(heroId, heroLevel);
                }
            }
            else if (_selectedHero.State == HeroState.Board)
            {
                if (_newHeroPlace.State == HeroState.Inventory)
                {
                    _selectedHero.BoardTile.SetNewHero(newHeroId, newHeroLevel);
                    _newHeroPlace.PlayerInventorySlot.SetInventoryItem(heroId, heroLevel);
                }
                else if (_newHeroPlace.State == HeroState.Board)
                {
                    _selectedHero.BoardTile.SetNewHero(newHeroId, newHeroLevel);
                    _newHeroPlace.BoardTile.SetNewHero(heroId, heroLevel);
                }
            }

            _selectedHero = null;
            _newHeroPlace = null;
        }
    }
}