namespace Quantum.Game
{
    public static unsafe class Events
    {
        public static void DisplayRoundNumber(Frame f)
        {
            f.Events.DisplayRoundNumber(f.Global->PhaseNumber);
        }

        public static void SetShopRollCost(Frame f, PlayerLink playerLink)
        {
            f.Events.SetRollCost(playerLink.Ref, playerLink.Info.Shop.RollCost);
        }

        public static void SetFreezeShop(Frame f, PlayerLink playerLink)
        {
            f.Events.FreezeShop(playerLink.Ref, playerLink.Info.Shop.IsLocked);
        }

        public static void GetShopHeroes(Frame f, PlayerLink playerLink)
        {
            f.Events.GetShopHeroes(f, playerLink.Ref, f.ResolveList(playerLink.Info.Shop.HeroesID));
        }

        public static void GetBoardHeroes(Frame f)
        {
            foreach (PlayerLink player in Player.GetAllPlayerLinks(f))
            {
                GetBoardHeroes(f, player.Ref);
            }
        }

        public static void GetBoardHeroes(Frame f, PlayerRef playerRef)
        {
            PlayerLink playerLink = Player.GetPlayer(f, playerRef);
            GetBoardHeroes(f, playerLink);
        }

        public static void GetBoardHeroes(Frame f, PlayerLink playerLink)
        {
            f.Events.GetBoardHeroes(f, playerLink.Ref,
                f.ResolveList(playerLink.Info.Board.HeroesID),
                f.ResolveList(playerLink.Info.Board.HeroesLevel));
        }

        public static void GetInventoryHeroes(Frame f)
        {
            foreach (PlayerLink player in Player.GetAllPlayerLinks(f))
            {
                GetInventoryHeroes(f, player.Ref);
            }
        }

        public static void GetInventoryHeroes(Frame f, PlayerRef playerRef)
        {
            PlayerLink playerLink = Player.GetPlayer(f, playerRef);
            GetInventoryHeroes(f, playerLink);
        }

        public static void GetInventoryHeroes(Frame f, PlayerLink playerLink)
        {
            f.Events.GetInventoryHeroes(f, playerLink.Ref,
                f.ResolveList(playerLink.Info.Inventory.HeroesID),
                f.ResolveList(playerLink.Info.Inventory.HeroesLevel));
        }

        public static void ChangeCoins(Frame f, PlayerLink playerLink)
        {
            f.Events.ChangeCoins(playerLink.Ref, playerLink.Info.Coins);
        }

        public static void GetCurrentPlayers(Frame f)
        {
            f.Events.GetCurrentPlayers(f, Player.GetAllPlayerLinks(f), BoardSystem.GetBoards(f));
        }

        public static void DisactiveEntity(Frame f, Board board, EntityRef entityRef)
        {
            f.Events.SetActiveEntity(f, board.Player1.Ref, entityRef, false, new EntityLevelData());
            f.Events.SetActiveEntity(f, board.Player2.Ref, entityRef, false, new EntityLevelData());
        }

        public static void ActiveEntity(Frame f, Board board, EntityRef entityRef, EntityLevelData entityLevelData)
        {
            f.Events.SetActiveEntity(f, board.Player1.Ref, entityRef, true, entityLevelData);
            f.Events.SetActiveEntity(f, board.Player2.Ref, entityRef, true, entityLevelData);
        }
    }
}