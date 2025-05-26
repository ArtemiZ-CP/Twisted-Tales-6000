using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroMovingSystem : SystemSignalsOnly, ISignalOnMoveHero, ISignalSellHero
    {
        QList<HeroIdLevel> _inventory;
        QList<HeroIdLevel> _board;
        QList<int> _shop;
        PlayerLink* _playerLink;
        Frame _f;

        public void SellHero(Frame f, PlayerLink* playerLink, HeroState heroState, int positionX, int positionY)
        {
            _inventory = f.ResolveList(playerLink->Info.Inventory.Heroes);
            _board = f.ResolveList(playerLink->Info.Board.Heroes);
            _shop = f.ResolveList(playerLink->Info.Shop.HeroesID);
            _playerLink = playerLink;
            _f = f;
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (heroState == HeroState.Shop)
            {
                return;
            }
            else if (heroState == HeroState.Inventory)
            {
                int heroIndex = positionX;

                if (heroIndex >= 0 && heroIndex < _inventory.Count)
                {
                    HeroIdLevel heroIdLevel = _inventory[heroIndex];

                    if (heroIdLevel.ID >= 0)
                    {
                        Player.AddCoins(f, playerLink, gameConfig.GetHeroSellCost(f, heroIdLevel.ID, heroIdLevel.Level));
                        heroIdLevel.ID = -1;
                        heroIdLevel.Level = 0;
                        _inventory[heroIndex] = heroIdLevel;
                    }
                }
            }
            else if (heroState == HeroState.Board)
            {
                int heroIndex = positionY * GameConfig.BoardSize + positionX;

                if (heroIndex >= 0 && heroIndex < _board.Count)
                {
                    HeroIdLevel heroIdLevel = _board[heroIndex];

                    if (heroIdLevel.ID >= 0)
                    {
                        Player.AddCoins(f, playerLink, gameConfig.GetHeroSellCost(f, heroIdLevel.ID, heroIdLevel.Level));
                        heroIdLevel.ID = -1;
                        heroIdLevel.Level = 0;
                        _board[heroIndex] = heroIdLevel;
                    }
                }
            }

            ShowHeroesOnBoardCount(f, *playerLink);
        }

        public void OnMoveHero(Frame f, PlayerLink* playerLink, HeroState HeroFromState, HeroState HeroToState, int positionFromX, int positionFromY, int positionToX, int positionToY)
        {
            _inventory = f.ResolveList(playerLink->Info.Inventory.Heroes);
            _board = f.ResolveList(playerLink->Info.Board.Heroes);
            _shop = f.ResolveList(playerLink->Info.Shop.HeroesID);
            _playerLink = playerLink;
            _f = f;

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
                HeroInfo heroInfo;

                if (positionFromX < 0 || positionFromX >= _shop.Count)
                {
                    heroInfo = null;
                }
                else
                {
                    heroInfo = GetBuyingHero(f, _shop[positionFromX]);
                }

                if (heroInfo == null)
                {
                    _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
                    return;
                }

                if (Player.TryRemoveCoins(f, playerLink, heroInfo.GetBuyCost(f)) == false)
                {
                    _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
                    return;
                }

                if (HeroToState == HeroState.Inventory)
                {
                    if (FromSToI(positionFromX, positionToX) == false)
                    {
                        Player.AddCoins(f, playerLink, heroInfo.GetBuyCost(f));
                    }
                }
                else if (HeroToState == HeroState.Board)
                {
                    if (FromSToB(positionFromX, boardToIndex) == false)
                    {
                        Player.AddCoins(f, playerLink, heroInfo.GetBuyCost(f));
                    }
                }

                f.Signals.TryUpgradeHero(playerLink);
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

            ShowHeroesOnBoardCount(f, *playerLink);
        }

        public static void ShowHeroesOnBoardCount(Frame f, PlayerLink playerLink)
        {
            int heroesCountOnBoard = GetHeroesCountOnBoard(f.ResolveList(playerLink.Info.Board.Heroes));
            int maxHeroesCountOnBoard = GetMaxHeroesCountOnBoard(f, playerLink);
            f.Events.ShowHeroesOnBoardCount(playerLink.Ref, heroesCountOnBoard, maxHeroesCountOnBoard);
        }

        private static int GetHeroesCountOnBoard(QList<HeroIdLevel> board)
        {
            int count = 0;

            for (int i = 0; i < board.Count; i++)
            {
                if (board[i].ID >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetMaxHeroesCountOnBoard(Frame f, PlayerLink playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            return gameConfig.ShopUpdrageSettings[playerLink.Info.Shop.Level].MaxCharactersOnBoard;
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
            if (from >= 0 && from < _inventory.Count && to >= 0 && to < _inventory.Count && _inventory[from].ID >= 0)
            {
                (_inventory[from], _inventory[to]) = (_inventory[to], _inventory[from]);
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: true);
            }
            else
            {
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
            }
        }

        private void FromIToB(int from, int to)
        {
            if (from >= 0 && from < _inventory.Count && to >= 0 && to < _board.Count && _inventory[from].ID >= 0)
            {
                if (_board[to].ID < 0 && IsAbleToMoveOnBoard() == false)
                {
                    _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
                }
                else
                {
                    (_inventory[from], _board[to]) = (_board[to], _inventory[from]);
                    _f.Events.MoveHero(_playerLink->Ref, IsMoved: true);
                }
            }
            else
            {
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
            }
        }

        private bool FromSToI(int from, int to)
        {
            if (from >= 0 && from < _shop.Count && to >= 0 && to < _inventory.Count && _shop[from] >= 0)
            {
                if (_shop[from] >= 0 && _inventory[to].ID < 0)
                {
                    HeroIdLevel heroIdLevel = _inventory[to];
                    heroIdLevel.ID = _shop[from];
                    _inventory[to] = heroIdLevel;
                    _shop[from] = -1;
                    _f.Events.MoveHero(_playerLink->Ref, IsMoved: true);
                    return true;
                }
            }

            _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
            return false;
        }

        private bool FromSToB(int from, int to)
        {
            if (from >= 0 && from < _shop.Count && to >= 0 && to < _board.Count && _shop[from] >= 0 && IsAbleToMoveOnBoard())
            {
                if (_shop[from] >= 0 && _board[to].ID < 0)
                {
                    HeroIdLevel heroIdLevel = _board[to];
                    heroIdLevel.ID = _shop[from];
                    _board[to] = heroIdLevel;
                    _shop[from] = -1;
                    _f.Events.MoveHero(_playerLink->Ref, IsMoved: true);
                    return true;
                }
            }

            _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
            return false;
        }

        private void FromBToI(int from, int to)
        {
            if (from >= 0 && from < _board.Count && to >= 0 && to < _inventory.Count && _board[from].ID >= 0)
            {
                (_board[from], _inventory[to]) = (_inventory[to], _board[from]);
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: true);
            }
            else
            {
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
            }
        }

        private void FromBToB(int from, int to)
        {
            if (from >= 0 && from < _board.Count && to >= 0 && to < _board.Count && _board[from].ID >= 0)
            {
                (_board[from], _board[to]) = (_board[to], _board[from]);
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: true);
            }
            else
            {
                _f.Events.MoveHero(_playerLink->Ref, IsMoved: false);
            }
        }

        private bool IsAbleToMoveOnBoard() => GetHeroesCountOnBoard(_board) < GetMaxHeroesCountOnBoard(_f, *_playerLink);
    }
}
