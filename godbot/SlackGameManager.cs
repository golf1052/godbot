using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using godbot.Game;

namespace godbot
{
    public class SlackGameManager : GameManager
    {
        public string RedPlayerDm { get; private set; }
        public string BluePlayerDm { get; private set; }
        private string debugChannel;
        
        protected override string OutputChannel
        {
            get
            {
                if (debug)
                {
                    return debugChannel;
                }
                else
                {
                    if (game.TeamTurn == Game.Constants.Teams.Red)
                    {
                        return RedPlayerDm;
                    }
                    else if (game.TeamTurn == Game.Constants.Teams.Blue)
                    {
                        return BluePlayerDm;
                    }
                    else
                    {
                        return debugChannel;
                    }
                }
            }
        }
        
        protected override string CurrentPlayerName
        {
            get
            {
                if (debug)
                {
                    if (game.TeamTurn == Game.Constants.Teams.Red)
                    {
                        return "Red Team";
                    }
                    else if (game.TeamTurn == Game.Constants.Teams.Blue)
                    {
                        return "Blue Team";
                    }
                    else
                    {
                        return "ERROR";
                    }
                }
                else
                {
                    if (game.TeamTurn == Game.Constants.Teams.Red)
                    {
                        return "Red";
                    }
                    else if (game.TeamTurn == Game.Constants.Teams.Blue)
                    {
                        return "Blue";
                    }
                    else
                    {
                        return "ERROR";
                    }
                }
            }
        }

        public SlackGameManager(string redPlayerId, string redPlayerDm, string bluePlayerId, string bluePlayerDm, bool debug = false, string debugChannel = null)
        {
            game = new Game.Game(redPlayerId, bluePlayerId);
            RedPlayerDm = redPlayerDm;
            BluePlayerDm = bluePlayerDm;
            this.debug = debug;
            this.debugChannel = debugChannel;
            WaitingForDieResponse = true;
            PlayersHaveDie = false;
            currentDieRoll = 0;
            currentPlayersMoves = 0;
            WaitingForDieRollResponse = false;
            WaitingForRoundResponse = false;
            partOfRound = HalfOfRound.FirstHalf;
            sectionOfRound = SectionOfRound.Settlements;
            GameIsOver = false;
            game.TeamTurn = Constants.Teams.Red;
        }
    }
}
