using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public struct Coord
    {
        public string X { get; private set; }
        public int XAsNumber
        {
            get
            {
                return X[0] - 65;
            }
        }
        public int XAsIndex
        {
            get
            {
                return XAsNumber - 1;
            }
        }
        public int Y { get; private set; }
        public int YAsIndex
        {
            get
            {
                return Y - 1;
            }
        }

        public Coord(string x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }

        public static bool operator ==(Coord a, Coord b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Coord a, Coord b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return (obj is Coord) && Equals((Coord)obj);
        }

        public bool Equals(Coord coord)
        {
            return X == coord.X && Y == coord.Y;
        }
    }
}
