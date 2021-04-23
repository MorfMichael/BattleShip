using NBattleshipCodingContest.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleShip.Extensions
{
    public static class Extensions
    {
        public static bool IsNeighbour(this BoardIndex index, BoardIndex neighbour)
        {
            BoardIndex[] neighbours = new int[] { index - 1, index + 1, index - 10, index + 10 }.Where(t => t >= 0 && t <= 99 && (t/10 == index/10 || t%10 == index%10)).Select(t => new BoardIndex(t)).ToArray();
            return neighbours.Contains(neighbour);
        }

        public static BoardIndex[] GetNeighbours(this BoardIndex index, bool includeEdges = false)
        {
            int row = index / 10, column = index % 10;
            BoardIndex[] neighbours = new int[] { index - 1, index + 1, index - 10, index + 10 }.Where(t => t >= 0 && t <= 99 && (t / 10 == row || t % 10 == column)).Select(t => new BoardIndex(t)).ToArray();

            if (includeEdges)
            {
                BoardIndex[] edges = new int[] { index - 11, index - 9, index + 11, index + 9 }.Where(t => t >= 0 && t <= 99 && (t / 10 == row - 1 || t / 10 == row + 1 || t % 10 == column - 1 || t % 10 == column + 1)).Select(t => new BoardIndex(t)).ToArray();
                neighbours = neighbours.Concat(edges).ToArray();
            }

            return neighbours;
        }
    }
}
