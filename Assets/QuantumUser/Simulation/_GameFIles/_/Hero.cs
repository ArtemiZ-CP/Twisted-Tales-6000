using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class Hero
    {
        public const int Level1 = 0;
        public const int Level2 = 1;
        public const int Level3 = 2;
        public const int UpgradeClosed = -1;
        public const int UpgradeOpened = 0;
        public const int UpgradeVariant1 = 1;
        public const int UpgradeVariant2 = 2;

        public static int GetHeroCost(Frame f, int heroID)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroInfo heroInfo = gameConfig.GetHeroInfo(f, heroID);
            return heroInfo.GetBuyCost(f);
        }

        public static bool TrySetNewBoardPosition(QList<FightingHero> heroes, ref FightingHero fightingHero, Vector2Int targetPosition)
        {
            if (HeroBoard.TryGetHeroIndexFromCords(targetPosition, out int heroNewIndex))
            {
                if (fightingHero.Index < 0 || heroes[heroNewIndex].Hero.Ref != default)
                {
                    return false;
                }

                fightingHero.TargetPositionX = targetPosition.x;
                fightingHero.TargetPositionY = targetPosition.y;
                FightingHero empty = heroes[heroNewIndex];
                empty.Index = fightingHero.Index;
                heroes[fightingHero.Index] = empty;
                fightingHero.Index = heroNewIndex;
                heroes[heroNewIndex] = fightingHero;
                return true;
            }

            return false;
        }

        public static void Spawn(Frame f, QList<HeroEntity> heroes, PlayerRef playerRef, int heroIndex, bool first)
        {
            HeroEntity hero = heroes[heroIndex];

            if (hero.ID < 0) return;

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroInfo heroInfo = gameConfig.GetHeroInfo(f, hero.ID);
            EntityRef heroEntity = f.Create(heroInfo.HeroPrototype);
            hero.Ref = heroEntity;
            hero.Player = playerRef;
            heroes[heroIndex] = hero;

            switch (heroInfo.HeroType)
            {
                case HeroAttackType.Melee:
                    f.Add<MeleeHero>(heroEntity);
                    break;
                case HeroAttackType.Ranged:
                    f.Add<RangedHero>(heroEntity);
                    break;
            }

            SetHeroTransform(f, hero, first);
            BoardSystem.DisactiveEntity(f, heroEntity);
        }

        public static QListPtr<HeroEntity> SetupHeroes(Frame f, PlayerLink* player)
        {
            if (player->Ref == default)
            {
                return default;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            QListPtr<HeroEntity> heroes = f.AllocateList<HeroEntity>();
            QList<HeroIdLevel> playerHeroesIDLevel = f.ResolveList(player->Info.Board.Heroes);
            QList<HeroEntity> playerHeroes = f.ResolveList(heroes);

            for (int i = 0; i < playerHeroesIDLevel.Count; i++)
            {
                int id = playerHeroesIDLevel[i].ID;
                HeroInfo heroInfo = gameConfig.GetHeroInfo(f, id);
                HeroEntity hero;

                if (heroInfo == null)
                {
                    hero = new()
                    {
                        ID = -1,
                        Level = -1,
                        NameIndex = -1,
                        TypeIndex = -1,
                        DefaultPosition = HeroBoard.GetTilePosition(f, i % GameplayConstants.BoardSize, i / GameplayConstants.BoardSize)
                    };
                }
                else
                {
                    hero = new()
                    {
                        ID = id,
                        Level = playerHeroesIDLevel[i].Level,
                        NameIndex = (int)heroInfo.Name,
                        TypeIndex = (int)heroInfo.Type,
                        DefaultPosition = HeroBoard.GetTilePosition(f, i % GameplayConstants.BoardSize, i / GameplayConstants.BoardSize)
                    };
                }

                playerHeroes.Add(hero);
            }

            return heroes;
        }

        public static QListPtr<HeroEntity> SetupHeroes(Frame f, RoundInfo roundInfo)
        {
            if (roundInfo == null)
            {
                return default;
            }

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            QListPtr<HeroEntity> heroes = f.AllocateList<HeroEntity>();
            QList<HeroEntity> playerHeroes = f.ResolveList(heroes);

            for (int i = 0; i < roundInfo.PVEBoard.Count; i++)
            {
                for (int j = 0; j < roundInfo.PVEBoard[i].Cells.Count; j++)
                {
                    int id = roundInfo.PVEBoard[i].Cells[^(j + 1)];
                    HeroInfo heroInfo = gameConfig.GetHeroInfo(f, id);
                    HeroEntity hero;

                    if (heroInfo == null)
                    {
                        hero = new()
                        {
                            ID = -1,
                            Level = -1,
                            NameIndex = -1,
                            TypeIndex = -1,
                            DefaultPosition = HeroBoard.GetTilePosition(f, i % GameplayConstants.BoardSize, i / GameplayConstants.BoardSize)
                        };
                    }
                    else
                    {
                        hero = new()
                        {
                            ID = id,
                            Level = Level1,
                            NameIndex = (int)heroInfo.Name,
                            TypeIndex = (int)heroInfo.Type,
                            DefaultPosition = HeroBoard.GetTilePosition(f, j, i),
                        };
                    }

                    playerHeroes.Add(hero);
                }
            }

            return heroes;
        }

        public static FightingHero SetupHero(Frame f, FightingHero hero, int heroIndex)
        {
            if (hero.Hero.ID < 0)
            {
                hero.IsAlive = false;
                return hero;
            }

            GameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            if (HeroBoard.TryGetHeroCordsFromIndex(heroIndex, out Vector2Int position))
            {
                hero.TargetPositionX = position.x;
                hero.TargetPositionY = position.y;
            }
            else
            {
                throw new System.NotSupportedException();
            }

            hero.Hero.RangePercentage = config.RangePercentage;
            hero.Hero.ManaRegen = config.ManaRegen;
            hero.IsAlive = true;
            hero.AttackTimer = 0;

            HeroStats heroStats = config.GetHeroStats(f, hero, BoardSystem.GetBoards(f)[hero.BoardIndex]);
            hero.Hero.MaxMana = heroStats.Mana;
            hero.CurrentMana = heroStats.StartMana;
            hero.Hero.AttackDamageType = (int)heroStats.AttackDamageType;

            HeroLevelStats heroLevelStats = heroStats.LevelStats[hero.Hero.Level];
            hero.Hero.Health = heroLevelStats.Health;
            hero.CurrentHealth = heroLevelStats.Health;
            hero.CurrentArmor = 0;
            hero.Hero.Defense = heroLevelStats.Defense;
            hero.Hero.MagicDefense = heroLevelStats.MagicDefense;
            hero.Hero.AttackDamage = heroLevelStats.AttackDamage;
            hero.Hero.AttackSpeed = heroLevelStats.AttackSpeed;
            hero.Hero.ProjectileSpeed = heroLevelStats.ProjectileSpeed;
            hero.Hero.Range = heroLevelStats.Range;

            return hero;
        }

        public static void Rotate(Frame f, HeroEntity hero, FPVector3 targetPosition)
        {
            Transform3D* heroTransform = f.Unsafe.GetPointer<Transform3D>(hero.Ref);

            FPVector3 direction = targetPosition - heroTransform->Position;

            if (direction == FPVector3.Zero)
            {
                return;
            }

            FPQuaternion targetRotation = FPQuaternion.LookRotation(direction);

            Rotate(f, hero, targetRotation);
        }

        public static void Rotate(Frame f, HeroEntity hero, FPQuaternion targetRotation)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);

            Transform3D* heroTransform = f.Unsafe.GetPointer<Transform3D>(hero.Ref);

            if (heroTransform->Rotation.Equals(targetRotation))
            {
                return;
            }

            FP rotationDelta = gameConfig.HeroRotationSpeed * f.DeltaTime;

            heroTransform->Rotation = FPQuaternion.RotateTowards(heroTransform->Rotation, targetRotation, rotationDelta);
        }

        private static void SetHeroTransform(Frame f, HeroEntity hero, bool first = true)
        {
            Transform3D* heroTransform = f.Unsafe.GetPointer<Transform3D>(hero.Ref);

            if (first)
            {
                heroTransform->Position = hero.DefaultPosition;
                heroTransform->Rotation = FPQuaternion.LookRotation(FPVector3.Forward);
            }
            else
            {
                heroTransform->Position = -hero.DefaultPosition;
                heroTransform->Rotation = FPQuaternion.LookRotation(FPVector3.Back);
            }
        }
    }
}
