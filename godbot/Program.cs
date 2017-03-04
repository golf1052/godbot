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
            Board board = new Board();
            Debug.WriteLine(board.GetTile("A", 1));
            Debug.WriteLine(board.GetTile("L", 12));
            Debug.WriteLine(board.GetTile("E", 7));

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
