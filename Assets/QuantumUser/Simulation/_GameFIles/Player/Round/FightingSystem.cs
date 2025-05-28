using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class FightingSystem : SystemSignalsOnly, ISignalStartFight
    {
        public void StartFight(Frame f, int roundIndex)
        {
            RoundInfo roundInfo = f.FindAsset(f.RuntimeConfig.GameConfig).GetRoundInfo(roundIndex);

            if (roundInfo.IsPVE)
            {
                MakePVEBoards(f, Player.GetAllPlayerLinks(f), roundInfo);
                f.Global->IsPVPRound = false;
            }
            else
            {
                if (TryMakePVPBoards(f, Player.GetAllPlayerLinks(f)))
                {
                    f.Global->IsPVPRound = true;
                }
                else
                {
                    MakePVEBoards(f, Player.GetAllPlayerLinks(f), new RoundInfo());
                    f.Global->IsPVPRound = false;
                }
            }

            List<EntityRef> boards = BoardSystem.GetBoardEntities(f);

            for (int i = 0; i < boards.Count; i++)
            {
                SetupBoard(f, BoardSystem.GetBoardPointer(f, boards[i]), i);
            }

            StatsDisplayer.UpdateStats(f);
        }

        private bool TryMakePVPBoards(Frame f, List<PlayerLink> players)
        {
            if (players.Count < 2)
            {
                return false;
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

            return true;
        }

        private void MakePVEBoards(Frame f, List<PlayerLink> players, RoundInfo roundInfo)
        {
            for (int i = 0; i < players.Count; i++)
            {
                f.Signals.MakeNewBoardPVE(players[i], roundInfo);
            }
        }

        private void SetupBoard(Frame f, Board* board, int boardIndex)
        {
            QList<FightingHero> fightingHeroesMap = f.ResolveList(board->FightingHeroesMap);
            QList<HeroEntity> heroesID1 = f.ResolveList(board->Heroes1);
            QList<HeroEntity> heroesID2 = f.ResolveList(board->Heroes2);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            for (int i = 0; i < GameplayConstants.BoardSize * GameplayConstants.BoardSize / 2; i++)
            {
                HeroEntity hero = heroesID1[i];
                int index = i;
                fightingHeroesMap.Add(SetupFightingHero(f, hero, boardIndex, index, GameplayConstants.Team1));
            }

            for (int i = 0; i < GameplayConstants.BoardSize * GameplayConstants.BoardSize / 2; i++)
            {
                HeroEntity hero = heroesID2[^(i + 1)];
                int index = GameplayConstants.BoardSize * GameplayConstants.BoardSize / 2 + i;
                fightingHeroesMap.Add(SetupFightingHero(f, hero, boardIndex, index, GameplayConstants.Team2));
            }

            for (int i = 0; i < fightingHeroesMap.Count; i++)
            {
                fightingHeroesMap[i] = Hero.SetupHero(f, fightingHeroesMap[i], i);
            }

            List<EntityLevelData> heroDataList = fightingHeroesMap.Select(hero => new EntityLevelData { Ref = hero.Hero.Ref, Level = hero.Hero.Level, ID = hero.Hero.ID }).ToList();
            f.Events.StartRound(f, board->Player1.Ref, board->Player2.Ref, heroDataList);
        }

        private FightingHero SetupFightingHero(Frame f, HeroEntity hero, int boardIndex, int index, int teamNumber)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            int extraLives = 0;

            if (hero.ID >= 0)
            {
                HeroNameEnum heroName = gameConfig.GetHeroInfo(f, hero.ID).Name;

                if (heroName == HeroNameEnum.Firebird)
                {
                    extraLives = 1;
                }
            }

            FightingHero fightingHero = new()
            {
                Hero = hero,
                Index = index,
                BoardIndex = boardIndex,
                TeamNumber = teamNumber,
                Effects = f.AllocateList<EffectQnt>(),
                AttackStage = 0,
                AbilityStage = 0,
                AttackTimer = 0,
                AbilityTimer = 0,
                ExtraLives = extraLives,
                IsPassiveAbilityActivated = false,
            };

            return fightingHero;
        }
    }
}
