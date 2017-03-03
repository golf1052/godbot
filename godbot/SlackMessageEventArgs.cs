using System;
using Newtonsoft.Json.Linq;

namespace godbot
{
    public class SlackMessageEventArgs : EventArgs
    {
        public JObject Message { get; set; }
    }
}
