using System.Collections.Generic;
using Quantum.Collections;
using UnityEngine.Scripting;
using System.Linq;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class PlayerInitializationSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public override void OnInit(Frame f)
        {
            f.Global->IsGameStarted = false;
        }

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if (firstTime == false)
            {
                ReinitializeEntity(f, player);

                return;
            }

            if (f.Global->IsGameStarted)
            {
                return;
            }

            InitializeNewPlayer(f, player);

            if (Player.GetAllPlayers(f).Count == f.SessionConfig.PlayerCount)
            {
                int botsCount = f.FindAsset(f.RuntimeConfig.GameConfig).MaxPlayers - f.SessionConfig.PlayerCount;
                InitializeBots(f, botsCount);
                f.Global->Boards = f.AllocateList<Board>();
                f.Global->IsGameStarted = true;
                Events.GetCurrentPlayers(f);
                f.Signals.BotStartRound();
            }

            ReinitializeEntity(f, player);
        }

        private void InitializeNewPlayer(Frame f, PlayerRef player)
        {
            EntityRef playerEntity = SpawnPlayer(f, player, out string playerName);
            InitializeEntity(f, player, playerEntity, playerName, bot: false);
        }

        private EntityRef SpawnPlayer(Frame f, PlayerRef player, out string playerName)
        {
            RuntimePlayer data = f.GetPlayerData(player);
            playerName = data.PlayerNickname;

            return f.Create(data.PlayerAvatar);
        }

        private void InitializeBots(Frame f, int botsCount)
        {
            for (int i = 0; i < botsCount; i++)
            {
                InitializeBot(f, i);
            }
        }

        private void InitializeBot(Frame f, int index)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            EntityRef botEntity = f.Create(gameConfig.BotPrototype);
            PlayerRef playerRef = new()
            {
                _index = index + 1000
            };

            InitializeEntity(f, playerRef, botEntity, $"Bot{index}", bot: true);
        }

        private void InitializeEntity(Frame f, PlayerRef playerRef, EntityRef entityRef, string name, bool bot)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            PlayerLink playerLink = new()
            {
                Ref = playerRef,
                Info = new()
                {
                    Nickname = name,
                    Shop = new()
                    {
                        HeroesID = f.AllocateList<int>(gameConfig.ShopSize),
                        IsLocked = false,
                        RollCost = 0
                    },
                    Inventory = new()
                    {
                        Heroes = f.AllocateList<HeroIdLevel>(gameConfig.InventorySize),
                    },
                    Board = new()
                    {
                        Heroes = f.AllocateList<HeroIdLevel>(GameConfig.BoardSize * GameConfig.BoardSize / 2),
                        Abilities = f.AllocateList<SelectedHeroAbility>()
                    },
                    Coins = gameConfig.CoinsPerRound[0],
                    Health = gameConfig.PlayerHealth,
                    Bot = bot
                }
            };

            FillList(f, playerLink.Info.Shop.HeroesID, gameConfig.ShopSize, -1);
            FillList(f, playerLink.Info.Inventory.Heroes, gameConfig.InventorySize, new HeroIdLevel { ID = -1, Level = 0 });
            FillList(f, playerLink.Info.Board.Heroes, GameConfig.BoardSize * GameConfig.BoardSize / 2, new HeroIdLevel { ID = -1, Level = 0 });

            f.Add(entityRef, playerLink);

            f.Signals.OnReloadShop(Player.GetPlayerPointer(f, entityRef));
        }

        private void ReinitializeEntity(Frame f, PlayerRef player)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            PlayerLink playerLink = Player.GetPlayerLink(f, player);

            Events.GetCurrentPlayers(f);
            Events.ChangeCoins(f, playerLink);
            Events.GetShopHeroes(f, playerLink);
            Events.GetBoardHeroes(f, playerLink);
            Events.GetInventoryHeroes(f, playerLink);
            HeroMovingSystem.ShowHeroesOnBoardCount(f, playerLink);
            Shop.SendShopUpgradeInfo(f, playerLink);
            Events.SetFreezeShop(f, playerLink);
            Events.SetShopRollCost(f, playerLink);
            Events.DisplayRoundNumber(f);

            if (f.Global->IsBuyPhase == false)
            {
                Board board = BoardSystem.GetBoard(f, playerLink.Ref);
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

                IEnumerable<EntityLevelData> heroDataList = heroes.Select(hero => new EntityLevelData { Ref = hero.Hero.Ref, Level = hero.Hero.Level, ID = hero.Hero.ID });
                f.Events.StartRound(f, board.Player1.Ref, board.Player2.Ref, heroDataList);

                QList<HeroProjectile> heroProjectiles = f.ResolveList(board.HeroProjectiles);

                foreach (HeroProjectile projectile in heroProjectiles)
                {
                    Events.ActiveEntity(f, board, projectile.Ref, new EntityLevelData() { Ref = projectile.Ref, Level = projectile.Level });
                }
            }
        }

        private void FillList<T>(Frame frame, QListPtr<T> list, int size, T value) where T : unmanaged
        {
            QList<T> resolvedList = frame.ResolveList(list);

            for (int i = 0; i < size; i++)
            {
                resolvedList.Add(value);
            }
        }
    }
}