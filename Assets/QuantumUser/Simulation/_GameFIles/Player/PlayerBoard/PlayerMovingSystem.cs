using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerMovingSystem : SystemSignalsOnly, ISignalOnMoveHero
    {
        QList<int> _inventory;
        QList<int> _inventoryLevels;
        QList<int> _board;
        QList<int> _boardLevels;
        QList<int> _shop;
        PlayerLink* _playerLink;
        Frame _frame;

        public void OnMoveHero(Frame f, PlayerLink* playerLink, HeroState HeroFromState, HeroState HeroToState, int positionFromX, int positionFromY, int positionToX, int positionToY)
        {
            _inventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            _inventoryLevels = f.ResolveList(playerLink->Info.Inventory.HeroesLevel);
            _board = f.ResolveList(playerLink->Info.Board.HeroesID);
            _boardLevels = f.ResolveList(playerLink->Info.Board.HeroesLevel);
            _shop = f.ResolveList(playerLink->Info.Shop.HeroesID);
            _playerLink = playerLink;
            _frame = f;

            int boardFromIndex = positionFromY * GameConfig.BoardSize + positionFromX;
            int boardToIndex = positionToY * GameConfig.BoardSize + positionToX;

            if (HeroFromState == HeroState.Inventory)
            {
                if (HeroToState == HeroState.Inventory)
                {
                    FromIToI(positionFromX, positionToX);
                }
                else if (HeroToState == HeroState.Board)
                {
                    FromIToB(positionFromX, boardToIndex);
                }
            }
            else if (HeroFromState == HeroState.Shop)
            {
                HeroInfo heroInfo = GetBuyingHero(f, _shop[positionFromX]);

                if (heroInfo == null || playerLink->Info.Coins < heroInfo.GetCost(f))
                {
                    _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
                    return;
                }

                if (HeroToState == HeroState.Inventory)
                {
                    FromSToI(positionFromX, positionToX, heroInfo.GetCost(f));
                }
                else if (HeroToState == HeroState.Board)
                {
                    FromSToB(positionFromX, boardToIndex, heroInfo.GetCost(f));
                }
            }
            else if (HeroFromState == HeroState.Board)
            {
                if (HeroToState == HeroState.Inventory)
                {
                    FromBToI(boardFromIndex, positionToX);
                }
                else if (HeroToState == HeroState.Board)
                {
                    FromBToB(boardFromIndex, boardToIndex);
                }
            }
        }

        private HeroInfo GetBuyingHero(Frame f, int heroID)
        {
            if (heroID < 0)
            {
                return null;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            AssetRef<HeroInfo>[] heroInfos = gameConfig.HeroInfos;
            HeroInfo heroInfo = null;

            for (int i = 0; i < heroInfos.Length; i++)
            {
                heroInfo = f.FindAsset(heroInfos[i]);

                if (i == heroID)
                {
                    break;
                }
            }

            return heroInfo;
        }

        private void FromIToI(int from, int to)
        {
            if (from >= 0 && from < _inventory.Count && to >= 0 && to < _inventory.Count)
            {
                (_inventory[from], _inventory[to]) = (_inventory[to], _inventory[from]);
                (_inventoryLevels[from], _inventoryLevels[to]) = (_inventoryLevels[to], _inventoryLevels[from]);
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: true);
            }
            else
            {
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
            }
        }

        private void FromIToB(int from, int to)
        {
            if (from >= 0 && from < _inventory.Count && to >= 0 && to < _board.Count)
            {
                (_inventory[from], _board[to]) = (_board[to], _inventory[from]);
                (_inventoryLevels[from], _boardLevels[to]) = (_boardLevels[to], _inventoryLevels[from]);
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: true);
            }
            else
            {
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
            }
        }

        private void FromSToI(int from, int to, int heroCost)
        {
            if (from >= 0 && from < _shop.Count && to >= 0 && to < _inventory.Count)
            {
                if (_shop[from] >= 0 && _inventory[to] < 0)
                {
                    _inventory[to] = _shop[from];
                    _shop[from] = -1;
                    _playerLink->Info.Coins -= heroCost;
                    _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: true);
                    return;
                }
            }

            _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
        }

        private void FromSToB(int from, int to, int heroCost)
        {
            if (from >= 0 && from < _shop.Count && to >= 0 && to < _board.Count)
            {
                if (_shop[from] >= 0 && _board[to] < 0)
                {
                    _board[to] = _shop[from];
                    _shop[from] = -1;
                    _playerLink->Info.Coins -= heroCost;
                    _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: true);
                    return;
                }
            }

            _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
        }

        private void FromBToI(int from, int to)
        {
            if (from >= 0 && from < _board.Count && to >= 0 && to < _inventory.Count)
            {
                (_board[from], _inventory[to]) = (_inventory[to], _board[from]);
                (_boardLevels[from], _inventoryLevels[to]) = (_inventoryLevels[to], _boardLevels[from]);
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: true);
            }
            else
            {
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
            }
        }

        private void FromBToB(int from, int to)
        {
            if (from >= 0 && from < _board.Count && to >= 0 && to < _board.Count)
            {
                (_board[from], _board[to]) = (_board[to], _board[from]);
                (_boardLevels[from], _boardLevels[to]) = (_boardLevels[to], _boardLevels[from]);
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: true);
            }
            else
            {
                _frame.Events.MoveHero(_playerLink->Ref, _playerLink->Info.Coins, IsMoved: false);
            }
        }
    }
}
