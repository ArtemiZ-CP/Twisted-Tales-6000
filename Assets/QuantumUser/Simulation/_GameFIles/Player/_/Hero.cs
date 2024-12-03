using Quantum.Collections;

namespace Quantum.Game
{
    public unsafe class Hero
    {
        public static void SetNewBoardPosision(QList<FightingHero> heroes, FightingHero fightingHero, int heroNewIndex)
        {
            FightingHero empty = heroes[heroNewIndex];
            empty.Index = fightingHero.Index;
            heroes[fightingHero.Index] = empty;
            fightingHero.Index = heroNewIndex;
            heroes[heroNewIndex] = fightingHero;
        }

        public static void Spawn(Frame f, QList<HeroEntity> heroes, int heroIndex, int teamNumber, bool first)
        {
            HeroEntity hero = heroes[heroIndex];

            if (hero.ID < 0) return;

            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            EntityRef heroEntity = f.Create(gameConfig.GetHeroPrototype(f, hero.ID));
            hero.Ref = heroEntity;
            hero.TeamNumber = teamNumber;
            heroes[heroIndex] = hero;

            SetHeroPosition(f, hero, first);
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
                        Level = 0,
                        DefaultPosition = HeroBoard.GetTilePosition(f, j, i),
                    };

                    playerHeroes.Add(hero);
                }
            }

            return heroes;
        }

        private static void SetHeroPosition(Frame f, HeroEntity hero, bool first = true)
        {
            Transform3D* heroTransform = f.Unsafe.GetPointer<Transform3D>(hero.Ref);

            if (first)
            {
                heroTransform->Position = hero.DefaultPosition;
            }
            else
            {
                heroTransform->Position = -hero.DefaultPosition;
            }
        }

    }
}
