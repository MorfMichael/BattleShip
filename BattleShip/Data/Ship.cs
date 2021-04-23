using BattleShip.Extensions;
using NBattleshipCodingContest.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleShip.Data
{
    public class Ship : List<BoardIndex>
    {
        public bool Horizontal => this.GroupBy(t => t.Row).Count() == 1;
        public bool Vertical => this.GroupBy(t => t.Column).Count() == 1;

        public bool IsNeighbour(BoardIndex index) => this.Any(x => x.IsNeighbour(index));
        public List<BoardIndex> GetIgnorables()
        {
            return this.SelectMany(t => t.GetNeighbours(true)).Distinct().ToList();
        }

        public BoardIndex[] GetRelevant()
        {
            if (!this.Any())
                return new BoardIndex[0];
            else if (this.Count == 1)
                return this[0].GetNeighbours();
            else
            {
                var result = new List<BoardIndex>();
                var sorted = this.OrderBy(t => (int)t);
                BoardIndex start = sorted.First(), end = sorted.Last();
                if (Horizontal)
                {
                    int left = start - 1, right = end + 1;
                    if (left >= 0 && left / 10 == start / 10)
                        result.Add(new BoardIndex(left));

                    if (right <= 99 && right / 10 == end / 10)
                        result.Add(new BoardIndex(right));
                }
                else
                {
                    int top = start - 10, bottom = end + 10;
                    if (top >= 0 && top % 10 == start % 10)
                        result.Add(new BoardIndex(top));

                    if (bottom <= 99 && bottom % 10 == end % 10)
                        result.Add(new BoardIndex(bottom));
                }

                return result.ToArray();
            }
        }
    }
}
