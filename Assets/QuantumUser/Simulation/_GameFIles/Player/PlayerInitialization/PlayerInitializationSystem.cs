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
                f.Events.InitPlayer(player);

                if (f.Global->IsBuyPhase == false)
                {
                    QList<Board> boards = f.ResolveList(f.Global->Boards);

                    foreach (Board board in boards)
                    {
                        QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
                        List<EntityLevelData> heroDataList = heroes.Select(hero => new EntityLevelData { Ref = hero.Hero.Ref, Level = hero.Hero.Level }).ToList();

                        if (board.Player1.Ref == player)
                        {
                            f.Events.StartRound(f, player, default, board.Ref, heroDataList);
                        }
                        else if (board.Player2.Ref == player)
                        {
                            f.Events.StartRound(f, default, player, board.Ref, heroDataList);
                        }
                    }
                }
                
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