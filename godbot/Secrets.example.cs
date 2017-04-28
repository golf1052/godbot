namespace godbot
{
    public sealed class SecretsExample
    {
        /// <summary>
        /// Slack bot token
        /// </summary>
        public const string Token = "";

        /// <summary>
        /// Twilio account SID
        /// </summary>
        public const string TwilioAccountSid = "";

        /// <summary>
        /// Twilio auth token
        /// </summary>
        public const string TwilioAuthToken = "";

        /// <summary>
        /// Twilio phone number. Should be formatted +[country calling code][phone number]. Example +16175555555
        /// </summary>
        public const string TwilioPhoneNumber = "";

        /// <summary>
        /// Debug number. Number that gets messaged when an error occurs. Formatted the same as the Twilio phone number.
        /// </summary>
        public const string DebugNumber = "";
    }
}
