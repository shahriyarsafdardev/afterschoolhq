using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AfterSchoolLibrary.Helpers;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Exceptions;

namespace AfterSchoolLibrary
{
    public class TwilioHelper
    {
        private const string AccountSID = "AC076fe5bae4597444448c56cea6038299";
        private const string AuthToken = "b5db6ddd85ed7f9bec66a3e96d5f6ccf";
        private const string TwilioPhoneNumber = "+12542756474";

        private static void InitializeTwilioClientService()
        {
           TwilioClient.Init(AccountSID, AuthToken);
        }

        public static SystemMessages SendSMS(string phoneNumber, string body)
        {
            var msgs = new SystemMessages();

            try
            {
                InitializeTwilioClientService();

                MessageResource.Create(
                    body: body,
                    from: new Twilio.Types.PhoneNumber(TwilioPhoneNumber),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );
            }
            catch (TwilioException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error sending sms", MessageType.UserError);
            }

            return msgs;
        }
    }
}
