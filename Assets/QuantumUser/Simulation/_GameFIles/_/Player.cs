using System.Collections.Generic;

namespace Quantum.Game
{
    public unsafe class Player
    {
        public static PlayerLink GetPlayerLink(FightingHero fightingHero, Board board)
        {
            return fightingHero.TeamNumber == GameplayConstants.Team1 ? board.Player1 : board.Player2;
        }

        public static EntityRef GetPlayerEntity(Frame f, PlayerRef playerRef)
        {
            foreach ((EntityRef entity, PlayerLink link) in f.GetComponentIterator<PlayerLink>())
            {
                if (link.Ref == playerRef)
                {
                    return entity;
                }
            }

            return default;
        }

        public static PlayerLink* GetPlayerPointer(Frame f, EntityRef entity)
        {
            return f.Unsafe.GetPointer<PlayerLink>(entity);
        }

        public static PlayerLink GetPlayerLink(Frame f, EntityRef entity)
        {
            return f.Get<PlayerLink>(entity);
        }

        public static PlayerLink* GetPlayerPointer(Frame f, PlayerRef playerRef)
        {
            foreach ((EntityRef entity, PlayerLink link) in f.GetComponentIterator<PlayerLink>())
            {
                if (link.Ref == playerRef)
                {
                    return GetPlayerPointer(f, entity);
                }
            }

            return default;
        }

        public static PlayerLink GetPlayerLink(Frame f, PlayerRef playerRef)
        {
            foreach ((EntityRef entity, PlayerLink link) in f.GetComponentIterator<PlayerLink>())
            {
                if (link.Ref == playerRef)
                {
                    return link;
                }
            }

            return default;
        }

        public static List<PlayerLink> GetAllPlayerLinks(Frame f)
        {
            List<PlayerLink> players = new();

            foreach ((EntityRef _, PlayerLink playerLink) in f.GetComponentIterator<PlayerLink>())
            {
                players.Add(playerLink);
            }

            return players;
        }

        public static List<EntityRef> GetAllPlayerEntities(Frame f)
        {
            List<EntityRef> playersEntity = new();

            foreach ((EntityRef entity, PlayerLink _) in f.GetComponentIterator<PlayerLink>())
            {
                playersEntity.Add(entity);
            }

            return playersEntity;
        }

        public static List<(EntityRef entity, PlayerLink link)> GetAllPlayers(Frame f)
        {
            List<(EntityRef entity, PlayerLink link)> playersEntity = new();

            foreach ((EntityRef entity, PlayerLink link) in f.GetComponentIterator<PlayerLink>())
            {
                playersEntity.Add((entity, link));
            }

            return playersEntity;
        }

        public static void ResetCoins(Frame f)
        {
            var players = GetAllPlayerEntities(f);

            foreach (var entity in players)
            {
                ResetCoins(f, GetPlayerPointer(f, entity));
            }
        }

        public static void ResetCoins(Frame f, PlayerLink* playerLink)
        {
            playerLink->Info.Coins = 0;
            f.Events.ChangeCoins(playerLink->Ref, playerLink->Info.Coins);
        }

        public static void AddCoins(Frame f, int coins)
        {
            if (coins < 0)
            {
                return;
            }

            var players = GetAllPlayerEntities(f);

            foreach (var entity in players)
            {
                AddCoins(f, GetPlayerPointer(f, entity), coins);
            }
        }

        public static void AddCoins(Frame f, PlayerLink* playerLink, int coins)
        {
            if (coins < 0)
            {
                return;
            }

            playerLink->Info.Coins += coins;
            f.Events.ChangeCoins(playerLink->Ref, playerLink->Info.Coins);
        }

        public static bool TryRemoveCoins(Frame f, PlayerLink* playerLink, int coins)
        {
            if (coins < 0)
            {
                return false;
            }

            if (playerLink->Info.Coins < coins)
            {
                return false;
            }

            playerLink->Info.Coins -= coins;

            f.Events.ChangeCoins(playerLink->Ref, playerLink->Info.Coins);

            return true;
        }
    }
}
