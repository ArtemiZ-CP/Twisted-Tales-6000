using UnityEngine;
using System.Collections.Generic;

namespace Quantum.Game
{
    public static class PathFinder
    {
        private static readonly int[] _dx = { 0, 0, 1, -1, 1, -1, 1, -1 };
        private static readonly int[] _dy = { 1, -1, 0, 0, 1, -1, -1, 1 };

        private static int _boardSize;

        public class Node
        {
            public Vector2Int Position;
            public Node Parent;

            public Node(Vector2Int position, Node parent = null)
            {
                Position = position;
                Parent = parent;
            }
        }

        public static bool TryFindPath(int[,] board, Vector2Int start, Vector2Int target, int heroRange, out Vector2Int nextPosition)
        {
            nextPosition = default;

            if (board == null || board.Length == 0)
            {
                return false;
            }

            if (board.GetLength(0) != board.GetLength(1))
            {
                throw new System.Exception("Board must be square");
            }

            _boardSize = board.GetLength(0);
            bool[,] visited = new bool[_boardSize, _boardSize];

            Queue<Node> queue = new();
            queue.Enqueue(new Node(start));

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();

                for (int i = 0; i < _dx.Length; i++)
                {
                    Vector2Int predictedPosition = current.Position + new Vector2Int(_dx[i], _dy[i]);

                    if (IsTargetPositionInRange(predictedPosition, target, heroRange - 1))
                    {
                        nextPosition = ConstructPath(new Node(predictedPosition, current));
                        return true;
                    }

                    if (IsInsideBoard(predictedPosition) &&
                        visited[predictedPosition.x, predictedPosition.y] == false &&
                        board[predictedPosition.x, predictedPosition.y] < 0)
                    {
                        visited[predictedPosition.x, predictedPosition.y] = true;
                        queue.Enqueue(new Node(predictedPosition, current));
                    }
                }
            }

            return false;
        }

        public static bool IsTargetPositionInRange(Vector2Int position, Vector2Int target, int range)
        {
            return Mathf.Abs(target.x - position.x) <= range && Mathf.Abs(target.y - position.y) <= range;
        }

        private static Vector2Int ConstructPath(Node node)
        {
            List<Vector2Int> path = new();

            while (node != null)
            {
                path.Add(node.Position);
                node = node.Parent;
            }

            return path.Count > 1 ? path[^2] : path[^1];
        }

        private static bool IsInsideBoard(Vector2Int position)
        {
            return position.x >= 0 && position.x < _boardSize && position.y >= 0 && position.y < _boardSize;
        }
    }
}