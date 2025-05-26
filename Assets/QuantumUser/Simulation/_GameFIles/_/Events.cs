using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class Events
    {
        public static void ChangeHeroStats(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            FP armor = fightingHero.CurrentArmor;

            QList<EffectQnt> effectQnts = f.ResolveList(fightingHero.Effects);

            for (int i = 0; i < effectQnts.Count; i++)
            {
                EffectQnt effectQnt = effectQnts[i];

                if (effectQnt.Index == (int)HeroEffects.EffectType.TemporaryArmor)
                {
                    armor += effectQnt.Value;
                }
            }

            f.Events.HeroHealthChanged(board.Player1.Ref, board.Player2.Ref, fightingHero.Hero.Ref, fightingHero.CurrentHealth, fightingHero.Hero.Health, armor);
            f.Events.HeroManaChanged(board.Player1.Ref, board.Player2.Ref, fightingHero.Hero.Ref, fightingHero.CurrentMana, fightingHero.Hero.MaxMana);
        }

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
            PlayerLink playerLink = Player.GetPlayerLink(f, playerRef);
            GetBoardHeroes(f, playerLink);
        }

        public static void GetBoardHeroes(Frame f, PlayerLink playerLink)
        {
            f.Events.GetBoardHeroes(f, playerLink.Ref, f.ResolveList(playerLink.Info.Board.Heroes));
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
            PlayerLink playerLink = Player.GetPlayerLink(f, playerRef);
            GetInventoryHeroes(f, playerLink);
        }

        public static void GetInventoryHeroes(Frame f, PlayerLink playerLink)
        {
            f.Events.GetInventoryHeroes(f, playerLink.Ref, f.ResolveList(playerLink.Info.Inventory.Heroes));
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