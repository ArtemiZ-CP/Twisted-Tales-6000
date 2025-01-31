using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroLevelProgression : SystemSignalsOnly, ISignalTryUpgradeHero
    {
        public void TryUpgradeHero(Frame f, PlayerLink* playerLink)
        {
            while (TryGetHeroesToUpgrade(f, playerLink, out List<HeroUpgradeInfo> heroUpgradeInfos))
            {
                UpgradeHeroes(f, playerLink, heroUpgradeInfos);
            }
            
            HeroMovingSystem.ShowHeroesOnBoardCount(f, *playerLink);
        }

        public static int GetHeroCount(Frame f, PlayerLink* playerLink, int heroID, int heroLevel)
        {
            QList<int> heroesInventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            QList<int> heroesLevelInventory = f.ResolveList(playerLink->Info.Inventory.HeroesLevel);
            QList<int> heroesBoard = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> heroesLevelBoard = f.ResolveList(playerLink->Info.Board.HeroesLevel);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            int heroCount = 0;

            for (int i = 0; i < heroesInventory.Count; i++)
            {
                if (heroesInventory[i] == heroID && heroesLevelInventory[i] == heroLevel)
                {
                    heroCount++;
                }
            }

            for (int i = 0; i < heroesBoard.Count; i++)
            {
                if (heroesBoard[i] == heroID && heroesLevelBoard[i] == heroLevel)
                {
                    heroCount++;
                }
            }

            return heroCount;
        }

        public static void UpgradeHero(Frame f, PlayerLink* playerLink, int heroID, int heroLevel, int heroesCountToUpgrade)
        {
            QList<int> heroesInventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            QList<int> heroesLevelInventory = f.ResolveList(playerLink->Info.Inventory.HeroesLevel);
            QList<int> heroesBoard = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> heroesLevelBoard = f.ResolveList(playerLink->Info.Board.HeroesLevel);

            bool onBoard = false;
            int heroIndex = -1;

            for (int i = 0; i < heroesBoard.Count; i++)
            {
                if (heroesBoard[i] == heroID && heroesLevelBoard[i] == heroLevel)
                {
                    onBoard = true;
                    heroIndex = i;
                    break;
                }
            }

            if (onBoard == false)
            {
                for (int i = 0; i < heroesInventory.Count; i++)
                {
                    if (heroesInventory[i] == heroID && heroesLevelInventory[i] == heroLevel)
                    {
                        heroIndex = i;
                        break;
                    }
                }
            }

            int heroesCount = 0;

            for (int i = 0; i < heroesInventory.Count; i++)
            {
                if (heroesCount == heroesCountToUpgrade)
                {
                    break;
                }

                if (heroesInventory[i] == heroID && heroesLevelInventory[i] == heroLevel)
                {
                    heroesInventory[i] = -1;
                    heroesLevelInventory[i] = 0;
                    heroesCount++;

                    if (heroesCount == heroesCountToUpgrade)
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < heroesBoard.Count; i++)
            {
                if (heroesCount == heroesCountToUpgrade)
                {
                    break;
                }

                if (heroesBoard[i] == heroID && heroesLevelBoard[i] == heroLevel)
                {
                    heroesBoard[i] = -1;
                    heroesLevelBoard[i] = 0;
                    heroesCount++;

                    if (heroesCount == heroesCountToUpgrade)
                    {
                        break;
                    }
                }
            }

            if (onBoard)
            {
                heroesBoard[heroIndex] = heroID;
                heroesLevelBoard[heroIndex] = heroLevel + 1;
            }
            else
            {
                heroesInventory[heroIndex] = heroID;
                heroesLevelInventory[heroIndex] = heroLevel + 1;
            }

            f.Events.GetBoardHeroes(f, playerLink->Ref,
                f.ResolveList(playerLink->Info.Board.HeroesID),
                f.ResolveList(playerLink->Info.Board.HeroesLevel));
            f.Events.GetInventoryHeroes(f, playerLink->Ref,
                f.ResolveList(playerLink->Info.Inventory.HeroesID),
                f.ResolveList(playerLink->Info.Inventory.HeroesLevel));
        }

        private bool TryGetHeroesToUpgrade(Frame f, PlayerLink* playerLink, out List<HeroUpgradeInfo> heroUpgradeInfos)
        {
            QList<int> heroesInventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            QList<int> heroesLevelInventory = f.ResolveList(playerLink->Info.Inventory.HeroesLevel);
            QList<int> heroesBoard = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> heroesLevelBoard = f.ResolveList(playerLink->Info.Board.HeroesLevel);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            heroUpgradeInfos = new List<HeroUpgradeInfo>();

            for (int heroID = 0; heroID < gameConfig.HeroInfos.Length; heroID++)
            {
                int[] heroesCount = new int[gameConfig.MaxLevel];

                for (int i = 0; i < heroesInventory.Count; i++)
                {
                    if (heroesInventory[i] == heroID)
                    {
                        heroesCount[heroesLevelInventory[i]]++;
                    }
                }

                for (int i = 0; i < heroesBoard.Count; i++)
                {
                    if (heroesBoard[i] == heroID)
                    {
                        heroesCount[heroesLevelBoard[i]]++;
                    }
                }

                foreach (int count in heroesCount)
                {
                    if (count >= gameConfig.HeroesCountToUpgrade)
                    {
                        heroUpgradeInfos.Add(new HeroUpgradeInfo
                        {
                            HeroID = heroID,
                            HeroLevel = heroesCount.ToList().IndexOf(count)
                        });
                    }
                }
            }

            if (heroUpgradeInfos.Count > 0)
            {
                return true;
            }

            return false;
        }

        private void UpgradeHeroes(Frame f, PlayerLink* playerLink, List<HeroUpgradeInfo> heroUpgradeInfos)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            foreach (HeroUpgradeInfo heroUpgradeInfo in heroUpgradeInfos)
            {
                UpgradeHero(f, playerLink, heroUpgradeInfo.HeroID, heroUpgradeInfo.HeroLevel, gameConfig.HeroesCountToUpgrade);
            }
        }

        private struct HeroUpgradeInfo
        {
            public int HeroID;
            public int HeroLevel;
        }
    }
}
