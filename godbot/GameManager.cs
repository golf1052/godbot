﻿using System;
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
        protected abstract string OutputChannel { get; }
        public bool WaitingForDieResponse { get; set; }
        public bool PlayersHaveDie { get; set; }
        public bool WaitingForDieRollResponse { get; set; }
        protected int currentDieRoll;
        protected int currentPlayersMoves;

        public bool WaitingForRoundResponse { get; set; }
        public bool GameIsOver { get; protected set; }

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
                var yearI = new GameInstruction($"It is year {game.Year}.", OutputChannel);
                instructions.Add(yearI);
                currentDieRoll = number;
                currentPlayersMoves = currentDieRoll;
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
                if (moves.Count > currentPlayersMoves)
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
                if (moves.Count > currentPlayersMoves)
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
                currentPlayersMoves = 6 - currentDieRoll;
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
        public string Recipient { get; private set; }

        public GameInstruction(string text, string recipient)
        {
            Text = text;
            Recipient = recipient;
        }
    }
}