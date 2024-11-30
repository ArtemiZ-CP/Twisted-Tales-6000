using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class RoundSystem : SystemMainThread, ISignalOnStartRound, ISignalOnEndRound, ISignalGetPlayersList
    {
        public override void OnInit(Frame f)
        {
            f.Global->IsBuyPhase = true;
            f.Global->IsDelayPassed = false;
            f.Global->PhaseTime = 0;
            f.Global->PhaseNumber = 0;
        }

        public override void Update(Frame f)
        {
            ProcessRound(f);
        }

        public void GetPlayersList(Frame f)
        {
            List<PlayerLink> players = new();

            foreach ((EntityRef _, PlayerLink playerLink) in f.GetComponentIterator<PlayerLink>())
            {
                players.Add(playerLink);
            }

            f.Events.GetCurrentPlayers(f, players, f.ResolveList(f.Global->Boards).ToList());
        }

        public void OnStartRound(Frame f)
        {
            if (IsRoundStarted(f)) return;

            StartRound(f);
        }

        public void OnEndRound(Frame f)
        {
            if (IsRoundStarted(f) == false) return;

            EndRound(f);
        }

        private void StartRound(Frame f)
        {
            f.Global->IsBuyPhase = false;
            f.Global->IsFighting = true;
            f.Global->IsDelayPassed = false;
            f.Global->PhaseTime = 0;
            f.Signals.StartFight(f.Global->PhaseNumber);
            GetPlayersList(f);
        }

        private void EndRound(Frame f)
        {
            f.Global->IsBuyPhase = true;
            f.Global->PhaseNumber++;
            f.Global->PVPStreak = 0;
            f.Global->PhaseTime = 0;
            f.Signals.ClearBoards();
            f.Events.EndRound();
            GetPlayersList(f);
        }

        private void ProcessRound(Frame f)
        {
            if (f.Global->IsBuyPhase)
            {
                ProcessBuyPhase(f);
            }
            else
            {
                if (f.Global->IsFighting)
                {
                    ProcessFightingPhase(f);
                }
                else
                {
                    ProcessEndRound(f);
                }
            }

            f.Global->PhaseTime += f.DeltaTime;
        }

        private void ProcessBuyPhase(Frame f)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (f.Global->PhaseTime > config.BuyPhaseTime)
            {
                StartRound(f);
            }

            f.Events.GetRoundTime(f.Global->IsBuyPhase, config.BuyPhaseTime - f.Global->PhaseTime);
        }

        private void ProcessFightingPhase(Frame f)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (f.Global->IsDelayPassed == false && f.Global->PhaseTime > config.StartFightingPhaseDelay)
            {
                f.Global->IsDelayPassed = true;
            }

            if (IsAllBoardsFinishRound(f) || f.Global->PhaseTime > config.FightPhaseTime)
            {
                f.Global->IsFighting = false;
                f.Global->PhaseTime = 0;
            }

            f.Events.GetRoundTime(f.Global->IsBuyPhase, config.FightPhaseTime - f.Global->PhaseTime);
        }

        private void ProcessEndRound(Frame f)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (f.Global->PhaseTime > config.EndFightingPhaseDelay)
            {
                ProcessResults(f);

                RoundInfo roundInfo = config.GetRoundInfo(f, f.Global->PhaseNumber);

                if (roundInfo.IsPVE == false)
                {
                    f.Global->PVPStreak++;

                    if (f.Global->PVPStreak >= config.PVPStreak)
                    {
                        f.Signals.OnEndRound();
                    }
                    else
                    {
                        f.Signals.ClearBoards();
                        StartRound(f);
                    }
                }
                else
                {
                    f.Signals.OnEndRound();
                }

            }

            f.Events.GetRoundTime(f.Global->IsBuyPhase, config.EndFightingPhaseDelay - f.Global->PhaseTime);
        }

        private bool IsAllBoardsFinishRound(Frame f)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);
            QList<Board> boards = f.ResolveList(f.Global->Boards);
            bool isAllBoardsFinish = true;

            foreach (Board board in boards)
            {
                if (IsBoardFinishRound(f, board, out bool isPlayer1Win, out bool isPlayer2Win, out bool isDraw, out int damage) == false)
                {
                    isAllBoardsFinish = false;
                }
            }

            return isAllBoardsFinish;
        }

        private bool IsBoardFinishRound(Frame f, Board board, out bool isPlayer1Win, out bool isPlayer2Win, out bool isDraw, out int damage)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

            int player1Count = heroes.Count(hero => hero.Hero.TeamNumber == 1 && hero.Hero.IsAlive);
            int player2Count = heroes.Count(hero => hero.Hero.TeamNumber == 2 && hero.Hero.IsAlive);

            isPlayer1Win = player1Count > 0 && player2Count == 0;
            isPlayer2Win = player2Count > 0 && player1Count == 0;
            isDraw = player1Count == 0 && player2Count == 0;
            damage = player1Count + player2Count;

            return isPlayer1Win || isPlayer2Win || isDraw;
        }

        private void ProcessResults(Frame f)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);

            List<(EntityRef entity, PlayerLink playerLink)> playersEntity = new();

            foreach ((EntityRef entity, PlayerLink playerLink) in f.GetComponentIterator<PlayerLink>())
            {
                playersEntity.Add((entity, playerLink));
            }

            foreach (Board board in boards)
            {
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

                int player1Count = heroes.Count(hero => hero.Hero.TeamNumber == 1 && hero.Hero.IsAlive);
                int player2Count = heroes.Count(hero => hero.Hero.TeamNumber == 2 && hero.Hero.IsAlive);

                bool isPlayer1Win = player1Count > 0 && player2Count == 0;
                bool isPlayer2Win = player2Count > 0 && player1Count == 0;
                int damage = player1Count + player2Count;

                if (isPlayer1Win && board.Player2.Ref != default)
                {
                    var playerInfo = playersEntity.First(player => player.playerLink.Ref == board.Player2.Ref);
                    PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(playerInfo.entity);
                    playerLink->Info.Health -= damage;
                }
                else if (isPlayer2Win && board.Player1.Ref != default)
                {
                    var playerInfo = playersEntity.First(player => player.playerLink.Ref == board.Player1.Ref);
                    PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(playerInfo.entity);
                    playerLink->Info.Health -= damage;
                }
            }
        }

        private bool IsRoundStarted(Frame f)
        {
            return f.Global->IsBuyPhase == false;
        }
    }
}
