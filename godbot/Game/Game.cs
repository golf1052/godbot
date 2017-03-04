using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public class Game
    {
        public Board board;
        private Random random;
        private List<Settlement> settlements;
        private Team redTeam;
        private Team blueTeam;
        public int Year { get; private set; }
        private int SwapYear { get; set; }
        public Constants.Teams TeamTurn { get; set; }
        private Team CurrentPlayingTeam
        {
            get
            {
                if (TeamTurn == Constants.Teams.Red)
                {
                    return redTeam;
                }
                else if (TeamTurn == Constants.Teams.Blue)
                {
                    return blueTeam;
                }
                else
                {
                    return null;
                }
            }
        }
        public bool KillPopulation { get; private set; }

        public Game(string redTeamUserId, string blueTeamUserId)
        {
            board = new Board();
            random = new Random();
            settlements = new List<Settlement>();
            redTeam = new Team(redTeamUserId, Constants.Teams.Red);
            blueTeam = new Team(blueTeamUserId, Constants.Teams.Blue);
            Year = 0;
            SwapYear = 0;
            KillPopulation = false;
        }

        public void RunYear()
        {
            // roll die
            int die = RollDie();
            // CurrentPlayingTeam moves = die
            // Player inputs settlement moves
            // Player inputs missile moves
            SwitchTeamTurn();
            // CurrentPlayingTeam moves = 6 - die
            // Player inputs settlement moves
            // Player inputs missile moves
            // Advance game
            Advance();
        }

        private void Advance()
        {
            if (KillPopulation)
            {
                redTeam.Population = 0;
                blueTeam.Population = 0;
            }

            foreach (Settlement settlement in settlements)
            {
                settlement.Advance();
                if (settlement.OwningTeam == Constants.Teams.Red)
                {
                    if (KillPopulation)
                    {
                        redTeam.Population += settlement.TeamPopulation;
                        blueTeam.Population += settlement.EnemyPopulation;
                    }
                    else
                    {
                        redTeam.Population += settlement.TeamPopulationPerTurn;
                        blueTeam.Population += settlement.EnemyPopulationPerTurn;
                    }
                }
                else if (settlement.OwningTeam == Constants.Teams.Blue)
                {
                    if (KillPopulation)
                    {
                        blueTeam.Population += settlement.TeamPopulation;
                        redTeam.Population += settlement.EnemyPopulation;
                    }
                    else
                    {
                        blueTeam.Population += settlement.TeamPopulationPerTurn;
                        redTeam.Population += settlement.EnemyPopulationPerTurn;
                    }
                }
            }
            Year++;
            if (Year >= SwapYear)
            {
                // initiate swap
                GetNewSwapYear();
            }
        }

        public List<string> AttemptPlaySettlements(params string[] coords)
        {
            List<string> failed = new List<string>();
            foreach (string stringCoord in coords)
            {
                Coord coord = Constants.StringToCoord(stringCoord);
                Tile tile = board.GetTile(coord);
                if (!tile.HasSettlement)
                {
                    Settlement settlement = new Settlement(CurrentPlayingTeam.TeamColor, tile);
                    settlements.Add(settlement);
                }
                else
                {
                    failed.Add(stringCoord);
                }
            }
            return failed;
        }

        public List<Settlement> PlayMissiles(params string[] coords)
        {
            List<Settlement> settlementsDestroyed = new List<Settlement>();
            List<Tile> allAffectedTiles = new List<Tile>();
            foreach (string stringCoord in coords)
            {
                Coord coord = Constants.StringToCoord(stringCoord);
                List<Tile> affectedTiles = board.GetGrid(coord, Constants.MissileExplosionSize);
                allAffectedTiles = AddToAffectedTiles(affectedTiles, allAffectedTiles);
            }
            foreach (Tile tile in allAffectedTiles)
            {
                settlementsDestroyed.Add(tile.Settlement);
            }
            return settlementsDestroyed;
        }

        public List<Tile> AddToAffectedTiles(List<Tile> newTiles, List<Tile> affectedTiles)
        {
            foreach (Tile tile in newTiles)
            {
                bool contains = false;
                foreach (Tile existingTile in affectedTiles)
                {
                    if (tile.Coord == existingTile.Coord)
                    {
                        contains = true;
                    }
                }
                if (!contains)
                {
                    affectedTiles.Add(tile);
                }
            }
            return affectedTiles;
        }

        public void SwitchTeamTurn()
        {
            if (TeamTurn == Constants.Teams.Red)
            {
                TeamTurn = Constants.Teams.Blue;
            }
            else if (TeamTurn == Constants.Teams.Blue)
            {
                TeamTurn = Constants.Teams.Red;
            }
        }

        public int RollDie()
        {
            return random.Next(1, 7);
        }

        public void GetNewSwapYear()
        {
            SwapYear = random.Next(15, 21) + Year;
            KillPopulation = true;
        }
    }
}
