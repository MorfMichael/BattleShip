using NBattleshipCodingContest.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleShip.Data
{
    public class Square
    {
        public Square(SquareContent content, int index)
        {
            Content = content;
            Index = new BoardIndex(index);
        }

        public BoardIndex Index { get; set; }

        public SquareContent Content { get; set; }

        public int Rank { get; set; }


    }
}
