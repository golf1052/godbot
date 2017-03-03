using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public struct Tile
    {
        public string X { get; private set; }
        public int Y { get; private set; }
        public Constants.Sides Side { get; set; }
        public bool CanPlaceOn
        {
            get
            {
                return Side == Constants.Sides.None;
            }
        }

        public Tile(string x, int y)
        {
            X = x;
            Y = y;
            Side = Constants.Sides.None;
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
    }
}
