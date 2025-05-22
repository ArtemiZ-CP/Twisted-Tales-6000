using System.Collections.Generic;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class HeroLevelProgression : SystemSignalsOnly, ISignalTryUpgradeHero, ISignalOnStartRound, ISignalOnEndRound, ISignalLevelUpHero
    {
        public void OnEndRound(Frame f)
        {
            var playerEntity = Player.GetAllPlayerEntities(f);

            for (int i = 0; i < playerEntity.Count; i++)
            {
                TryUpgradeHero(f, Player.GetPlayerPointer(f, playerEntity[i]));
            }
        }

        public void OnStartRound(Frame f)
        {
            var playerLinks = Player.GetAllPlayerLinks(f);

            for (int i = 0; i < playerLinks.Count; i++)
            {
                PlayerLink playerLink = playerLinks[i];
                QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink.Info.Board.Abilities);

                for (int j = 0; j < abilities.Count; j++)
                {
                    SelectedHeroAbility selectedHeroAbility = abilities[j];

                    if (selectedHeroAbility.SecondAbilityIndex == 0)
                    {
                        f.Events.LevelUpHero(playerLink.Ref, selectedHeroAbility.HeroID, Hero.Level2, IsCompleted: false);
                    }

                    if (selectedHeroAbility.ThirdAbilityIndex == 0)
                    {
                        f.Events.LevelUpHero(playerLink.Ref, selectedHeroAbility.HeroID, Hero.Level3, IsCompleted: false);
                    }
                }
            }
        }

        public void LevelUpHero(Frame f, PlayerLink* playerLink, int heroID, int heroLevel, int upgradeLevel)
        {
            QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink->Info.Board.Abilities);

            for (int j = 0; j < abilities.Count; j++)
            {
                SelectedHeroAbility selectedHeroAbility = abilities[j];

                if (selectedHeroAbility.HeroID == heroID)
                {
                    if (heroLevel == Hero.Level2 && selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeOpened)
                    {
                        selectedHeroAbility.SecondAbilityIndex = upgradeLevel;
                    }
                    else if (heroLevel == Hero.Level3 && selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeOpened)
                    {
                        selectedHeroAbility.ThirdAbilityIndex = upgradeLevel;
                    }
                    else
                    {
                        return;
                    }

                    abilities[j] = selectedHeroAbility;
                    f.Events.LevelUpHero(playerLink->Ref, heroID, heroLevel, IsCompleted: true);
                    return;
                }
            }
        }

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

            if (f.Global->IsBuyPhase)
            {
                for (int i = 0; i < heroesBoard.Count; i++)
                {
                    if (heroesBoard[i] == heroID && heroesLevelBoard[i] == heroLevel)
                    {
                        heroCount++;
                    }
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
            QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink->Info.Board.Abilities);

            bool onBoard = false;
            int heroIndex = -1;
            int nextLevel = heroLevel + 1;

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
                heroesLevelBoard[heroIndex] = nextLevel;
            }
            else
            {
                heroesInventory[heroIndex] = heroID;
                heroesLevelInventory[heroIndex] = nextLevel;
            }

            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, heroID, out int index);

            if (nextLevel == Hero.Level2 && selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeClosed)
            {
                selectedHeroAbility.SecondAbilityIndex = Hero.UpgradeOpened;
                abilities[index] = selectedHeroAbility;
                f.Events.LevelUpHero(playerLink->Ref, heroID, nextLevel, IsCompleted: false);
            }
            else if (nextLevel == Hero.Level3 && selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeClosed)
            {
                selectedHeroAbility.ThirdAbilityIndex = Hero.UpgradeOpened;
                abilities[index] = selectedHeroAbility;
                f.Events.LevelUpHero(playerLink->Ref, heroID, nextLevel, IsCompleted: false);
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

                if (f.Global->IsBuyPhase)
                {
                    for (int i = 0; i < heroesBoard.Count; i++)
                    {
                        if (heroesBoard[i] == heroID)
                        {
                            heroesCount[heroesLevelBoard[i]]++;
                        }
                    }
                }

                for (int level = 0; level < heroesCount.Length; level++)
                {
                    int count = heroesCount[level];

                    if (count >= gameConfig.HeroesCountToUpgrade)
                    {
                        heroUpgradeInfos.Add(new HeroUpgradeInfo
                        {
                            HeroID = heroID,
                            HeroLevel = level
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
