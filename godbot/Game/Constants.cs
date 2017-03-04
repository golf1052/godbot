using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public static class Constants
    {
        public const int FriendlyTerritoryPopulationPerTurn = 1000;
        public const int FriendlyTerritoryEnemyPopulationPerTurn = 0;
        public const int EnemyTerritoryPopulationPerTurn = 3000;
        public const int EnemyTerritoryEnemyPopulationPerTurn = 1000;
        public const int PopulationMultiplier = 1000;
        public const int MissileExplosionSize = 3;

        public enum Teams
        {
            None,
            Red,
            Blue
        }

        public static Coord StringToCoord(string str)
        {
            return new Coord(str[0].ToString(), int.Parse(str[1].ToString()));
        }
    }
}
