using System.Diagnostics;
using System.IO;
using godbot.Game;
using Microsoft.AspNetCore.Hosting;

namespace godbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Game.Game game = new Game.Game("", "");
            //game.TeamTurn = Constants.Teams.Red;
            //var settlements = game.AttemptPlaySettlements("A1", "A1", "C3");
            //var hitSettlements = game.PlayMissiles("E6", "A1", "A1");
            //Tests tests = new Tests();
            //tests.Run();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
