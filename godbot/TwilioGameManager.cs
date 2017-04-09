using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Lookups.V1;

namespace godbot
{
    public class TwilioGameManager : GameManager
    {
        public string RedPlayerNumber { get; private set; }
        public string RedPlayerDisplayNumber { get; private set; }
        public string BluePlayerNumber { get; private set; }
        public string BluePlayerDisplayNumber { get; private set; }

        protected override string CurrentPlayerName
        {
            get
            {
                if (game.TeamTurn == Game.Constants.Teams.Red)
                {
                    return RedPlayerDisplayNumber;
                }
                else if (game.TeamTurn == Game.Constants.Teams.Blue)
                {
                    return BluePlayerDisplayNumber;
                }
                else
                {
                    return "ERROR";
                }
            }
        }

        protected override string OutputChannel
        {
            get
            {
                if (game.TeamTurn == Game.Constants.Teams.Red)
                {
                    return RedPlayerNumber;
                }
                else if (game.TeamTurn == Game.Constants.Teams.Blue)
                {
                    return BluePlayerNumber;
                }
                else
                {
                    return Secrets.DebugNumber;
                }
            }
        }

        protected override string OtherChannel
        {
            get
            {
                if (game.TeamTurn == Game.Constants.Teams.Red)
                {
                    return BluePlayerNumber;
                }
                else if (game.TeamTurn == Game.Constants.Teams.Blue)
                {
                    return RedPlayerNumber;
                }
                else
                {
                    return Secrets.DebugNumber;
                }
            }
        }

        public TwilioGameManager(PhoneNumberResource redPlayerNumber, PhoneNumberResource bluePlayerNumber, bool debug = false)
        {
            game = new Game.Game(redPlayerNumber.PhoneNumber.ToString(), bluePlayerNumber.PhoneNumber.ToString());
            RedPlayerNumber = redPlayerNumber.PhoneNumber.ToString();
            RedPlayerDisplayNumber = redPlayerNumber.NationalFormat.ToString();
            BluePlayerNumber = bluePlayerNumber.PhoneNumber.ToString();
            BluePlayerDisplayNumber = bluePlayerNumber.NationalFormat.ToString();
            this.debug = debug;
            WaitingForDieResponse = true;
            PlayersHaveDie = false;
            currentDieRoll = 0;
            WaitingForDieRollResponse = false;
            WaitingForRoundResponse = false;
            partOfRound = HalfOfRound.FirstHalf;
            sectionOfRound = SectionOfRound.Settlements;
            GameIsOver = false;
            game.TeamTurn = Game.Constants.Teams.Red;
        }
    }
}
