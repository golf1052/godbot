using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using godbot.Game;

namespace godbot
{
    public abstract class GameManager
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

        protected Game.Game game;
        protected bool debug;
        protected HalfOfRound partOfRound;
        protected SectionOfRound sectionOfRound;

        protected abstract string CurrentPlayerName { get; }
        public abstract string OutputChannel { get; }
        protected abstract string OtherChannel { get; }
        protected List<string> BothChannels
        {
            get
            {
                List<string> channels = new List<string>();
                channels.Add(OutputChannel);
                channels.Add(OtherChannel);
                return channels;
            }
        }
        public bool WaitingForDieResponse { get; set; }
        public bool PlayersHaveDie { get; set; }
        public bool WaitingForDieRollResponse { get; set; }
        protected int currentDieRoll;

        public bool WaitingForRoundResponse { get; set; }
        public bool GameIsOver { get; protected set; }
        public bool CanDraw
        {
            get
            {
                return game.CanDraw;
            }
        }
        public bool RedHasDrawn { get; private set; }
        public bool BlueHasDrawn { get; private set; }

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
                if (game.Year == 1)
                {
                    var redTeamI = new GameInstruction($"You are the Red team.", OutputChannel);
                    var blueTeamI = new GameInstruction($"You are the Blue team.", OtherChannel);
                    instructions.Add(redTeamI);
                    instructions.Add(blueTeamI);
                    var scoreGoalI = new GameInstruction($"You are trying to reach a score of {Constants.WinningScore / Constants.PopulationMultiplier}.", BothChannels);
                    instructions.Add(scoreGoalI);
                }
                var yearI = new GameInstruction($"It is year {game.Year}.", BothChannels);
                instructions.Add(yearI);
                currentDieRoll = number;
                game.CurrentPlayingTeam.Moves = currentDieRoll;
                if (debug)
                {
                    var i = new GameInstruction($"{CurrentPlayerName} rolls a {currentDieRoll}. They can take {currentDieRoll} resources.", OutputChannel);
                    instructions.Add(i);
                }
                else
                {
                    var playerI = new GameInstruction($"You can take {currentDieRoll} resources.", OutputChannel);
                    var otherPlayerI = new GameInstruction($"The enemy can take {currentDieRoll} resources.", OtherChannel);
                    instructions.Add(playerI);
                    instructions.Add(otherPlayerI);
                }
            }
            instructions.AddRange(OutputBeginningOfRound());
            return instructions;
        }

        public List<GameInstruction> OutputBeginningOfRound()
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (debug)
            {
                var mainI = new GameInstruction($"{CurrentPlayerName} place your settlements and then input coordinates. Use spaces or commas to deliminate. Enter none for no settlements.", OutputChannel);
                instructions.Add(mainI);
            }
            else
            {
                var mainI = new GameInstruction($"Input settlement coordinates. Enter none for no settlements.", OutputChannel);
                instructions.Add(mainI);
            }
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
                if (moves.Count > game.CurrentPlayingTeam.Moves)
                {
                    var i = new GameInstruction("Entered too many moves. Please enter your settlement coordinates again.", OutputChannel);
                    instructions.Add(i);
                    return instructions;
                }
                foreach (var move in moves)
                {
                    if (!Constants.ValidCoord(move))
                    {
                        var i = new GameInstruction("Invalid move. Please enter your settlement coordinates again.", OutputChannel);
                        instructions.Add(i);
                        return instructions;
                    }
                }
                game.CurrentPlayingTeam.Moves -= moves.Count;
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
            if (game.CurrentPlayingTeam.Moves > 0)
            {
                if (debug)
                {
                    var rocketsI = new GameInstruction($"{CurrentPlayerName}, you have {game.CurrentPlayingTeam.Moves} remaining. Play your missiles and then input coordinates. Use spaces or commas to deliminate. Enter none for no missiles.", OutputChannel);
                    instructions.Add(rocketsI);
                }
                else
                {
                    var rocketsI = new GameInstruction($"You have {game.CurrentPlayingTeam.Moves} remaining. Input missile coordinates. Enter none for no missiles.", OutputChannel);
                    instructions.Add(rocketsI);
                }
                sectionOfRound = SectionOfRound.Missiles;
            }
            else
            {
                if (debug)
                {
                    var noMovesLeftI = new GameInstruction($"{CurrentPlayerName}, you have no moves left.", OutputChannel);
                    instructions.Add(noMovesLeftI);
                }
                else
                {
                    var noMovesLeftI = new GameInstruction($"You have no moves left.", OutputChannel);
                    instructions.Add(noMovesLeftI);
                }
                instructions.AddRange(SwapHalf());
            }
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
                if (moves.Count > game.CurrentPlayingTeam.Moves)
                {
                    var i = new GameInstruction("Entered too many moves. Please enter your settlement coordinates again.", OutputChannel);
                    instructions.Add(i);
                    return instructions;
                }
                foreach (var move in moves)
                {
                    if (!Constants.ValidCoord(move))
                    {
                        var i = new GameInstruction("Invalid move. Please enter your missile coordinates again.", OutputChannel);
                        instructions.Add(i);
                        return instructions;
                    }
                }
                List<Settlement> destroyedSettlements = game.PlayMissiles(moves);
                if (destroyedSettlements.Count > 0)
                {
                    string teamDestroyed = string.Empty;
                    string enemyDestroyed = string.Empty;
                    int teamDestroyedNum = 0;
                    int enemyDestroyedNum = 0;
                    foreach (var settlement in destroyedSettlements)
                    {
                        if (settlement.OwningTeam == game.TeamTurn)
                        {
                            teamDestroyed += $" {settlement.Tile.Coord.ToString()}";
                            teamDestroyedNum++;
                        }
                        else
                        {
                            enemyDestroyed += $" {settlement.Tile.Coord.ToString()}";
                            enemyDestroyedNum++;
                        }
                    }
                    if (string.IsNullOrEmpty(teamDestroyed))
                    {
                        var teamI = new GameInstruction($"Your destroyed settlements:{teamDestroyed}. These settlements are lost.", OutputChannel);
                        instructions.Add(teamI);
                    }
                    if (debug)
                    {
                        var enemyI = new GameInstruction($"The following settlements for the enemy side were destroyed:{enemyDestroyed}. These settlements are lost.", OutputChannel);
                        instructions.Add(enemyI);
                    }
                    else
                    {
                        var numberDestroyedI = new GameInstruction($"You have destroyed {enemyDestroyedNum} enemy settlements.", OutputChannel);
                        var enemyI = new GameInstruction($"Your destroyed settlements:{enemyDestroyed}. These settlements are lost.", OtherChannel);
                        instructions.Add(numberDestroyedI);
                        instructions.Add(enemyI);
                    }
                }
                else
                {
                    var noneDestroyedI = new GameInstruction($"You have destroyed 0 enemy settlements.", OutputChannel);
                    int randWord = game.random.Next(0, 2);
                    var enemyNoneDestroyedI = new GameInstruction($"The enemy has launched a missile {(randWord == 0 ? "strike" : "attack")} but did not hit any of your settlements.", OtherChannel);
                    instructions.Add(noneDestroyedI);
                    instructions.Add(enemyNoneDestroyedI);
                }
                game.RemoveDestroyedSettlements(destroyedSettlements);
            }
            instructions.AddRange(SwapHalf());
            return instructions;
        }

        private List<GameInstruction> SwapHalf()
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (partOfRound == HalfOfRound.FirstHalf)
            {
                partOfRound = HalfOfRound.SecondHalf;
                sectionOfRound = SectionOfRound.Settlements;
                game.CurrentPlayingTeam.Moves = 0;
                var switchI = new GameInstruction($"Switching turns", BothChannels);
                instructions.Add(switchI);
                game.SwitchTeamTurn();
                game.CurrentPlayingTeam.Moves = 6 - currentDieRoll;
                if (currentDieRoll != 6)
                {
                    if (debug)
                    {
                        var i = new GameInstruction($"{CurrentPlayerName} can take {6 - currentDieRoll} resources.", OutputChannel);
                        instructions.Add(i);
                    }
                    else
                    {
                        var playerI = new GameInstruction($"You can take {6 - currentDieRoll} resources.", OutputChannel);
                        var otherPlayerI = new GameInstruction($"The enemy can take {6 - currentDieRoll} resources.", OtherChannel);
                        instructions.Add(playerI);
                        instructions.Add(otherPlayerI);
                    }
                    instructions.AddRange(OutputBeginningOfRound());
                }
                else
                {
                    if (debug)
                    {
                        var i = new GameInstruction($"{CurrentPlayerName} cannot play this turn.", OutputChannel);
                        instructions.Add(i);
                    }
                    else
                    {
                        var playerI = new GameInstruction($"You cannot play this turn.", OutputChannel);
                        var otherPlayerI = new GameInstruction($"The enemy cannot play this turn.", OtherChannel);
                        instructions.Add(playerI);
                        instructions.Add(otherPlayerI);
                    }
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
            game.CurrentPlayingTeam.Moves = 0;
            game.Advance();
            var pointAnnoucment = new GameInstruction($"Red: {game.GetTeamPopulationTotalString(Constants.Teams.Red)} ({game.GetTeamPopulationDifferenceString(Constants.Teams.Red)}). Blue: {game.GetTeamPopulationTotalString(Constants.Teams.Blue)} ({game.GetTeamPopulationDifferenceString(Constants.Teams.Blue)}).", BothChannels);
            instructions.Add(pointAnnoucment);
            if (game.RedTeam.Population >= Constants.WinningScore &&
                game.BlueTeam.Population >= Constants.WinningScore)
            {
                if (game.RedTeam.Population > game.BlueTeam.Population)
                {
                    var i = new GameInstruction("The Red side has won the game.", BothChannels);
                    instructions.Add(i);
                }
                else if (game.RedTeam.Population < game.BlueTeam.Population)
                {
                    var i = new GameInstruction("The Blue side has won the game.", BothChannels);
                    instructions.Add(i);
                }
                else
                {
                    var i = new GameInstruction("The game has ended in a draw.", BothChannels);
                    instructions.Add(i);
                }
                GameIsOver = true;
                return instructions;
            }
            else if (game.RedTeam.Population >= Constants.WinningScore)
            {
                var i = new GameInstruction("The Red side has won the game.", BothChannels);
                instructions.Add(i);
                GameIsOver = true;
                return instructions;
            }
            else if (game.BlueTeam.Population >= Constants.WinningScore)
            {
                var i = new GameInstruction("The Blue side has won the game.", BothChannels);
                instructions.Add(i);
                GameIsOver = true;
                return instructions;
            }
            if (game.ResolveSwap)
            {
                var swapAnnoucement = new GameInstruction("PLEASE SWITCH SIDES. MOVE POSITIONS. REMEMBER, G.O.D. IS ON YOUR SIDE.", BothChannels);
                instructions.Add(swapAnnoucement);
                if (!game.AlreadySwapped)
                {
                    var gameChangeAnnouncement = new GameInstruction("Points are now people. When you bomb a tile you kill all people living the affected settlements.", BothChannels);
                    instructions.Add(gameChangeAnnouncement);
                    var newScoreAnnouncement = new GameInstruction($"You are now trying to reach a population of {Constants.WinningScore}.", BothChannels);
                    instructions.Add(newScoreAnnouncement);
                    var drawI = new GameInstruction($"You can now initiate a draw by sending draw. Both sides must agree to draw on the same turn.", BothChannels);
                    instructions.Add(drawI);
                }
                game.ResolveSwap = false;
                game.AlreadySwapped = true;
                game.StartKilingPopulation();
            }
            instructions.AddRange(ResetDraw());
            partOfRound = HalfOfRound.FirstHalf;
            sectionOfRound = SectionOfRound.Settlements;
            instructions.AddRange(StartYear());
            return instructions;
        }

        public List<GameInstruction> AttemptDraw(string from)
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (from == game.RedTeam.UserId)
            {
                RedHasDrawn = true;
            }
            else if (from == game.BlueTeam.UserId)
            {
                BlueHasDrawn = true;
            }
            if (RedHasDrawn && BlueHasDrawn)
            {
                var drawGameI = new GameInstruction("Both players have agreed to a draw. The game has ended.", BothChannels);
                instructions.Add(drawGameI);
                GameIsOver = true;
            }
            else
            {
                var i = new GameInstruction("The other player also needs to agree to a draw.", from);
                instructions.Add(i);
            }
            return instructions;
        }

        private List<GameInstruction> ResetDraw()
        {
            List<GameInstruction> instructions = new List<GameInstruction>();
            if (RedHasDrawn)
            {
                var redI = new GameInstruction("Draw failed.", game.RedTeam.UserId);
                instructions.Add(redI);
            }
            else if (BlueHasDrawn)
            {
                var blueI = new GameInstruction("Draw failed.", game.BlueTeam.UserId);
                instructions.Add(blueI);
            }
            RedHasDrawn = false;
            BlueHasDrawn = false;
            return instructions;
        }
    }

    public struct GameInstruction
    {
        public string Text { get; private set; }
        public List<string> Recipients { get; private set; }

        public GameInstruction(string text, string recipient)
        {
            Recipients = new List<string>();
            Text = text;
            Recipients.Add(recipient);
        }

        public GameInstruction(string text, List<string> recipients)
        {
            Text = text;
            Recipients = recipients;
        }
    }
}
