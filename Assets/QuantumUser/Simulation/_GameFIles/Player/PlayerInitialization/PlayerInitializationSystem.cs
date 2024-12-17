using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerInitializationSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if (firstTime == false)
            {
                // reconnecting player

                return;
            }

            RuntimePlayer data = f.GetPlayerData(player);
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            EntityRef playerEntity = f.Create(entityPrototypeAsset);

            PlayerLink playerLink = new()
            {
                Ref = player,
                Info = new()
                {
                    Shop = new()
                    {
                        HeroesID = f.AllocateList<int>(gameConfig.ShopSize)
                    },
                    Inventory = new()
                    {
                        HeroesID = f.AllocateList<int>(gameConfig.InventorySize),
                        HeroesLevel = f.AllocateList<int>(gameConfig.InventorySize)
                    },
                    Board = new()
                    {
                        HeroesID = f.AllocateList<int>(GameConfig.BoardSize * GameConfig.BoardSize / 2),
                        HeroesLevel = f.AllocateList<int>(GameConfig.BoardSize * GameConfig.BoardSize / 2)
                    },
                    Coins = 1 + gameConfig.CoinsPerRound,
                    Health = gameConfig.PlayerHealth
                }
            };

            FillList(f, playerLink.Info.Shop.HeroesID, gameConfig.ShopSize, -1);
            FillList(f, playerLink.Info.Inventory.HeroesID, gameConfig.InventorySize, -1);
            FillList(f, playerLink.Info.Board.HeroesID, GameConfig.BoardSize * GameConfig.BoardSize / 2, -1);
            FillList(f, playerLink.Info.Inventory.HeroesLevel, gameConfig.InventorySize, 0);
            FillList(f, playerLink.Info.Board.HeroesLevel, GameConfig.BoardSize * GameConfig.BoardSize / 2, 0);

            f.Signals.OnReloadShop(&playerLink);
            f.Add(playerEntity, playerLink);
            f.Events.InitPlayer(player);
            f.Events.GetShopUpgradeCost(player, gameConfig.ShopUpdrageSettings[0].Cost);
        }

        public void OnPlayerRemoved(Frame f, PlayerRef player)
        {

        }

        private void FillList(Frame frame, QListPtr<int> list, int size, int value)
        {
            QList<int> resolvedList = frame.ResolveList(list);

            for (int i = 0; i < size; i++)
            {
                resolvedList.Add(value);
            }
        }
    }
}