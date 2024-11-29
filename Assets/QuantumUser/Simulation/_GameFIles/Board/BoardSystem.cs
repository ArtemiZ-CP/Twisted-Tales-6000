using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BoardSystem : SystemSignalsOnly, ISignalMakeNewBoardPVP, ISignalMakeNewBoardPVE, ISignalClearBoards
    {
        public static void DisactiveEntity(Frame f, EntityRef entity)
        {
            GetGameObject(f, entity).SetActive(false);
        }

        public static GameObject GetGameObject(Frame f, EntityRef entity)
        {
            if (f.TryGet(entity, out View view))
            {
                EntityView entityView = f.FindAsset(view.Current);
                return entityView.Prefab;
            }

            return null;
        }

        public override void OnInit(Frame f)
        {
            f.Global->Boards = f.AllocateList<Board>();
            ClearBoards(f);
        }

        public void MakeNewBoardPVP(Frame f, PlayerLink player1, PlayerLink player2, QBoolean main)
        {
            EntityRef boardRef = SpawnBoard(f, &player1, &player2, main);
            ActiveBoard(f, boardRef);
        }

        public void MakeNewBoardPVE(Frame f, PlayerLink player, RoundInfo roundInfo)
        {
            EntityRef boardRef = SpawnBoard(f, &player, roundInfo);
            ActiveBoard(f, boardRef);
        }

        public void ClearBoards(Frame f)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);

            for (int i = 0; i < boards.Count; i++)
            {
                Board board = boards[i];
                ClearBoard(f, board);
            }

            boards.Clear();
        }

        private void ClearBoard(Frame f, Board board)
        {
            EntityRef boardEntity = board.Ref;
            ClearHeroes(f, board.Heroes1);
            ClearHeroes(f, board.Heroes2);
            ClearProjectiles(f, board.HeroProjectiles);
            f.FreeList(board.FightingHeroesMap);
            f.Destroy(boardEntity);
        }

        private void ActiveBoard(Frame f, EntityRef boardEntity)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);
            Board* playerBoard = f.Unsafe.GetPointer<Board>(boardEntity);

            SpawnHeroes(f, playerBoard);
        }

        private void ClearHeroes(Frame f, QListPtr<Hero> heroesPtr)
        {
            if (heroesPtr == null) return;

            QList<Hero> heroes = f.ResolveList(heroesPtr);

            for (int i = 0; i < heroes.Count; i++)
            {
                f.Destroy(heroes[i].Ref);
            }

            f.FreeList(heroesPtr);
        }

        private void ClearProjectiles(Frame f, QListPtr<HeroProjectile> projectilesPtr)
        {
            if (projectilesPtr == null) return;

            QList<HeroProjectile> projectiles = f.ResolveList(projectilesPtr);

            for (int i = 0; i < projectiles.Count; i++)
            {
                f.Destroy(projectiles[i].Ref);
            }

            f.FreeList(projectilesPtr);
        }

        private EntityRef SpawnBoard(Frame f, PlayerLink* player1, PlayerLink* player2, bool main)
        {
            EntityPrototype boardPrototype = f.FindAsset(f.RuntimeConfig.Board);
            EntityRef boardEntity = f.Create(boardPrototype);

            SetupBoard(f, boardEntity, player1, player2, main);
            DisactiveEntity(f, boardEntity);

            return boardEntity;
        }

        private EntityRef SpawnBoard(Frame f, PlayerLink* player1, RoundInfo roundInfo)
        {
            EntityPrototype boardPrototype = f.FindAsset(f.RuntimeConfig.Board);
            EntityRef boardEntity = f.Create(boardPrototype);

            SetupBoard(f, boardEntity, player1, roundInfo);
            DisactiveEntity(f, boardEntity);

            return boardEntity;
        }

        private void SpawnHeroes(Frame f, Board* board)
        {
            QList<Hero> heroes1 = f.ResolveList(board->Heroes1);
            QList<Hero> heroes2 = f.ResolveList(board->Heroes2);

            for (int i = 0; i < heroes1.Count; i++)
            {
                Hero hero = heroes1[i];
                if (hero.ID < 0) continue;

                hero = SpawnHero(f, hero);
                hero.TeamNumber = 1;
                heroes1[i] = hero;
                SetHeroPosition(f, hero);
                DisactiveEntity(f, hero.Ref);
            }

            for (int i = 0; i < heroes2.Count; i++)
            {
                Hero hero = heroes2[i];
                if (hero.ID < 0) continue;

                hero = SpawnHero(f, hero);
                hero.TeamNumber = 2;
                heroes2[i] = hero;
                SetHeroPosition(f, hero, first: false);
                DisactiveEntity(f, hero.Ref);
            }
        }

        private Hero SpawnHero(Frame f, Hero hero)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            EntityRef heroEntity = f.Create(gameConfig.GetHeroPrototype(f, hero.ID));
            hero.Ref = heroEntity;
            return hero;
        }

        private void SetHeroPosition(Frame f, Hero hero, bool first = true)
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

        private void SetupBoard(Frame f, EntityRef boardEntity, PlayerLink* player1, PlayerLink* player2, bool main)
        {
            Board* board = f.Unsafe.GetPointer<Board>(boardEntity);

            board->Ref = boardEntity;
            board->FightingHeroesMap = f.AllocateList<Hero>(GameConfig.BoardSize * GameConfig.BoardSize);
            board->HeroProjectiles = f.AllocateList<HeroProjectile>();

            board->Heroes1 = SetupHeroes(f, player1, board->Heroes1);
            board->Heroes2 = SetupHeroes(f, player2, board->Heroes2);

            board->Player1 = *player1;
            board->Player2 = main ? *player2 : default;

            QList<Board> boards = f.ResolveList(f.Global->Boards);
            boards.Add(*board);
        }

        private void SetupBoard(Frame f, EntityRef boardEntity, PlayerLink* player1, RoundInfo roundInfo)
        {
            Board* board = f.Unsafe.GetPointer<Board>(boardEntity);

            board->Ref = boardEntity;
            board->FightingHeroesMap = f.AllocateList<Hero>(GameConfig.BoardSize * GameConfig.BoardSize);
            board->HeroProjectiles = f.AllocateList<HeroProjectile>();

            board->Heroes1 = SetupHeroes(f, player1, board->Heroes1);
            board->Heroes2 = SetupHeroes(f, roundInfo, board->Heroes2);

            board->Player1 = *player1;
            board->Player2 = default;

            QList<Board> boards = f.ResolveList(f.Global->Boards);
            boards.Add(*board);
        }

        private QListPtr<Hero> SetupHeroes(Frame f, PlayerLink* player, QListPtr<Hero> heroes)
        {
            if (player->Ref == default)
            {
                return heroes;
            }

            QList<int> playerHeroesID = f.ResolveList(player->Info.Board.HeroesID);
            QList<int> playerHeroesLevel = f.ResolveList(player->Info.Board.HeroesLevel);
            heroes = f.AllocateList<Hero>();
            QList<Hero> playerHeroes = f.ResolveList(heroes);

            for (int i = 0; i < playerHeroesID.Count; i++)
            {
                Hero hero = new()
                {
                    ID = playerHeroesID[i],
                    Level = playerHeroesLevel[i],
                    DefaultPosition = BoardPosition.GetTilePosition(f, i % GameConfig.BoardSize, i / GameConfig.BoardSize)
                };

                playerHeroes.Add(hero);
            }

            return heroes;
        }

        private QListPtr<Hero> SetupHeroes(Frame f, RoundInfo roundInfo, QListPtr<Hero> heroes)
        {
            if (roundInfo == null)
            {
                return heroes;
            }

            heroes = f.AllocateList<Hero>();
            QList<Hero> playerHeroes = f.ResolveList(heroes);

            for (int i = 0; i < roundInfo.PVEBoard.Count; i++)
            {
                for (int j = 0; j < roundInfo.PVEBoard[i].Cells.Count; j++)
                {
                    Hero hero = new()
                    {
                        ID = roundInfo.PVEBoard[i].Cells[^(j + 1)],
                        Level = 0,
                        DefaultPosition = BoardPosition.GetTilePosition(f, j, i),
                    };

                    playerHeroes.Add(hero);
                }
            }

            return heroes;
        }
    }
}
