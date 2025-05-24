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

        public static bool TrySetNewBoardPosition(QList<FightingHero> heroes, ref FightingHero fightingHero,  Vector2Int targetPosition)
        {
            if (HeroBoard.TryConvertCordsToIndex(targetPosition, out int heroNewIndex))
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
                case HeroType.Melee:
                    f.Add<MeleeHero>(heroEntity);
                    break;
                case HeroType.Ranged:
                    f.Add<RangedHero>(heroEntity);
                    break;
            }

            SetHeroTransform(f, hero, first);
            BoardSystem.DisactiveEntity(f, heroEntity);
        }

        public static QListPtr<HeroEntity> SetupHeroes(Frame f, PlayerLink* player, QListPtr<HeroEntity> heroes)
        {
            if (player->Ref == default)
            {
                return heroes;
            }

            heroes = f.AllocateList<HeroEntity>();
            QList<int> playerHeroesID = f.ResolveList(player->Info.Board.HeroesID);
            QList<int> playerHeroesLevel = f.ResolveList(player->Info.Board.HeroesLevel);
            QList<HeroEntity> playerHeroes = f.ResolveList(heroes);

            for (int i = 0; i < playerHeroesID.Count; i++)
            {
                HeroEntity hero = new()
                {
                    ID = playerHeroesID[i],
                    Level = playerHeroesLevel[i],
                    DefaultPosition = HeroBoard.GetTilePosition(f, i % GameConfig.BoardSize, i / GameConfig.BoardSize)
                };

                playerHeroes.Add(hero);
            }

            return heroes;
        }

        public static QListPtr<HeroEntity> SetupHeroes(Frame f, RoundInfo roundInfo, QListPtr<HeroEntity> heroes)
        {
            if (roundInfo == null)
            {
                return heroes;
            }

            heroes = f.AllocateList<HeroEntity>();
            QList<HeroEntity> playerHeroes = f.ResolveList(heroes);

            for (int i = 0; i < roundInfo.PVEBoard.Count; i++)
            {
                for (int j = 0; j < roundInfo.PVEBoard[i].Cells.Count; j++)
                {
                    HeroEntity hero = new()
                    {
                        ID = roundInfo.PVEBoard[i].Cells[^(j + 1)],
                        Level = Hero.Level1,
                        DefaultPosition = HeroBoard.GetTilePosition(f, j, i),
                    };

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

            if (HeroBoard.TryGetHeroCords(heroIndex, out Vector2Int position))
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

            HeroInfo heroInfo = config.GetHeroInfo(f, hero.Hero.ID);
            hero.Hero.MaxMana = heroInfo.Mana;
            hero.CurrentMana = heroInfo.StartMana;
            hero.Hero.AttackDamageType = (int)heroInfo.AttackDamageType;

            HeroLevelStats heroLevelStats = heroInfo.HeroStats[hero.Hero.Level];
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
