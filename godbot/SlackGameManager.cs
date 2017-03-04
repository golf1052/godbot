using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using godbot.Game;

namespace godbot
{
    public class SlackGameManager
    {
        public enum HalfOfRound
        {
            FirstHalf,
            SecondHalf
        }

        public enum SectionOfRound
        {
            Settlements,
            Missiles
        }
        private godbot.Game.Game game;
        public string RedPlayerDm { get; private set; }
        public string BluePlayerDm { get; private set; }
        private bool debug;
        private string debugChannel;
        private HalfOfRound partOfRound;
        private SectionOfRound sectionOfRound;
        private string OutputChannel
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
        
        private string CurrentPlayerName
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

        public bool WaitingForDieResponse { get; set; }
        public bool PlayersHaveDie { get; set; }
        public bool WaitingForDieRollResponse { get; set; }
        private int currentDieRoll;

        public bool WaitingForRoundResponse { get; set; }
        public bool GameIsOver { get; private set; }

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
            partOfRound = HalfOfRound.FirstHalf;
            sectionOfRound = SectionOfRound.Settlements;
            GameIsOver = false;
        }

        public List<GameInstruction> StartYear()
        {
            WaitingForRoundResponse = false;
            WaitingForDieRollResponse = true;
            partOfRound = HalfOfRound.FirstHalf;
            sectionOfRound = SectionOfRound.Settlements;
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (!PlayersHaveDie)
            {
                int roll = game.RollDie();
                return ProcessDieRoll(roll.ToString());
            }
            else
            {
                var i = new GameInstruction($"Please roll your die and then enter the die roll", OutputChannel);
                instructions.Add(i);
            }
            return instructions;
        }

        public List<GameInstruction> ProcessDieRoll(string text)
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            int number;
            int.TryParse(text, out number);
            if (number < 1 || number > 6)
            {
                var i = new GameInstruction("Invalid die roll, please enter a number between 1 and 6", OutputChannel);
                instructions.Add(i);
                return instructions;
            }
            else
            {
                WaitingForDieRollResponse = false;
                WaitingForRoundResponse = true;
                currentDieRoll = number;
                var i = new GameInstruction($"{CurrentPlayerName} rolls a {currentDieRoll}. They can take {currentDieRoll} resources.", OutputChannel);
                instructions.Add(i);
            }
            instructions.AddRange(OutputBeginningOfRound());
            return instructions;
        }

        public List<GameInstruction> OutputBeginningOfRound()
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            var mainI = new GameInstruction($"{CurrentPlayerName} place your settlements and then input coordinates. Use spaces or commas to deliminate. Enter none for no settlements.", OutputChannel);
            instructions.Add(mainI);
            return instructions;
        }

        public List<GameInstruction> ContinueRound(string text)
        {
            if (sectionOfRound == SectionOfRound.Settlements)
            {
                return ProcessSettlements(text);
            }
            else
            {
                return ProcessMissiles(text);
            }
        }

        public List<GameInstruction> ProcessSettlements(string text)
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (text != "NONE" && text != "NO")
            {
                List<string> moves = new List<string>();
                string[] commaSplit = text.Split(',');
                foreach (var fragment in commaSplit)
                {
                    string[] spaceSplit = fragment.Trim().Split(' ');
                    foreach (var coord in spaceSplit)
                    {
                        moves.Add(coord);
                    }
                }
                foreach (var move in moves)
                {
                    if (!Constants.ValidCoord(move))
                    {
                        var i = new GameInstruction("Invalid move. Please enter your settlement coordinates again.", OutputChannel);
                        instructions.Add(i);
                    }
                }
                List<string> failedMoves = game.AttemptPlaySettlements(moves);
                if (failedMoves.Count > 0)
                {
                    string failed = string.Empty;
                    foreach (var str in failedMoves)
                    {
                        failed += $" {str}";
                    }
                    var i = new GameInstruction($"The following moves failed:{failed}. These settlements are lost.", OutputChannel);
                    instructions.Add(i);
                }
            }
            var rocketsI = new GameInstruction($"{CurrentPlayerName} play your missiles and then input coordinates. Use spaces or commas to deliminate. Enter none for no missiles.", OutputChannel);
            instructions.Add(rocketsI);
            sectionOfRound = SectionOfRound.Missiles;
            return instructions;
        }

        public List<GameInstruction> ProcessMissiles(string text)
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (text != "NONE" && text != "NO")
            {
                List<string> moves = new List<string>();
                string[] commaSplit = text.Split(',');
                foreach (var fragment in commaSplit)
                {
                    string[] spaceSplit = fragment.Trim().Split(' ');
                    foreach (var coord in spaceSplit)
                    {
                        moves.Add(coord);
                    }
                }
                foreach (var move in moves)
                {
                    if (!Constants.ValidCoord(move))
                    {
                        var i = new GameInstruction("Invalid move. Please enter your missile coordinates again.", OutputChannel);
                        instructions.Add(i);
                    }
                }
                List<Settlement> destroyedSettlements = game.PlayMissiles(moves);
                if (destroyedSettlements.Count > 0)
                {
                    string teamDestroyed = string.Empty;
                    string enemyDestroyed = string.Empty;
                    foreach (var settlement in destroyedSettlements)
                    {
                        if (settlement.OwningTeam == game.TeamTurn)
                        {
                            teamDestroyed += $" {settlement.Tile.Coord.ToString()}";
                        }
                        else
                        {
                            enemyDestroyed += $" {settlement.Tile.Coord.ToString()}";
                        }
                    }
                    var teamI = new GameInstruction($"The following settlements for your side were destroyed:{teamDestroyed}. These settlements are lost.", OutputChannel);
                    instructions.Add(teamI);
                    if (debug)
                    {
                        var enemyI = new GameInstruction($"The following settlements for the enemy side were destroyed:{enemyDestroyed}. These settlements are lost.", OutputChannel);
                        instructions.Add(enemyI);
                    }
                }
                game.RemoveDestroyedSettlements(destroyedSettlements);
            }
            if (partOfRound == HalfOfRound.FirstHalf)
            {
                partOfRound = HalfOfRound.SecondHalf;
                sectionOfRound = SectionOfRound.Settlements;
                var switchI = new GameInstruction($"Switching turns...", OutputChannel);
                instructions.Add(switchI);
                game.SwitchTeamTurn();
                if (currentDieRoll != 6)
                {
                    var i = new GameInstruction($"{CurrentPlayerName} can take {6 - currentDieRoll} resources.", OutputChannel);
                    instructions.Add(i);
                    instructions.AddRange(OutputBeginningOfRound());
                }
                else
                {
                    var i = new GameInstruction($"{CurrentPlayerName} cannot play this turn.", OutputChannel);
                    instructions.Add(i);
                    instructions.AddRange(OutputEndOfRound());
                }
            }
            else
            {
                instructions.AddRange(OutputEndOfRound());
            }
            return instructions;
        }

        public List<GameInstruction> OutputEndOfRound()
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            game.Advance();
            var pointAnnoucment = new GameInstruction($"The Red side has {game.GetTeamPopulationTotalString(Constants.Teams.Red)}. The Blue side has {game.GetTeamPopulationTotalString(Constants.Teams.Blue)}", OutputChannel);
            instructions.Add(pointAnnoucment);
            if (game.RedTeam.Population >= Constants.WinningScore)
            {
                var i = new GameInstruction("The Red side has won the game.", OutputChannel);
                instructions.Add(i);
                GameIsOver = true;
                return instructions;
            }
            else if (game.BlueTeam.Population >= Constants.WinningScore)
            {
                var i = new GameInstruction("The Blue side has won the game.", OutputChannel);
                instructions.Add(i);
                GameIsOver = true;
                return instructions;
            }
            if (game.ResolveSwap)
            {
                var swapAnnoucement = new GameInstruction("PLEASE SWITCH SIDES. GET UP AND SWITCH. MOVE POSITIONS. GO GO GO.", OutputChannel);
                instructions.Add(swapAnnoucement);
                var gameChangeAnnoucement = new GameInstruction("Points are now people. When you bomb a tile you kill all people living the affected settlements. This decreases population of each side accordingly. God is on your side.", OutputChannel);
                instructions.Add(gameChangeAnnoucement);
                game.ResolveSwap = false;
            }
            partOfRound = HalfOfRound.FirstHalf;
            sectionOfRound = SectionOfRound.Settlements;
            instructions.AddRange(StartYear());
            return instructions;
        }
    }

    public struct GameInstruction
    {
        public string Text { get; private set; }
        public string Channel { get; private set; }

        public GameInstruction(string text, string channel)
        {
            Text = text;
            Channel = channel;
        }
    }
}
