using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace godbot
{
    public static class HelperMethods
    {
        public static async Task SendSms(string to, string message)
        {
            await SendSms(to, Secrets.TwilioPhoneNumber, message);
        }

        public static async Task SendSms(string to, string from, string message)
        {
            await MessageResource.CreateAsync(
                to: new PhoneNumber(to),
                from: new PhoneNumber(from),
                body: message);
        }
    }
}
