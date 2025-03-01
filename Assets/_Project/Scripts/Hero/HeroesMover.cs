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
        private EntityRef _selectedHeroRef;
        private HeroObject _selectedHero;
        private HeroObject _newHeroPlace;
        private bool _isCommandSended = false;
        private bool _isRoundStarted = false;
        private bool _isMoved = false;

        public bool IsRoundStarted => _isRoundStarted;
        public EntityRef SelectedHeroRef => _selectedHeroRef;
        public event System.Action<HeroObject> ClickedOnHero;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventMoveHero>(listener: this, handler: SwitchHeroes);
            QuantumEvent.Subscribe<EventStartRound>(listener: this, handler: StartRound);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: EndRound);
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

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _selectedHeroRef = default;
            }

            if (_isRoundStarted && UnityEngine.Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    if (hit.collider.gameObject.TryGetComponent(out QuantumEntityView entityView))
                    {
                        _selectedHeroRef = entityView.EntityRef;
                    }
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                TryGetHero();
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
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

        public bool TrySellHero(HeroObject heroToSell)
        {
            int HeroPositionX = -1;
            int HeroPositionY = -1;

            if (heroToSell.State == HeroState.Inventory)
            {
                HeroPositionX = _playerInventory.GetSlotIndex(heroToSell.PlayerInventorySlot);
            }
            else if (heroToSell.State == HeroState.Shop)
            {
                return false;
            }
            else if (heroToSell.State == HeroState.Board)
            {
                Vector2Int tileIndex = _playerBoard.GetTileIndex(heroToSell.BoardTile);
                HeroPositionX = tileIndex.x;
                HeroPositionY = tileIndex.y;
            }

            if (QuantumConnection.IsAbleToConnectQuantum())
            {
                CommandSellHero commandSellHero = new()
                {
                    HeroState = (int)heroToSell.State,
                    HeroPositionX = HeroPositionX,
                    HeroPositionY = HeroPositionY,
                };

                QuantumRunner.DefaultGame.SendCommand(commandSellHero);
            }
            else
            {
                return false;
            }

            heroToSell.SellHero();
            heroToSell.SetBaseTransform();
            SetBaseHeroSize(heroToSell);
            _selectedHero = null;
            _newHeroPlace = null;

            return true;
        }

        private void StartRound(EventStartRound eventStartRound)
        {
            _isRoundStarted = true;
            ResetSelectedHeroes();
        }

        private void EndRound(EventEndRound eventEndRound)
        {
            EndMoveHero();
            _isRoundStarted = false;
            _selectedHeroRef = default;
        }

        private bool TryGetHero()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _heroLayerMask))
            {
                if (hit.collider.gameObject.TryGetComponent(out _selectedHero) && _selectedHero.Id >= 0)
                {
                    return true;
                }
            }

            _selectedHero = null;
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
                    SetNewHeroPlace(inventorySlot.Hero, inventorySlot.HeroParentPosition, isUIScale: true, isUIRotation: true);
                }
            }
            else if (_isRoundStarted == false && _playerBoard.TryGetBoardTile(out Board.Tile boardTile))
            {
                if (boardTile.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(boardTile.Hero, boardTile.Position, isUIScale: false, isUIRotation: false);
                }
            }
            else
            {
                SetNewHeroPlace(null, newObjectPosition, isUIScale: true, isUIRotation: true);
            }
        }

        private void MoveHeroFromShop(Vector3 cursorPoint, Vector3 newObjectPosition)
        {
            if (Vector3.Distance(cursorPoint, _selectedHero.GetSlotPosition()) < _moveDistanceToStartMove)
            {
                SetNewHeroPlace(null, _selectedHero.GetBasePosition(), isUIScale: true, isUIRotation: true);
                return;
            }

            if (_playerInventory.TryGetInventoryPoint(cursorPoint, _moveDistanceToStartMove, out PlayerInventorySlot inventorySlot) && inventorySlot.IsSlotEmpty)
            {
                if (inventorySlot.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(inventorySlot.Hero, inventorySlot.HeroParentPosition, isUIScale: true, isUIRotation: true);
                }
            }
            else if (_isRoundStarted == false && _playerBoard.TryGetBoardTile(out Board.Tile boardTile) && boardTile.Hero.Id < 0)
            {
                if (boardTile.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(boardTile.Hero, boardTile.Position, isUIScale: false, isUIRotation: false);
                }
            }
            else
            {
                SetNewHeroPlace(null, newObjectPosition, isUIScale: true, isUIRotation: true);
            }
        }

        private void MoveHeroFromBoard(Vector3 cursorPoint, Vector3 newObjectPosition)
        {
            if (_isRoundStarted) return;

            if (_playerInventory.TryGetInventoryPoint(cursorPoint, _moveDistanceToStartMove, out PlayerInventorySlot inventorySlot))
            {
                if (inventorySlot.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(inventorySlot.Hero, inventorySlot.HeroParentPosition, isUIScale: true, isUIRotation: true);
                }
            }
            else if (_playerBoard.TryGetBoardTile(out Board.Tile boardTile))
            {
                if (boardTile.Hero != _newHeroPlace)
                {
                    SetNewHeroPlace(boardTile.Hero, boardTile.Position, isUIScale: false, isUIRotation: false);
                }
            }
            else
            {
                SetNewHeroPlace(null, newObjectPosition, isUIScale: true, isUIRotation: true);
            }
        }

        private void EndMoveHero()
        {
            if (_isMoved == false)
            {
                ClickedOnHero?.Invoke(_selectedHero);
            }

            _isMoved = false;

            if (_selectedHero != null && _selectedHero.Id >= 0)
            {
                if (_newHeroPlace != null && _selectedHero != _newHeroPlace)
                {
                    SendMoveCommand();
                    return;
                }
            }

            ResetSelectedHeroes();
        }

        private void ResetSelectedHeroes()
        {
            if (_selectedHero != null)
            {
                _selectedHero.SetBaseTransform();
                SetBaseHeroSize(_selectedHero);
                _selectedHero = null;
            }

            if (_newHeroPlace != null)
            {
                _newHeroPlace.SetBaseTransform();
                SetBaseHeroSize(_newHeroPlace);
                _newHeroPlace = null;
            }
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

        private void SetNewHeroPlace(HeroObject hero, Vector3 position, bool isUIScale, bool isUIRotation)
        {
            if (_newHeroPlace != null)
            {
                _newHeroPlace.SetBaseTransform();
                SetBaseHeroSize(_newHeroPlace);
            }

            _newHeroPlace = hero;

            if (hero != null)
            {
                _newHeroPlace.Move(_selectedHero.GetBasePosition());
                SetupHero(_newHeroPlace, _selectedHero.IsUI, _selectedHero.IsUI);
            }

            _selectedHero.Move(position);
            SetupHero(_selectedHero, isUIScale, isUIRotation);
        }

        private void SetBaseHeroSize(HeroObject hero, bool moveInstantly = false)
        {
            bool isUI = hero.IsUI;
            SetupHero(hero, isUIScale: isUI, isUIRotation: isUI, moveInstantly);
        }

        private void SetupHero(HeroObject hero, bool isUIScale, bool isUIRotation, bool moveInstantly = false)
        {
            hero.SetActiveShadows(isUIScale == false);

            Transform parent = hero.transform.parent;
            hero.transform.SetParent(null);

            hero.SetHeroScale(isUIScale, moveInstantly);
            hero.SetRotation(isUIRotation);

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

            if (_selectedHero != null)
            {
                _selectedHero.SetBaseTransform();
                SetBaseHeroSize(_selectedHero, moveInstantly: true);
            }

            if (_newHeroPlace != null)
            {
                _newHeroPlace.SetBaseTransform();
                SetBaseHeroSize(_newHeroPlace, moveInstantly: true);
            }

            if (_selectedHero == null || _newHeroPlace == null)
            {
                return;
            }

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