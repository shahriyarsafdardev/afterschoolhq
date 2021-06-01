using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class PayoutFactory
    {
        public static Payout Create(Account Account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Payout()
            {
                AccountId= Account.ID.Value,
                Amount = 100,
                PayoutStatusId = PayoutStatus.Requested,
                PaymentTypeId = PaymentType.CreditCard,
                DateRequested = DateTime.Now.AddDays(-5),
                RequestedBy = 1,
                ExternalPayoutId = "Test payout"
            };

            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
