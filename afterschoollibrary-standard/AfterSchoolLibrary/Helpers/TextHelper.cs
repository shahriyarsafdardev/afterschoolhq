using BasicLibrary;

namespace AfterSchoolLibrary.Helpers
{
    public class TextHelper
    {
        public static void SendCheckInCheckOutRevokeAccessText(string userPhone, string masterRegistrantDisplayName, string masterRegistrantFirstName)
        {
            var msgs = new SystemMessages();

            var smsBody = $"This is an automated message from AfterSchool HQ. " +
                                           $"You are no longer an authorized person for {masterRegistrantDisplayName}. " +
                                           $"If you believe this is an error,  please contact {masterRegistrantFirstName}’s parent or guardian.";

            TwilioHelper.SendSMS(
                userPhone,
                smsBody,
                null,
                null,
                msgs
            );
        }
    }
}
