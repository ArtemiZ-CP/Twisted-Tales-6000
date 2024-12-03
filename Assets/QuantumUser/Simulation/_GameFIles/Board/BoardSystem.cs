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
            ClearHeroes(f, board.HeroesID1);
            ClearHeroes(f, board.HeroesID2);
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

        private void ClearHeroes(Frame f, QListPtr<HeroEntity> heroesPtr)
        {
            if (heroesPtr == null) return;

            QList<HeroEntity> heroes = f.ResolveList(heroesPtr);

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
            QList<HeroEntity> heroesID1 = f.ResolveList(board->HeroesID1);
            QList<HeroEntity> heroesID2 = f.ResolveList(board->HeroesID2);

            for (int i = 0; i < heroesID1.Count; i++)
            {
                Hero.Spawn(f, heroesID1, i, 1, first: true);
            }

            for (int i = 0; i < heroesID2.Count; i++)
            {
                Hero.Spawn(f, heroesID2, i, 2, first: false);
            }
        }

        private void SetupBoard(Frame f, EntityRef boardEntity, PlayerLink* player1, PlayerLink* player2, bool main)
        {
            Board* board = f.Unsafe.GetPointer<Board>(boardEntity);

            board->Ref = boardEntity;
            board->FightingHeroesMap = f.AllocateList<FightingHero>(GameConfig.BoardSize * GameConfig.BoardSize);
            board->HeroProjectiles = f.AllocateList<HeroProjectile>();

            board->HeroesID1 = Hero.SetupHeroes(f, player1, board->HeroesID1);
            board->HeroesID2 = Hero.SetupHeroes(f, player2, board->HeroesID2);

            board->Player1 = *player1;
            board->Player2 = main ? *player2 : default;

            QList<Board> boards = f.ResolveList(f.Global->Boards);
            boards.Add(*board);
        }

        private void SetupBoard(Frame f, EntityRef boardEntity, PlayerLink* player1, RoundInfo roundInfo)
        {
            Board* board = f.Unsafe.GetPointer<Board>(boardEntity);

            board->Ref = boardEntity;
            board->FightingHeroesMap = f.AllocateList<FightingHero>(GameConfig.BoardSize * GameConfig.BoardSize);
            board->HeroProjectiles = f.AllocateList<HeroProjectile>();

            board->HeroesID1 = Hero.SetupHeroes(f, player1, board->HeroesID1);
            board->HeroesID2 = Hero.SetupHeroes(f, roundInfo, board->HeroesID2);

            board->Player1 = *player1;
            board->Player2 = default;

            QList<Board> boards = f.ResolveList(f.Global->Boards);
            boards.Add(*board);
        }
    }
}
