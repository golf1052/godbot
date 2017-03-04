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
        public const int WinningScore = 5000 * PopulationMultiplier;

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

        public static bool ValidCoord(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length == 0 || str.Length > 3)
            {
                return false;
            }
            char lettter = str[0];
            string numberPortion = str.Substring(1);
            if (lettter < 65 || lettter > 76)
            {
                return false;
            }
            int number;
            int.TryParse(numberPortion, out number);
            if (number == 0)
            {
                return false;
            }
            if (number < 1 || number > 12)
            {
                return false;
            }
            return true;
        }

        public static Teams GetOppositeTeam(Teams team)
        {
            if (team == Teams.Red)
            {
                return Teams.Blue;
            }
            else if (team == Teams.Blue)
            {
                return Teams.Red;
            }
            else
            {
                return Teams.None;
            }
        }
    }
}
