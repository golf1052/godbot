using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public class Board
    {
        public const int BoardSize = 12;

        // X is letters A - L
        // Y is numbers 1 - 12
        public Tile[,] board;

        public Board()
        {
            board = new Tile[BoardSize, BoardSize];
            for (int i = 0; i < BoardSize; i++)
            {
                int currentLetter = 65 + i;
                for (int j = 0; j < BoardSize; j++)
                {
                    board[i, j] = new Tile(((char)currentLetter).ToString(), j + 1);
                }
            }
        }

        public Tile GetTile(string letter, int number)
        {
            int x = 65 - letter[0];
            int y = number - 1;
            return board[x, y];
        }
    }
}
