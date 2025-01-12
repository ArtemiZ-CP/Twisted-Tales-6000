using System.Collections.Generic;
using Quantum.Collections;
using UnityEngine.Scripting;
using System.Linq;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerInitializationSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        public override void OnInit(Frame f)
        {
            f.Global->RngSession = new Photon.Deterministic.RNGSession(1);
            f.Global->IsGameStarted = false;
        }

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if (firstTime == false)
            {
                ReinitializePlayer(f, player);

                return;
            }

            if (f.Global->IsGameStarted)
            {
                return;
            }

            InitializeNewPlayer(f, player);

            if (Player.GetAllPlayers(f).Count == f.SessionConfig.PlayerCount)
            {
                f.Global->IsGameStarted = true;
            }
        }

        public void OnPlayerRemoved(Frame f, PlayerRef player)
        {

        }

        private void InitializeNewPlayer(Frame f, PlayerRef player)
        {
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
                    Coins = gameConfig.CoinsPerRound[0],
                    Health = gameConfig.PlayerHealth
                }
            };

            FillList(f, playerLink.Info.Shop.HeroesID, gameConfig.ShopSize, -1);
            FillList(f, playerLink.Info.Inventory.HeroesID, gameConfig.InventorySize, -1);
            FillList(f, playerLink.Info.Inventory.HeroesLevel, gameConfig.InventorySize, 0);
            FillList(f, playerLink.Info.Board.HeroesID, GameConfig.BoardSize * GameConfig.BoardSize / 2, -1);
            FillList(f, playerLink.Info.Board.HeroesLevel, GameConfig.BoardSize * GameConfig.BoardSize / 2, 0);

            f.Add(playerEntity, playerLink);

            f.Signals.OnReloadShop(Player.GetPlayerPointer(f, playerEntity), cost: 0);

            ReinitializePlayer(f, player);
        }

        private void ReinitializePlayer(Frame f, PlayerRef player)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            PlayerLink* playerLink = Player.GetPlayerPointer(f, player);
            int shopLevel = playerLink->Info.Shop.Level;
            int shopUpgradeCost = gameConfig.ShopUpdrageSettings[shopLevel].Cost;

            f.Events.GetPlayerInfo(f, player, playerLink->Info);
            f.Events.GetCurrentPlayers(f, Player.GetAllPlayersLink(f), BoardSystem.GetBoards(f));
            f.Events.ChangeCoins(player, playerLink->Info.Coins);
            f.Events.ReloadShop(f, player, f.ResolveList(playerLink->Info.Shop.HeroesID).ToList());
            Shop.SendShopUpgradeInfo(f, playerLink);

            if (f.Global->IsBuyPhase == false)
            {
                Board board = BoardSystem.GetBoard(f, playerLink->Ref);
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

                List<EntityLevelData> heroDataList = heroes.Select(hero => new EntityLevelData { Ref = hero.Hero.Ref, Level = hero.Hero.Level, ID = hero.Hero.ID }).ToList();
                f.Events.StartRound(f, board.Player1.Ref, board.Player2.Ref, heroDataList);

                List<EntityLevelData> projectilesData = f.ResolveList(board.HeroProjectiles).Select(p => new EntityLevelData { Ref = p.Ref, Level = p.Level }).ToList();
                f.Events.GetProjectiles(f, board.Player1.Ref, board.Player2.Ref, projectilesData);
            }
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