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
            f.Events.GetCurrentPlayers(f, Player.GetAllPlayerLinks(f), BoardSystem.GetBoards(f));
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
            Shop.ReloadOnEndRound(f);
            f.Global->IsBuyPhase = true;
            f.Global->PhaseNumber++;
            f.Global->PVPStreak = 0;
            f.Global->PhaseTime = 0;
            f.Signals.ClearBoards();
            f.Events.EndRound();
            Shop.AddXP(f, f.FindAsset(f.RuntimeConfig.GameConfig).XPByRound);
            GetPlayersList(f);
            Events.GetBoardHeroes(f);
            Events.GetInventoryHeroes(f);
            Shop.SetFreezeShop(f, isLocked: false);
            Events.DisplayRoundNumber(f);
            QList<Board> boards = f.ResolveList(f.Global->Boards);
            boards.Clear();
            f.Signals.BotStartRound();
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

                RoundInfo roundInfo = config.GetRoundInfo(f.Global->PhaseNumber);

                if (roundInfo.IsPVE == false)
                {
                    f.Global->PVPStreak++;

                    if (f.Global->PVPStreak >= config.PVPStreak)
                    {
                        EndRound(f);
                        ProcessPlayersCoins(f);
                    }
                    else
                    {
                        f.Signals.ClearBoards();
                        StartRound(f);
                        ProcessPlayersCoins(f);
                    }
                }
                else
                {
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

            int player1Count = heroes.Count(hero => hero.TeamNumber == GameplayConstants.Team1 && hero.IsAlive);
            int player2Count = heroes.Count(hero => hero.TeamNumber == GameplayConstants.Team2 && hero.IsAlive);

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

                int player1StarsCount = heroes.Where(hero => hero.TeamNumber == GameplayConstants.Team1 && hero.IsAlive)
                                            .Sum(hero => hero.Hero.Level + 1);
                int player2StarsCount = heroes.Where(hero => hero.TeamNumber == GameplayConstants.Team2 && hero.IsAlive)
                                            .Sum(hero => hero.Hero.Level + 1);

                bool isPlayer1Win = player1StarsCount > 0 && player2StarsCount == 0;
                bool isPlayer2Win = player2StarsCount > 0 && player1StarsCount == 0;
                int damage = player1StarsCount + player2StarsCount;

                ProcessResult(f, board, isPlayer1Win, isPlayer2Win, damage);
            }
        }

        private void ProcessResult(Frame f, Board board, bool isPlayer1Win, bool isPlayer2Win, int damage)
        {
            ProcessResult(f, board.Player1.Ref, board.Player2.Ref, isPlayer1Win, isPlayer2Win, damage);
            ProcessResult(f, board.Player2.Ref, board.Player1.Ref, isPlayer2Win, isPlayer1Win, damage);
        }

        private void ProcessResult(Frame f, PlayerRef targetRef, PlayerRef enemyRef, bool isPlayerWin, bool isEnemyWin, int damage)
        {
            if (targetRef != default)
            {
                PlayerLink* playerLink = Player.GetPlayerPointer(f, targetRef);
                int roundResult = 0;

                if (isPlayerWin)
                {
                    roundResult = 1;
                }
                else if (isEnemyWin)
                {
                    Shop.SetRollCost(f, playerLink, freeRoll: true);
                    playerLink->Info.Health -= damage;
                    roundResult = -1;
                }

                ProcessStreak(f, playerLink, roundResult);
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

            var players = Player.GetAllPlayersEntity(f);

            foreach (EntityRef player in players)
            {
                ProcessPlayerCoins(f, Player.GetPlayerPointer(f, player));
            }
        }

        private void ProcessPlayerCoins(Frame f, PlayerLink* player)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            int baseCoins;

            if (f.Global->PhaseNumber < gameConfig.CoinsPerRound.Count)
            {
                baseCoins = gameConfig.CoinsPerRound[f.Global->PhaseNumber];
            }
            else
            {
                baseCoins = gameConfig.CoinsPerRound[^1];
            }

            int roundResultCoins = 0;
            int streakCoins = 0;

            if (player->Info.StreakType != 0)
            {
                int streakIndex = player->Info.Streak;

                if (player->Info.StreakType > 0)
                {
                    roundResultCoins = gameConfig.CoinsPerWin;

                    if (streakIndex < gameConfig.WinStreakCoins.Count)
                    {
                        streakCoins = gameConfig.WinStreakCoins[streakIndex];
                    }
                    else
                    {
                        streakCoins = gameConfig.WinStreakCoins[^1];
                    }
                }
                else if (player->Info.StreakType < 0)
                {
                    roundResultCoins = gameConfig.CoinsPerLose;

                    if (streakIndex < gameConfig.LoseStreakCoins.Count)
                    {
                        streakCoins = gameConfig.LoseStreakCoins[streakIndex];
                    }
                    else
                    {
                        streakCoins = gameConfig.LoseStreakCoins[^1];
                    }
                }
            }

            Player.AddCoins(f, player, baseCoins + roundResultCoins + streakCoins);
            f.Events.ShowCoinsReward(player->Ref, player->Info.StreakType, baseCoins, roundResultCoins, streakCoins);
        }

        private void ProcessStreak(Frame f, PlayerLink* player, int roundResult)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (roundResult == player->Info.StreakType)
            {
                player->Info.Streak++;

                if (gameConfig.ResetWinStreakOnEnd && roundResult > 0 && player->Info.Streak >= gameConfig.WinStreakCoins.Count)
                {
                    player->Info.Streak = 0;
                }
                else if (gameConfig.ResetLoseStreakOnEnd && roundResult < 0 && player->Info.Streak >= gameConfig.LoseStreakCoins.Count)
                {
                    player->Info.Streak = 0;
                }
            }
            else
            {
                player->Info.Streak = 0;
                player->Info.StreakType = roundResult;
            }
        }
    }
}