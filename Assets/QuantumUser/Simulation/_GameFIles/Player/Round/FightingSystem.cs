using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class FightingSystem : SystemSignalsOnly, ISignalStartFight
    {
        public void StartFight(Frame f, int roundIndex)
        {
            RoundInfo roundInfo = f.FindAsset(f.RuntimeConfig.GameConfig).GetRoundInfo(f, roundIndex);

            QList<Board> boards;

            if (roundInfo.IsPVE)
            {
                boards = MakePVEBoards(f, GetCurrentPlayers(f), roundInfo);
            }
            else
            {
                boards = MakePVPBoards(f, GetCurrentPlayers(f));
            }

            foreach (var board in boards)
            {
                SetupBoard(f, board);
            }
        }

        private QList<Board> MakePVPBoards(Frame f, List<PlayerLink> players)
        {
            if (players.Count < 2)
            {
                throw new System.Exception("Not enough players to start the round");
            }

            players = players.OrderBy(_ => f.RNG->Next()).ToList();

            for (int i = 0; i < players.Count; i += 2)
            {
                if (i != players.Count - 1)
                {
                    f.Signals.MakeNewBoardPVP(players[i], players[i + 1], main: true);
                }
                else
                {
                    int random = f.RNG->Next(0, players.Count - 1);

                    if (random >= i)
                    {
                        f.Signals.MakeNewBoardPVP(players[i], players[random + 1], main: false);
                    }
                    else
                    {
                        f.Signals.MakeNewBoardPVP(players[i], players[random], main: false);
                    }
                }
            }

            return GetCurrentBoards(f);
        }

        private QList<Board> MakePVEBoards(Frame f, List<PlayerLink> players, RoundInfo roundInfo)
        {
            for (int i = 0; i < players.Count; i++)
            {
                f.Signals.MakeNewBoardPVE(players[i], roundInfo);
            }

            return GetCurrentBoards(f);
        }

        private void SetupBoard(Frame f, Board board)
        {
            QList<Hero> fightingHeroesMap = f.ResolveList(board.FightingHeroesMap);
            QList<Hero> heroes1 = f.ResolveList(board.Heroes1);
            QList<Hero> heroes2 = f.ResolveList(board.Heroes2);

            for (int i = 0; i < GameConfig.BoardSize * GameConfig.BoardSize / 2; i++)
            {
                fightingHeroesMap.Add(heroes1[i]);
            }

            for (int i = 0; i < GameConfig.BoardSize * GameConfig.BoardSize / 2; i++)
            {
                fightingHeroesMap.Add(heroes2[^(i + 1)]);
            }

            for (int i = 0; i < fightingHeroesMap.Count; i++)
            {
                if (fightingHeroesMap[i].Ref == default)
                {
                    continue;
                }

                fightingHeroesMap[i] = SetupHero(f, fightingHeroesMap, fightingHeroesMap[i]);
            }

            QList<Hero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<EntityLevelData> heroDataList = heroes.Select(hero => new EntityLevelData { Ref = hero.Ref, Level = hero.Level }).ToList();
            f.Events.StartRound(f, board.Player1.Ref, board.Player2.Ref, board.Ref, heroDataList);
        }

        private Hero SetupHero(Frame f, QList<Hero> fightingHeroesMap, Hero hero)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (BoardPosition.TryGetHeroCords(f, fightingHeroesMap, hero, out Vector2Int position))
            {
                hero.TargetPositionX = position.x;
                hero.TargetPositionY = position.y;
            }
            else
            {
                throw new System.NotSupportedException();
            }

            HeroInfo heroInfo = config.GetHeroInfo(f, hero.ID);
            hero.Health = heroInfo.Health;
            hero.CurrentHealth = hero.Health;
            hero.Defense = heroInfo.Defense;
            hero.Damage = heroInfo.Damage;
            hero.AttackSpeed = FP.FromFloat_UNSAFE(heroInfo.AttackSpeed);
            hero.ProjectileSpeed = FP.FromFloat_UNSAFE(heroInfo.ProjectileSpeed);
            hero.Range = heroInfo.Range;
            hero.RangePercentage = FP.FromFloat_UNSAFE(heroInfo.RangePercentage);
            hero.IsAlive = true;
            hero.AttackTimer = 0;

            return hero;
        }

        private QList<Board> GetCurrentBoards(Frame f)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);

            return boards;
        }

        private List<PlayerLink> GetCurrentPlayers(Frame f)
        {
            List<PlayerLink> players = new();

            foreach (var (_, player) in f.GetComponentIterator<PlayerLink>())
            {
                players.Add(player);
            }

            return players;
        }
    }
}
