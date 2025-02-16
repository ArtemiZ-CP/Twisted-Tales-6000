using System.Collections.Generic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class Bot
    {
        public static List<EntityRef> GetAllPlayerLinks(Frame f)
        {
            List<EntityRef> players = new();

            foreach ((EntityRef entityRef, PlayerLink playerLink) in f.GetComponentIterator<PlayerLink>())
            {
                if (playerLink.Info.Bot)
                {
                    players.Add(entityRef);
                }
            }

            return players;
        }

        public static PlayerLink* GetPlayerPointer(Frame f, EntityRef entity)
        {
            return f.Unsafe.GetPointer<PlayerLink>(entity);
        }

        public static void ProcessStartRound(Frame f, PlayerLink* playerLink)
        {
            if (playerLink->Info.Bot == false)
            {
                return;
            }

            TryBuyHeroes(f, playerLink);
            TryPlaceHeroes(f, playerLink);
            TryUpgradeShop(f, playerLink);
        }

        private static void TryBuyHeroes(Frame f, PlayerLink* playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            QList<int> shop = f.ResolveList(playerLink->Info.Shop.HeroesID);
            ShopUpdrageSettings currentUpgrade = gameConfig.ShopUpdrageSettings[playerLink->Info.Shop.Level];

            for (int i = 0; i < shop.Count; i++)
            {
                if (playerLink->Info.Coins == 0)
                {
                    break;
                }

                if (ContainSameHero(f, playerLink, shop[i]) == false &&
                    GetHeroesCount(f, playerLink) >= currentUpgrade.MaxCharactersOnBoard)
                {
                    continue;
                }

                f.Signals.OnBuyHero(playerLink, i);
            }
        }

        private static void TryPlaceHeroes(Frame f, PlayerLink* playerLink)
        {
            QList<int> inventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] < 0)
                {
                    continue;
                }

                (int x, int y) = GetEmptyBoardPositions(f, playerLink);

                f.Signals.OnMoveHero(playerLink, HeroState.Inventory, HeroState.Board, i, 0, x, y);
            }
        }

        private static void TryUpgradeShop(Frame f, PlayerLink* playerLink)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            for (int i = 0; i < 10; i++)
            {
                if (playerLink->Info.Coins < gameConfig.ShopUpgrageCost)
                {
                    break;
                }

                f.Signals.OnUpgradeShop(playerLink);
            }
        }

        private static (int, int) GetEmptyBoardPositions(Frame f, PlayerLink* playerLink)
        {
            QList<int> board = f.ResolveList(playerLink->Info.Board.HeroesID);
            List<int> emptyPositions = new List<int>();

            for (int index = 0; index < board.Count / 2; index++)
            {
                if (board[index] < 0)
                {
                    emptyPositions.Add(index);
                }
            }

            if (emptyPositions.Count == 0)
            {
                return (-1, -1);
            }

            int randomIndex = f.RNG->Next(0, emptyPositions.Count);
            int selectedPosition = emptyPositions[randomIndex];
            
            GameConfig.ArrayIndexToCords(selectedPosition, out int x, out int y);
            return (x, y);
        }

        private static int GetHeroesCount(Frame f, PlayerLink* playerLink)
        {
            QList<int> board = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> inventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            int count = 0;

            for (int i = 0; i < board.Count; i++)
            {
                if (board[i] >= 0)
                {
                    count++;
                }
            }

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ContainSameHero(Frame f, PlayerLink* playerLink, int heroID)
        {
            QList<int> board = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> inventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);

            for (int i = 0; i < board.Count; i++)
            {
                if (board[i] == heroID)
                {
                    return true;
                }
            }

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == heroID)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
