using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class StripeAccountFactory
    {
        public static StripeAccount Create(Account account, StripeVerificationStatus stripeVerificationStatus, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new StripeAccount()
            {
                AccountId = account.ID,
                VerificationStatusId = stripeVerificationStatus
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
