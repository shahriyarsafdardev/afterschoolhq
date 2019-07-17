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
        private const string AccountSID = "ACb88ed7b072661362238f3b6f8224d070";
        private const string AuthToken = "0583dc2ccc604d5579c3baf252cc1a93";
        private const string TwilioPhoneNumber = "+13176444332";

        private static void InitializeTwilioClientService()
        {
           TwilioClient.Init(AccountSID, AuthToken);
        }

        public static MessageResource SendSMS(string phoneNumber, string body, SystemMessages msgs)
        {
            MessageResource resource = null;

            try
            {
                InitializeTwilioClientService();

                resource = MessageResource.Create(
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

            return resource;
        }
    }
}
