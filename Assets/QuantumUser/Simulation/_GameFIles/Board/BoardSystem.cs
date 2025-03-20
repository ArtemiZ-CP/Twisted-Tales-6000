using System.Collections.Generic;
using System.Linq;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BoardSystem : SystemSignalsOnly, ISignalMakeNewBoardPVP, ISignalMakeNewBoardPVE, ISignalClearBoards
    {
        public static List<Board> GetBoards(Frame f)
        {
            QList<Board> boards = f.ResolveList(f.Global->Boards);

            if (boards.Count != 0)
            {
                return boards.ToList();
            }

            foreach (var board in f.GetComponentIterator<Board>())
            {
                boards.Add(board.Component);
            }

            return boards.ToList();
        }

        public static Board GetBoard(Frame f, PlayerRef playerRef)
        {
            var boards = GetBoards(f);

            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].Player1.Ref == playerRef || boards[i].Player2.Ref == playerRef)
                {
                    return boards[i];
                }
            }

            return default;
        }

        public static List<EntityRef> GetBoardEntities(Frame f)
        {
            List<EntityRef> boardEntities = new();

            foreach (var board in f.GetComponentIterator<Board>())
            {
                boardEntities.Add(board.Entity);
            }

            return boardEntities;
        }

        public static Board* GetBoardPointer(Frame f, EntityRef entity)
        {
            return f.Unsafe.GetPointer<Board>(entity);
        }

        public static void DisactiveEntity(Frame f, EntityRef entity)
        {
            GetGameObject(f, entity)?.SetActive(false);
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
            List<EntityRef> boards = GetBoardEntities(f);

            for (int i = 0; i < boards.Count; i++)
            {
                Board* board = GetBoardPointer(f, boards[i]);
                ClearBoard(f, board);
            }

            boards.Clear();
        }

        private void ClearBoard(Frame f, Board* board)
        {
            ClearHeroes(f, board->HeroesID1);
            f.FreeList(board->HeroesID1);
            board->HeroesID1 = default;

            ClearHeroes(f, board->HeroesID2);
            f.FreeList(board->HeroesID2);
            board->HeroesID2 = default;

            ClearProjectiles(f, *board);
            f.FreeList(board->HeroProjectiles);
            board->HeroProjectiles = default;

            ClearFightingHeroes(f, board->FightingHeroesMap);
            f.FreeList(board->FightingHeroesMap);
            board->FightingHeroesMap = default;

            f.FreeList(board->GlobalEffects);
            board->GlobalEffects = default;

            f.Destroy(board->Ref);
        }

        private void ActiveBoard(Frame f, EntityRef boardEntity)
        {
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
        }

        private void ClearProjectiles(Frame f, Board board)
        {
            if (board.HeroProjectiles == null) return;

            QList<HeroProjectile> projectiles = f.ResolveList(board.HeroProjectiles);

            for (int i = 0; i < projectiles.Count; i++)
            {
                HeroProjectile heroProjectile = projectiles[i];
                Events.DisactiveEntity(f, board, heroProjectile.Ref);
            }
        }

        private void ClearFightingHeroes(Frame f, QListPtr<FightingHero> FightingHeroes)
        {
            if (FightingHeroes == null) return;

            QList<FightingHero> heroes = f.ResolveList(FightingHeroes);

            for (int i = 0; i < heroes.Count; i++)
            {
                FightingHero fightingHero = heroes[i];
                f.FreeList(fightingHero.Effects);
            }
        }

        private EntityRef SpawnBoard(Frame f, PlayerLink* player1, PlayerLink* player2, bool main)
        {
            EntityRef boardEntity = SpawnBoard(f);

            SetupBoard(f, boardEntity, player1, player2, main);

            return boardEntity;
        }

        private EntityRef SpawnBoard(Frame f, PlayerLink* player1, RoundInfo roundInfo)
        {
            EntityRef boardEntity = SpawnBoard(f);

            SetupBoard(f, boardEntity, player1, roundInfo);

            return boardEntity;
        }

        private EntityRef SpawnBoard(Frame f)
        {
            EntityRef boardEntity = f.Create();
            f.Add<Board>(boardEntity);

            return boardEntity;
        }

        private void SpawnHeroes(Frame f, Board* board)
        {
            QList<HeroEntity> heroes1 = f.ResolveList(board->HeroesID1);
            QList<HeroEntity> heroes2 = f.ResolveList(board->HeroesID2);

            for (int i = 0; i < heroes1.Count; i++)
            {
                Hero.Spawn(f, heroes1, board->Player1.Ref, i, first: true);
            }

            for (int i = 0; i < heroes2.Count; i++)
            {
                Hero.Spawn(f, heroes2, board->Player1.Ref, i, first: false);
            }
        }

        private void SetupBoard(Frame f, EntityRef boardEntity, PlayerLink* player1, PlayerLink* player2, bool main)
        {
            Board* board = GetBoardPointer(f, boardEntity);

            board->Ref = boardEntity;

            board->FightingHeroesMap = f.AllocateList<FightingHero>(GameConfig.BoardSize * GameConfig.BoardSize);
            board->HeroProjectiles = f.AllocateList<HeroProjectile>();
            board->GlobalEffects = f.AllocateList<GlobalEffectQnt>();

            board->HeroesID1 = Hero.SetupHeroes(f, player1, board->HeroesID1);
            board->HeroesID2 = Hero.SetupHeroes(f, player2, board->HeroesID2);

            board->Player1 = *player1;
            board->Player2 = main ? *player2 : default;
        }

        private void SetupBoard(Frame f, EntityRef boardEntity, PlayerLink* player1, RoundInfo roundInfo)
        {
            Board* board = GetBoardPointer(f, boardEntity);

            board->Ref = boardEntity;

            board->FightingHeroesMap = f.AllocateList<FightingHero>(GameConfig.BoardSize * GameConfig.BoardSize);
            board->HeroProjectiles = f.AllocateList<HeroProjectile>();
            board->GlobalEffects = f.AllocateList<GlobalEffectQnt>();

            board->HeroesID1 = Hero.SetupHeroes(f, player1, board->HeroesID1);
            board->HeroesID2 = Hero.SetupHeroes(f, roundInfo, board->HeroesID2);

            board->Player1 = *player1;
            board->Player2 = default;
        }
    }
}
