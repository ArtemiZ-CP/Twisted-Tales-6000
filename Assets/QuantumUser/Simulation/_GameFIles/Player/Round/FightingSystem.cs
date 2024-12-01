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

            for (int i = 0; i < boards.Count; i++)
            {
                SetupBoard(f, boards[i], i);
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

            return f.ResolveList(f.Global->Boards);
        }

        private QList<Board> MakePVEBoards(Frame f, List<PlayerLink> players, RoundInfo roundInfo)
        {
            for (int i = 0; i < players.Count; i++)
            {
                f.Signals.MakeNewBoardPVE(players[i], roundInfo);
            }

            return f.ResolveList(f.Global->Boards);
        }

        private void SetupBoard(Frame f, Board board, int boardIndex)
        {
            QList<FightingHero> fightingHeroesMap = f.ResolveList(board.FightingHeroesMap);
            QList<Hero> heroesID1 = f.ResolveList(board.HeroesID1);
            QList<Hero> heroesID2 = f.ResolveList(board.HeroesID2);

            for (int i = 0; i < GameConfig.BoardSize * GameConfig.BoardSize / 2; i++)
            {
                FightingHero hero = new()
                {
                    Hero = heroesID1[i],
                    Index = i,
                    BoardIndex = boardIndex
                };

                fightingHeroesMap.Add(hero);
            }

            for (int i = 0; i < GameConfig.BoardSize * GameConfig.BoardSize / 2; i++)
            {
                FightingHero hero = new()
                {
                    Hero = heroesID2[^(i + 1)],
                    Index = GameConfig.BoardSize * GameConfig.BoardSize / 2 + i,
                    BoardIndex = boardIndex
                };

                fightingHeroesMap.Add(hero);
            }

            for (int i = 0; i < fightingHeroesMap.Count; i++)
            {
                if (fightingHeroesMap[i].Hero.Ref == default)
                {
                    continue;
                }

                fightingHeroesMap[i] = SetupHero(f, fightingHeroesMap[i], i);
            }

            List<EntityLevelData> heroDataList = fightingHeroesMap.Select(hero => new EntityLevelData { Ref = hero.Hero.Ref, Level = hero.Hero.Level }).ToList();
            f.Events.StartRound(f, board.Player1.Ref, board.Player2.Ref, board.Ref, heroDataList);
        }

        private FightingHero SetupHero(Frame f, FightingHero hero, int heroIndex)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (HeroBoard.TryGetHeroCords(heroIndex, out Vector2Int position))
            {
                hero.Hero.TargetPositionX = position.x;
                hero.Hero.TargetPositionY = position.y;
            }
            else
            {
                throw new System.NotSupportedException();
            }

            HeroInfo heroInfo = config.GetHeroInfo(f, hero.Hero.ID);
            HeroLevelStats heroLevelStats = heroInfo.HeroStats[hero.Hero.Level];
            hero.Hero.Health = FP.FromFloat_UNSAFE(heroLevelStats.Health);
            hero.Hero.CurrentHealth = FP.FromFloat_UNSAFE(heroLevelStats.Health);
            hero.Hero.Defense = FP.FromFloat_UNSAFE(heroLevelStats.Defense);
            hero.Hero.Damage = FP.FromFloat_UNSAFE(heroLevelStats.Damage);
            hero.Hero.AttackSpeed = FP.FromFloat_UNSAFE(heroLevelStats.AttackSpeed);
            hero.Hero.ProjectileSpeed = FP.FromFloat_UNSAFE(heroLevelStats.ProjectileSpeed);
            hero.Hero.Range = heroLevelStats.Range;
            hero.Hero.RangePercentage = FP.FromFloat_UNSAFE(config.RangePercentage);
            hero.Hero.IsAlive = true;
            hero.Hero.AttackTimer = 0;

            return hero;
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
