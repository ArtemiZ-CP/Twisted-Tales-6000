using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static class BoardPosition
    {
        public static Vector2Int GetHeroCords(Hero hero)
        {
            return new Vector2Int(hero.TargetPositionX, hero.TargetPositionY);
        }

        public static FPVector3 GetHeroPosition(Frame f, Hero hero)
        {
            Vector2Int cords = GetHeroCords(hero);

            return GetTilePosition(f, cords);
        }

        public static bool TryGetHeroCords(Frame f, QList<Hero> heroes, Hero hero, out Vector2Int cords)
        {
            int index = heroes.ToList().IndexOf(hero);

            return TryConvertIndexToCords(f, index, out cords);
        }

        public static FPVector3 GetTilePosition(Frame f, Vector2Int cords)
        {
            return GetTilePosition(f, cords.x, cords.y);
        }

        public static FPVector3 GetTilePosition(Frame f, int x, int y)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            FP tileSize = FP.FromFloat_UNSAFE(gameConfig.TileSize);

            FPVector3 position = new FPVector3(x, 0, y) * tileSize;
            position -= tileSize * new FPVector3(GameConfig.BoardSize, 0, GameConfig.BoardSize) / 2;
            position += new FPVector3(tileSize, 0, tileSize) / 2;

            return position;
        }

        public static bool TryConvertIndexToCords(Frame f, int index, out Vector2Int cords)
        {
            cords = new Vector2Int(index % GameConfig.BoardSize, index / GameConfig.BoardSize);

            return cords.x >= 0 && cords.x < GameConfig.BoardSize && cords.y >= 0 && cords.y < GameConfig.BoardSize;
        }

        public static bool TryConvertCordsToIndex(Frame f, Vector2Int cords, out int index)
        {
            index = cords.x + cords.y * GameConfig.BoardSize;

            return index >= 0 && index < GameConfig.BoardSize * GameConfig.BoardSize;
        }
    }
}
