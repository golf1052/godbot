using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace godbot.Game
{
    public class Team
    {
        public Constants.Teams TeamColor { get; private set; }
        public int Population { get; set; }
        public int PreviousPopulation { get; set; }
        public string UserId { get; private set; }
        public int Moves { get; set; }

        public Team(string userId, Constants.Teams teamColor)
        {
            TeamColor = teamColor;
            Population = 0;
            UserId = userId;
        }
    }
}
