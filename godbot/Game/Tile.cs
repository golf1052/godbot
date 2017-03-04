using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public struct Tile
    {
        public Coord Coord { get; private set; }
        public Settlement Settlement { get; set; }
        public bool HasSettlement
        {
            get
            {
                return Settlement == null;
            }
        }

        public Tile(Coord coord)
        {
            Coord = coord;
            Settlement = null;
        }

        public override string ToString()
        {
            return Coord.ToString();
        }
    }
}
