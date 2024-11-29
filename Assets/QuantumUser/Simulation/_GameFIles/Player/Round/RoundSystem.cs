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

            if (IsAllBoardsFinishRound(f, out var results) || f.Global->PhaseTime > config.FightPhaseTime)
            {
                ProcessResults(f, results);
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

        private bool IsAllBoardsFinishRound(Frame f, out List<(Board board, bool isPlayer1Win, bool isPlayer2Win, bool isDraw, int damage)> results)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);
            QList<Board> boards = f.ResolveList(f.Global->Boards);
            results = new();
            bool isAllBoardsFinish = true;

            foreach (Board board in boards)
            {
                if (IsBoardFinishRound(f, board, out bool isPlayer1Win, out bool isPlayer2Win, out bool isDraw, out int damage) == false)
                {
                    isAllBoardsFinish = false;
                }

                results.Add((board, isPlayer1Win, isPlayer2Win, isDraw, damage));
            }

            return isAllBoardsFinish;
        }

        private bool IsBoardFinishRound(Frame f, Board board, out bool isPlayer1Win, out bool isPlayer2Win, out bool isDraw, out int damage)
        {
            QList<Hero> heroes = f.ResolveList(board.FightingHeroesMap);

            int player1Count = heroes.Count(hero => hero.TeamNumber == 1 && hero.IsAlive);
            int player2Count = heroes.Count(hero => hero.TeamNumber == 2 && hero.IsAlive);

            isPlayer1Win = player1Count > 0 && player2Count == 0;
            isPlayer2Win = player2Count > 0 && player1Count == 0;
            isDraw = player1Count == 0 && player2Count == 0;
            damage = player1Count + player2Count;

            return isPlayer1Win || isPlayer2Win || isDraw;
        }

        private void ProcessResults(Frame f, List<(Board board, bool isPlayer1Win, bool isPlayer2Win, bool isDraw, int damage)> results)
        {
            List<(EntityRef entity, PlayerLink playerLink)> playersEntity = new();

            foreach ((EntityRef entity, PlayerLink playerLink) in f.GetComponentIterator<PlayerLink>())
            {
                playersEntity.Add((entity, playerLink));
            }

            for (int i = 0; i < results.Count; i++)
            {
                (Board board, bool isPlayer1Win, bool isPlayer2Win, bool isDraw, int damage) = results[i];

                if (isPlayer1Win)
                {
                    var playerInfo = playersEntity.First(player => player.playerLink.Ref == board.Player2.Ref);
                    PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(playerInfo.entity);
                    playerLink->Info.Health -= damage;
                }
                else if (isPlayer2Win)
                {
                    var playerInfo = playersEntity.First(player => player.playerLink.Ref == board.Player1.Ref);
                    PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(playerInfo.entity);
                    playerLink->Info.Health -= damage;
                }

                Log.Debug($"Results processed {damage}");
            }

            GetPlayersList(f);
        }

        private bool IsRoundStarted(Frame f)
        {
            return f.Global->IsBuyPhase == false;
        }
    }
}
