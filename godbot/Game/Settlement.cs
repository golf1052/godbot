using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public class Settlement
    {
        public Constants.Teams OwningTeam { get; private set; }
        public Tile Tile { get; private set; }
        public bool InEnemyTerritory
        {
            get
            {
                if (OwningTeam == Constants.Teams.Red)
                {
                    return Tile.Coord.Y > 6;
                }
                else if (OwningTeam == Constants.Teams.Blue)
                {
                    return Tile.Coord.Y < 7;
                }
                else
                {
                    return false;
                }
            }
        }
        public int TeamPopulation { get; private set; }
        public int EnemyPopulation { get; private set; }
        public int TeamPopulationPerTurn
        {
            get
            {
                if (!InEnemyTerritory)
                {
                    return Constants.FriendlyTerritoryPopulationPerTurn;
                }
                else
                {
                    return Constants.EnemyTerritoryPopulationPerTurn;
                }
            }
        }
        public int EnemyPopulationPerTurn
        {
            get
            {
                if (InEnemyTerritory)
                {
                    return Constants.EnemyTerritoryEnemyPopulationPerTurn;
                }
                else
                {
                    return Constants.FriendlyTerritoryEnemyPopulationPerTurn;
                }
            }
        }

        public Settlement(Constants.Teams team, Tile tile)
        {
            OwningTeam = team;
            Tile = tile;
            TeamPopulation = 0;
            EnemyPopulation = 0;
            Advance();
        }

        public void Advance()
        {
            TeamPopulation += TeamPopulationPerTurn;
            EnemyPopulation += EnemyPopulationPerTurn;
        }
    }
}
