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
                    board[i, j] = new Tile(new Coord(((char)currentLetter).ToString(), j + 1));
                }
            }
        }

        public Tile GetTile(string letter, int number)
        {
            int x = letter[0] - 65;
            int y = number - 1;
            return board[x, y];
        }

        public Tile GetTile(Coord coord)
        {
            int x = coord.X[0] - 65;
            int y = coord.Y - 1;
            return board[x, y];
        }

        /// <summary>
        /// Tries to get a tile
        /// </summary>
        /// <param name="x">x coord, 0 index</param>
        /// <param name="y">y coord, 0 index</param>
        /// <returns>null if coord does not exist, a tile if coord exists</returns>
        public Tile? TryGetTile(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return null;
            }
            else
            {
                return board[x + 1, y + 1];
            }
        }

        public List<Tile> GetGrid(Coord center, int size)
        {
            if (size % 2 != 1)
            {
                throw new Exception($"{nameof(size)} must be an odd number");
            }
            List<Tile> tiles = new List<Tile>();
            int startingCornerNumber = size - size / 2;
            for (int i = center.XAsIndex - startingCornerNumber; i < size; i++)
            {
                for (int j = center.YAsIndex - startingCornerNumber; j < size; j++)
                {
                    Tile? tile = TryGetTile(i, j);
                    if (tile != null)
                    {
                        tiles.Add(tile.Value);
                    }
                }
            }
            return tiles;
        }
    }
}
