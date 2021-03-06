using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Twilio.TwiML;
using Twilio.Types;
using Twilio.Rest.Lookups.V1;
using Twilio.Rest.Api.V2010.Account;
using Twilio;
using System.Collections.Generic;
using System.Text;

namespace godbot.Controllers
{
    [Route("api/[controller]")]
    public class GodBotController : Controller
    {
        public static Client client;

        static GodBotController()
        {
            TwilioClient.Init(Secrets.TwilioAccountSid, Secrets.TwilioAuthToken);
        }

        private static TwilioGameManager currentGame;
        private static bool currentlyAwaitingOtherPlayer;
        private static PhoneNumberResource requestingPlayer;
        private static PhoneNumberResource awaitingOtherPlayer;

        public static async Task StartClient()
        {
            client = new Client(Secrets.Token);
            HttpClient httpClient = new HttpClient();
            Uri uri = new Uri($"{Client.BaseUrl}rtm.start?token={Secrets.Token}");
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            JObject responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            await client.Connect(new Uri((string)responseObject["url"]));
        }

        [HttpGet]
        [Route("ping")]
        public JObject Ping()
        {
            JObject o = new JObject();
            if (currentGame != null)
            {
                o["status"] = "current_game";
            }
            else
            {
                o["status"] = "no_game";
            }
            return o;
        }

        [HttpPost]
        public async Task Index()
        {
            string from = Request.Form["From"];
            string body = Request.Form["Body"].ToString().ToLower();
            if (currentGame != null)
            {
                if (from != currentGame.RedPlayerNumber && from != currentGame.BluePlayerNumber)
                {
                    await HelperMethods.SendSms(from, "A game is already in progress");
                    return;
                }
                if (!currentGame.CanDraw)
                {
                    if (from != currentGame.OutputChannel)
                    {
                        await HelperMethods.SendSms(from, "It's not your turn yet.");
                        return;
                    }
                }
                else
                {
                    if (body.Trim() != "draw")
                    {
                        if (from != currentGame.OutputChannel)
                        {
                            await HelperMethods.SendSms(from, "It's not your turn yet.");
                            return;
                        }
                    }
                    else
                    {
                        await SendGameInstructions(currentGame.AttemptDraw(from));
                        if (currentGame.GameIsOver)
                        {
                            currentGame = null;
                        }
                        return;
                    }
                }
                if (currentGame.WaitingForDieResponse)
                {
                    if (body == "yes")
                    {
                        currentGame.WaitingForDieResponse = false;
                        currentGame.PlayersHaveDie = true;
                    }
                    else if (body == "no")
                    {
                        currentGame.WaitingForDieResponse = false;
                        currentGame.PlayersHaveDie = false;
                    }
                    await SendGameInstructions(currentGame.StartYear());
                }
                else if (currentGame.WaitingForDieRollResponse)
                {
                    await SendGameInstructions(currentGame.ProcessDieRoll(body));
                }
                else if (currentGame.WaitingForRoundResponse)
                {
                    var i = currentGame.ContinueRound(body.ToUpper());
                    if (!currentGame.GameIsOver)
                    {
                        await SendGameInstructions(i);
                    }

                    if (currentGame.GameIsOver)
                    {
                        currentGame = null;
                    }
                }
            }
            if (currentlyAwaitingOtherPlayer)
            {
                if (from == awaitingOtherPlayer.PhoneNumber.ToString())
                {
                    if (body == "yes" || body == "y")
                    {
                        await HelperMethods.SendSms(requestingPlayer.PhoneNumber.ToString(), "Your request has been accepted. Starting game.");
                        currentGame = new TwilioGameManager(requestingPlayer, awaitingOtherPlayer);
                        currentGame.WaitingForDieResponse = false;
                        currentGame.PlayersHaveDie = false;
                        await SendGameInstructions(currentGame.StartYear());
                        ResetPlayers();
                    }
                    else
                    {
                        await HelperMethods.SendSms(awaitingOtherPlayer.PhoneNumber.ToString(), "OK, rejecting request.");
                        await HelperMethods.SendSms(requestingPlayer.PhoneNumber.ToString(), "The other player has rejected the request.");
                        ResetPlayers();
                    }
                }
                else
                {
                    await HelperMethods.SendSms(from, "Currently waiting for another player to join a game.");
                }
            }
            if (body.StartsWith("start game") && body.Length > 11)
            {
                PhoneNumber potentialNumber = new PhoneNumber(body.Substring(11));
                PhoneNumberResource otherResult = null;
                try
                {
                    otherResult = await PhoneNumberResource.FetchAsync(potentialNumber);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("");
                    await HelperMethods.SendSms(from, "That doesn't appear to be a valid number. Try again");
                    return;
                }

                PhoneNumberResource result = await PhoneNumberResource.FetchAsync(new PhoneNumber(from));
                requestingPlayer = result;
                awaitingOtherPlayer = otherResult;
                currentlyAwaitingOtherPlayer = true;
                await HelperMethods.SendSms(otherResult.PhoneNumber.ToString(), $"{result.NationalFormat} has sent you a game request. Text back yes or y to accept. Text back no or n to reject.");
            }
        }

        private void ResetPlayers()
        {
            currentlyAwaitingOtherPlayer = false;
            awaitingOtherPlayer = null;
            requestingPlayer = null;
        }

        public async Task SendGameInstructions(List<GameInstruction> instructions)
        {
            SmsMessage first = new SmsMessage(0);
            SmsMessage second = new SmsMessage(0);
            foreach (var i in instructions)
            {
                foreach (var recipient in i.Recipients)
                {
                    if (string.IsNullOrEmpty(first.Number))
                    {
                        first.Number = recipient;
                    }
                    else if (string.IsNullOrEmpty(second.Number))
                    {
                        if (first.Number != recipient)
                        {
                            second.Number = recipient;
                        }
                    }
                    if (recipient == first.Number)
                    {
                        await first.Append(i.Text, currentGame.GameYear);
                    }
                    else if (recipient == second.Number)
                    {
                        await second.Append(i.Text, currentGame.GameYear);
                    }
                }
            }
            await first.Finish(currentGame.GameYear);
            await second.Finish(currentGame.GameYear);
        }
    }

    class SmsMessage
    {
        public const int MaxTextMessageSize = 160;

        public string Number { get; set; }
        private int numberSent;
        private StringBuilder currentlyBuildingMessage;

        public SmsMessage(int number)
        {
            Number = null;
            numberSent = number;
            currentlyBuildingMessage = new StringBuilder();
        }

        public async Task Append(string message, int currentYear)
        {
            string[] splitMessage = message.Split(' ');
            for (int i = 0; i < splitMessage.Length; i++)
            {
                if ($"#{currentYear}-{numberSent + 1}: ".Length + currentlyBuildingMessage.Length + splitMessage[i].Length + 1 <= MaxTextMessageSize)
                {
                    currentlyBuildingMessage.Append($"{splitMessage[i]} ");
                }
                else
                {
                    numberSent++;
                    await HelperMethods.SendSms(Number, $"#{currentYear}-{numberSent}: {currentlyBuildingMessage.ToString().Trim()}");
                    currentlyBuildingMessage.Clear();
                    currentlyBuildingMessage.Append($"{splitMessage[i]} ");
                }
            }
        }

        public async Task Finish(int currentYear)
        {
            if (currentlyBuildingMessage.Length > 0)
            {
                if (!string.IsNullOrEmpty(Number))
                {
                    numberSent++;
                    await HelperMethods.SendSms(Number, $"#{currentYear}-{numberSent}: {currentlyBuildingMessage.ToString().Trim()}");
                }
            }
        }
    }
}
