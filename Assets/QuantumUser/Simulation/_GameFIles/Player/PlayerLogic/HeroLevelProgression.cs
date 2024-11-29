using System.Linq;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroLevelProgression : SystemMainThreadFilter<HeroLevelProgression.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (TryGetHeroToUpgrade(f, filter.PlayerLink, out int heroIDToUpgrade, out int heroLevelToUpgrade))
            {
                UpgradeHero(f, filter.PlayerLink, heroIDToUpgrade, heroLevelToUpgrade);
            }
        }

        private bool TryGetHeroToUpgrade(Frame f, PlayerLink* playerLink, out int heroIDToUpgrade, out int heroLevelToUpgrade)
        {
            QList<int> heroesInventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            QList<int> heroesLevelInventory = f.ResolveList(playerLink->Info.Inventory.HeroesLevel);
            QList<int> heroesBoard = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> heroesLevelBoard = f.ResolveList(playerLink->Info.Board.HeroesLevel);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

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
                        heroIDToUpgrade = heroID;
                        heroLevelToUpgrade = heroesCount.ToList().IndexOf(count);
                        return true;
                    }
                }
            }

            heroIDToUpgrade = -1;
            heroLevelToUpgrade = -1;
            return false;
        }

        private void UpgradeHero(Frame f, PlayerLink* playerLink, int heroID, int heroLevel)
        {
            QList<int> heroesInventory = f.ResolveList(playerLink->Info.Inventory.HeroesID);
            QList<int> heroesLevelInventory = f.ResolveList(playerLink->Info.Inventory.HeroesLevel);
            QList<int> heroesBoard = f.ResolveList(playerLink->Info.Board.HeroesID);
            QList<int> heroesLevelBoard = f.ResolveList(playerLink->Info.Board.HeroesLevel);
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

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
                if (heroesCount == gameConfig.HeroesCountToUpgrade)
                {
                    break;
                }

                if (heroesInventory[i] == heroID && heroesLevelInventory[i] == heroLevel)
                {
                    heroesInventory[i] = -1;
                    heroesLevelInventory[i] = 0;
                    heroesCount++;

                    if (heroesCount == gameConfig.HeroesCountToUpgrade)
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < heroesBoard.Count; i++)
            {
                if (heroesCount == gameConfig.HeroesCountToUpgrade)
                {
                    break;
                }

                if (heroesBoard[i] == heroID && heroesLevelBoard[i] == heroLevel)
                {
                    heroesBoard[i] = -1;
                    heroesLevelBoard[i] = 0;
                    heroesCount++;

                    if (heroesCount == gameConfig.HeroesCountToUpgrade)
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

            f.Events.GetPlayerInfo(f, playerLink->Ref, playerLink->Info);
        }
    }
}
