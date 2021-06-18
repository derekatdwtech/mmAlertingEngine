using System;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace tempaast.alerting.helpers
{
    public class TwilioHelper
    {
        ILogger _log;
        public TwilioHelper(ILogger log)
        {
            _log = log;
            TwilioClient.Init(Environment.GetEnvironmentVariable("twilioAccountSid"), Environment.GetEnvironmentVariable("twilioApiKey"));
        }

        public void SendMessage(string number, string message)
        {
            var sms = MessageResource.Create(
                            body: message,
                            from: new Twilio.Types.PhoneNumber(Environment.GetEnvironmentVariable("twilioPhoneNumber")),
                            to: new Twilio.Types.PhoneNumber(number.ToString())
                        );
        }

    }
}
