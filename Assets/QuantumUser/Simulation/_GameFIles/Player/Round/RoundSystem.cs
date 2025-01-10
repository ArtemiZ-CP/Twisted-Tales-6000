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
            if (IsGameStarted(f) == false) return;

            ProcessRound(f);
        }

        public void GetPlayersList(Frame f)
        {
            f.Events.GetCurrentPlayers(f, Player.GetAllPlayersLink(f), BoardSystem.GetBoards(f));
        }

        public void OnStartRound(Frame f)
        {
            if (IsRoundStarted(f) || IsGameStarted(f) == false) return;

            StartRound(f);
        }

        public void OnEndRound(Frame f)
        {
            if (IsRoundStarted(f) == false || IsGameStarted(f) == false) return;

            ProcessEndRound(f, finishAnyway: true);
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
            Shop.AddXP(f, f.FindAsset(f.RuntimeConfig.GameConfig).XPByRound);
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
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (f.Global->PhaseTime > gameConfig.BuyPhaseTime)
            {
                StartRound(f);
            }

            f.Events.GetRoundTime(f.Global->IsBuyPhase, IsPVPRound: false, gameConfig.BuyPhaseTime - f.Global->PhaseTime);
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

            f.Events.GetRoundTime(f.Global->IsBuyPhase, f.Global->IsPVPRound, config.FightPhaseTime - f.Global->PhaseTime);
        }

        private void ProcessEndRound(Frame f, bool finishAnyway = false)
        {
            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (finishAnyway || f.Global->PhaseTime > config.EndFightingPhaseDelay)
            {
                ProcessResults(f);

                RoundInfo roundInfo = config.GetRoundInfo(f, f.Global->PhaseNumber);

                if (roundInfo.IsPVE == false)
                {
                    f.Global->PVPStreak++;

                    if (f.Global->PVPStreak >= config.PVPStreak)
                    {
                        Shop.Reload(f);
                        EndRound(f);
                        ProcessPlayersCoins(f);
                    }
                    else
                    {
                        f.Signals.ClearBoards();
                        StartRound(f);
                    }
                }
                else
                {
                    Shop.Reload(f);
                    EndRound(f);
                    ProcessPlayersCoins(f);
                }

            }

            f.Events.GetRoundTime(f.Global->IsBuyPhase, f.Global->IsPVPRound, config.EndFightingPhaseDelay - f.Global->PhaseTime);
        }

        private bool IsAllBoardsFinishRound(Frame f)
        {
            List<Board> boards = BoardSystem.GetBoards(f);
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
            List<Board> boards = BoardSystem.GetBoards(f);

            var playersEntity = Player.GetAllPlayers(f);

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
                    var (entity, link) = playersEntity.First(player => player.link.Ref == board.Player2.Ref);
                    Player.GetPlayerPointer(f, entity)->Info.Health -= damage;
                }
                else if (isPlayer2Win && board.Player1.Ref != default)
                {
                    var (entity, link) = playersEntity.First(player => player.link.Ref == board.Player1.Ref);
                    Player.GetPlayerPointer(f, entity)->Info.Health -= damage;
                }
            }
        }

        private bool IsGameStarted(Frame f) => f.Global->IsGameStarted;

        private bool IsRoundStarted(Frame f) => f.Global->IsBuyPhase == false;

        private void ProcessPlayersCoins(Frame f)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (gameConfig.ResetCoinsOnEndRound)
            {
                Player.ResetCoins(f);
            }

            int coins;

            if (f.Global->PhaseNumber < gameConfig.CoinsPerRound.Count)
            {
                coins = gameConfig.CoinsPerRound[f.Global->PhaseNumber];
            }
            else
            {
                coins = gameConfig.CoinsPerRound[^1];
            }

            Player.AddCoins(f, coins);
        }
    }
}