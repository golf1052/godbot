using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace godbot.Game
{
    public class Tests
    {
        public Tests()
        {
        }

        public void Run()
        {
            Coord stc1 = Constants.StringToCoord("A1");
            Coord stc2 = Constants.StringToCoord("E10");

            bool tc1 = Constants.ValidCoord("A1");
            bool tc2 = Constants.ValidCoord("E10");
            bool tc3 = !Constants.ValidCoord("M1");
            bool tc4 = !Constants.ValidCoord("F13");
            bool tc5 = !Constants.ValidCoord("2A");

            Constants.Teams got1 = Constants.GetOppositeTeam(Constants.Teams.Red);
            Constants.Teams got2 = Constants.GetOppositeTeam(Constants.Teams.Blue);

            Tile t1 = new Tile(new Coord("A", 1));
            Tile t2 = new Tile(new Coord("L", 12));

            Settlement s1 = new Settlement(Constants.Teams.Red, t1);
            bool s1Enemy = !s1.InEnemyTerritory;
            Settlement s2 = new Settlement(Constants.Teams.Blue, t1);
            bool s2Enemy = s2.InEnemyTerritory;
            Settlement s3 = new Settlement(Constants.Teams.Red, t2);
            bool s3Enemy = s3.InEnemyTerritory;
            Settlement s4 = new Settlement(Constants.Teams.Blue, t2);
            bool s4Enemy = !s4.InEnemyTerritory;

            Board board = new Board();

            Tile bgt1 = board.GetTile("A", 1);
            Tile bgt2 = board.GetTile("E", 10);

            Tile bgt3 = board.GetTile(bgt1.Coord);
            Tile bgt4 = board.GetTile(bgt2.Coord);

            Tile tgt1 = board.TryGetTile(0, 0);
            bool tgtc1 = tgt1.Coord == new Coord("A", 1);
            Tile tgt2 = board.TryGetTile(4, 9);
            bool tgtc2 = tgt2.Coord == new Coord("E", 10);
            bool tgt3 = board.TryGetTile(12, 12) == null;
            bool tgt4 = board.TryGetTile(-1, -1) == null;

            var bgg1 = board.GetGrid(stc1, Constants.MissileExplosionSize);
            bool bgg1s = bgg1.Count == 4;
            var bgg2 = board.GetGrid(stc2, Constants.MissileExplosionSize);
            bool bgg2s = bgg2.Count == 9;

            Game game = new Game(string.Empty, string.Empty);
            game.TeamTurn = Constants.Teams.Red;
            bool cy1 = game.Year == 1;
            var aps1 = game.AttemptPlaySettlements("A1");
            bool aps1c = aps1.Count == 0;
            bool aps1cp = game.board.GetTile("A", 1).HasSettlement;
            var pm1 = game.PlayMissiles("L12");
            bool pm1c = pm1.Count == 0;
            game.SwitchTeamTurn();
            bool ctt1 = game.TeamTurn == Constants.Teams.Blue;
            var aps2 = game.AttemptPlaySettlements("L12", "A1");
            bool aps2c = aps2.Count == 1;
            bool aps2cc = aps2[0] == "A1";
            bool aps2cp = game.board.GetTile("L", 12).HasSettlement;
            var pm2 = game.PlayMissiles("A1");
            bool pm2c = pm2.Count == 1;
            bool sc1 = pm2[0].Tile.Coord == new Coord("A", 1);
            bool st1 = pm2[0].OwningTeam == Constants.Teams.Red;
            //game.RemoveDestroyedSettlements(pm2);
            var at1 = game.board.GetTile("A", 1);
            bool ats1 = at1.HasSettlement == false;
            bool kp1 = !game.KillPopulation;
            game.Advance();
            bool cy2 = game.Year == 2;
            bool cp1 = game.RedTeam.Population == 1000;
            bool cp2 = game.BlueTeam.Population == 1000;
            game.RemoveDestroyedSettlements(game.PlayMissiles("A1"));
            game.SwitchTeamTurn();
            bool ctt2 = game.TeamTurn == Constants.Teams.Red;
            var f1 = game.AttemptPlaySettlements("C8");
            bool f1c = f1.Count == 0;
            game.Advance();
            bool cy3 = game.Year == 3;
            bool cp3 = game.RedTeam.Population == 4000;
            bool cp4 = game.BlueTeam.Population == 3000;
            game.KillPopulation = true;
            game.Advance();
            bool cy4 = game.Year == 4;
            bool cp5 = game.RedTeam.Population == 12000;
            bool cp6 = game.BlueTeam.Population == 7000;
            var pm3 = game.PlayMissiles("D9");
            game.RemoveDestroyedSettlements(pm3);
            game.Advance();
            bool cy5 = game.Year == 5;
            bool cp7 = game.RedTeam.Population == 4000;
            bool cp8 = game.BlueTeam.Population == 5000;
            Debug.WriteLine("");
        }
    }
}
