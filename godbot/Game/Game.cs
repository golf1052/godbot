using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public class Game
    {
        public Board board;
        public Random random;
        private List<Settlement> settlements;
        public Team RedTeam { get; private set; }
        public Team BlueTeam { get; private set; }
        public int Year { get; private set; }
        public bool ResolveSwap { get; set; }
        public bool AlreadySwapped { get; set; }
        private int SwapYear { get; set; }
        public Constants.Teams TeamTurn { get; set; }
        public Team CurrentPlayingTeam
        {
            get
            {
                if (TeamTurn == Constants.Teams.Red)
                {
                    return RedTeam;
                }
                else if (TeamTurn == Constants.Teams.Blue)
                {
                    return BlueTeam;
                }
                else
                {
                    return null;
                }
            }
        }
        public bool KillPopulation { get; set; }

        public Game(string redTeamUserId, string blueTeamUserId)
        {
            board = new Board();
            random = new Random();
            settlements = new List<Settlement>();
            RedTeam = new Team(redTeamUserId, Constants.Teams.Red);
            BlueTeam = new Team(blueTeamUserId, Constants.Teams.Blue);
            Year = 0;
            ResolveSwap = false;
            AlreadySwapped = false;
            SwapYear = 0;
            GetNewSwapYear();
            Year = 1;
            KillPopulation = false;
        }

        public void Advance()
        {
            RedTeam.PreviousPopulation = RedTeam.Population;
            BlueTeam.PreviousPopulation = BlueTeam.Population;

            if (KillPopulation)
            {
                RedTeam.Population = 0;
                BlueTeam.Population = 0;
            }

            foreach (Settlement settlement in settlements)
            {
                settlement.Advance();
                if (settlement.OwningTeam == Constants.Teams.Red)
                {
                    if (KillPopulation)
                    {
                        RedTeam.Population += settlement.TeamPopulation;
                        BlueTeam.Population += settlement.EnemyPopulation;
                    }
                    else
                    {
                        RedTeam.Population += settlement.TeamPopulationPerTurn;
                        BlueTeam.Population += settlement.EnemyPopulationPerTurn;
                    }
                }
                else if (settlement.OwningTeam == Constants.Teams.Blue)
                {
                    if (KillPopulation)
                    {
                        BlueTeam.Population += settlement.TeamPopulation;
                        RedTeam.Population += settlement.EnemyPopulation;
                    }
                    else
                    {
                        BlueTeam.Population += settlement.TeamPopulationPerTurn;
                        RedTeam.Population += settlement.EnemyPopulationPerTurn;
                    }
                }
            }
            Year++;
            if (Year >= SwapYear)
            {
                ResolveSwap = true;
                GetNewSwapYear();
            }
        }

        public List<string> AttemptPlaySettlements(params string[] coords)
        {
            return AttemptPlaySettlements(new List<string>(coords));
        }

        public List<string> AttemptPlaySettlements(List<string> coords)
        {
            List<string> failed = new List<string>();
            foreach (string stringCoord in coords)
            {
                Coord coord = Constants.StringToCoord(stringCoord);
                Tile tile = board.GetTile(coord);
                if (!tile.HasSettlement)
                {
                    Settlement settlement = new Settlement(CurrentPlayingTeam.TeamColor, tile);
                    tile.Settlement = settlement;
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
            return PlayMissiles(new List<string>(coords));
        }

        public List<Settlement> PlayMissiles(List<string> coords)
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
                if (tile.HasSettlement)
                {
                    settlementsDestroyed.Add(tile.Settlement);
                }
            }
            return settlementsDestroyed;
        }

        public void RemoveDestroyedSettlements(List<Settlement> destroyedSettlements)
        {
            foreach (Settlement settlement in destroyedSettlements)
            {
                Tile tile = settlement.Tile;
                tile.Settlement = null;
                settlements.Remove(settlement);
            }
        }

        private List<Tile> AddToAffectedTiles(List<Tile> newTiles, List<Tile> affectedTiles)
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
            SwapYear = random.Next(10, 16) + Year;
        }

        public void StartKilingPopulation()
        {
            KillPopulation = true;
        }

        public string GetTeamPopulationTotalString(Constants.Teams team)
        {
            if (!KillPopulation)
            {
                if (team == Constants.Teams.Red)
                {
                    return $"{RedTeam.Population / Constants.PopulationMultiplier} points";
                }
                else if (team == Constants.Teams.Blue)
                {
                    return $"{BlueTeam.Population / Constants.PopulationMultiplier} points";
                }
                else
                {
                    return "ERROR points";
                }
            }
            else
            {
                if (team == Constants.Teams.Red)
                {
                    return $"{RedTeam.Population} people";
                }
                else if (team == Constants.Teams.Blue)
                {
                    return $"{BlueTeam.Population} people";
                }
                else
                {
                    return "ERROR people";
                }
            }
        }

        public string GetTeamPopulationDifferenceString(Constants.Teams team)
        {
            int teamDifference = 0;
            if (team == Constants.Teams.Red)
            {
                teamDifference = RedTeam.Population - RedTeam.PreviousPopulation;
            }
            else if (team == Constants.Teams.Blue)
            {
                teamDifference = BlueTeam.Population - BlueTeam.PreviousPopulation;
            }

            if (!KillPopulation)
            {
                if (teamDifference >= 0)
                {
                    return $"gained {teamDifference / Constants.PopulationMultiplier} points";
                }
                else
                {
                    return $"lost {teamDifference / Constants.PopulationMultiplier} points";
                }
            }
            else
            {
                if (teamDifference >= 0)
                {
                    return $"gained {teamDifference} people";
                }
                else
                {
                    return $"lost {teamDifference} people";
                }
            }
        }
    }
}
