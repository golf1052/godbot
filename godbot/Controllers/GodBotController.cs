using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace godbot.Controllers
{
    [Route("api/[controller]")]
    public class GodBotController : Controller
    {
        public static Client client;

        public static async Task StartClient()
        {
            client = new Client(Secrets.Token);
            HttpClient httpClient = new HttpClient();
            Uri uri = new Uri($"{Client.BaseUrl}rtm.start?token={Secrets.Token}");
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            JObject responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            await client.Connect(new Uri((string)responseObject["url"]));
        }
    }
}
