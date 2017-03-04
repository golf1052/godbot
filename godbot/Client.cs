using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using golf1052.SlackAPI;
using golf1052.SlackAPI.Events;
using golf1052.SlackAPI.Objects;
using golf1052.SlackAPI.Other;
using godbot.Game;

namespace godbot
{
    public class Client
    {
        public const string BaseUrl = "https://slack.com/api/";
        public const string ChannelId = "G4DK350RK";
        public const string golf1052UserId = "U03FNCQL3";
        public const string golf1052Dm = "D0L4V7E68";

        static Client()
        {
        }

        private SlackCore slackCore;
        private List<SlackUser> slackUsers;
        private List<SlackChannel> slackChannels;
        ClientWebSocket webSocket;

        event EventHandler<SlackMessageEventArgs> MessageReceived;

        SlackGameManager currentGame;
        bool outputToChannel;
        bool debug;

        public Client(string accessToken)
        {
            webSocket = new ClientWebSocket();
            MessageReceived += Client_MessageReceived;
            slackCore = new SlackCore(accessToken);
            slackUsers = new List<SlackUser>();
            slackChannels = new List<SlackChannel>();
            outputToChannel = true;
            debug = true;
        }

        public async Task Connect(Uri uri)
        {
            await webSocket.ConnectAsync(uri, CancellationToken.None);
            slackUsers = await slackCore.UsersList();
            slackChannels = await slackCore.ChannelsList(1);
            await Receive();
        }

        public async Task Receive()
        {
            while (!webSocket.CloseStatus.HasValue)
            {
                MemoryStream stream = new MemoryStream();
                StreamReader reader = new StreamReader(stream);
                bool endOfData = false;
                while (!endOfData)
                {
                    byte[] buf = new byte[8192];
                    ArraySegment<byte> buffer = new ArraySegment<byte>(buf);
                    WebSocketReceiveResult response = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    stream.Write(buffer.Array, buffer.Offset, buffer.Count);
                    endOfData = response.EndOfMessage;
                }
                stream.Seek(0, SeekOrigin.Begin);
                while (!reader.EndOfStream)
                {
                    string read = reader.ReadLine();
                    JObject o = JObject.Parse(read);
                    if (o["type"] != null)
                    {
                        string messageType = (string)o["type"];
                        if (messageType == "message")
                        {
                            SlackMessageEventArgs newMessage = new SlackMessageEventArgs();
                            newMessage.Message = o;
                            MessageReceived(this, newMessage);
                        }
                    }
                }
            }
        }

        private async void Client_MessageReceived(object sender, SlackMessageEventArgs e)
        {
            string text = (string)e.Message["text"];
            string channel = (string)e.Message["channel"];
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(channel))
            {
                // subtype or message changed
            }
            if (debug && channel == ChannelId)
            {
                if (text.ToLower() == "godbot start game")
                {
                    if (currentGame == null)
                    {
                        string user = (string)e.Message["user"];
                        if (user == golf1052UserId)
                        {
                            currentGame = new SlackGameManager(user, golf1052Dm, user, golf1052Dm, debug, ChannelId);
                            await SendSlackMessage("Do you have a die that you can roll? (Answer yes or no)", channel);
                        }
                        else
                        {
                            await SendSlackMessage("Only Sanders can start games right now.", channel);
                        }
                    }
                    else
                    {
                        await SendSlackMessage("Cannot start game. Game already in progress.", channel);
                    }
                    return;
                }
            }
            if (text.ToLower() == "godbot ping")
            {
                await SendSlackMessage("pong", channel);
            }
            else if (text.ToLower() == "godbot help")
            {
                await SendSlackMessage("not implemented yet", channel);
            }
            else if (text.ToLower().StartsWith("gotbot"))
            {
                await SendSlackMessage("My name is godbot.", channel);
            }
            else if (text.ToLower() == "godbot stop game")
            {
                if (currentGame != null)
                {
                    currentGame = null;
                    await SendSlackMessage("Ended game", channel);
                }
            }
            else if (currentGame != null)
            {
                if (currentGame.WaitingForDieResponse)
                {
                    if (text.ToLower() == "yes")
                    {
                        currentGame.WaitingForDieResponse = false;
                        currentGame.PlayersHaveDie = true;
                    }
                    else if (text.ToLower() == "no")
                    {
                        currentGame.WaitingForDieResponse = false;
                        currentGame.PlayersHaveDie = false;
                    }
                    await SendGameInstructions(currentGame.StartYear());
                }
                else if (currentGame.WaitingForDieRollResponse)
                {
                    await SendGameInstructions(currentGame.ProcessDieRoll(text));
                }
                else if (currentGame.WaitingForRoundResponse)
                {
                    var i = currentGame.ContinueRound(text.ToUpper());
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
        }

        public T GetRandomFromList<T>(List<T> list)
        {
            Random random = new Random();
            return list[random.Next(list.Count)];
        }

        public async Task SendGameInstructions(List<GameInstruction> instructions)
        {
            foreach (var i in instructions)
            {
                await SendSlackMessage(i.Text, i.Channel);
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        public async Task SendSlackMessage(List<string> lines, string channel)
        {
            string message = string.Empty;
            for (int i = 0; i < lines.Count; i++)
            {
                if (i != lines.Count - 1)
                {
                    message += lines[i] += '\n';
                }
                else
                {
                    message += lines[i];
                }
            }
            await SendSlackMessage(message, channel);
        }

        public async Task SendSlackMessage(string message, string channel)
        {
            JObject o = new JObject();
            o["id"] = 1;
            o["type"] = "message";
            o["channel"] = channel;
            o["text"] = message;
            await SendMessage(o.ToString(Formatting.None));
        }
        
        public async Task SendTyping(string channel)
        {
            JObject o = new JObject();
            o["id"] = 1;
            o["type"] = "typing";
            o["channel"] = channel;
            await SendMessage(o.ToString(Formatting.None));
        }

        public async Task SendMessage(string message)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendApiCall(string endpoint)
        {
            Uri url = new Uri(BaseUrl + endpoint);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            JObject responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
