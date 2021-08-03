using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class PurchaseFactory
    {

        public static Purchase Create(Account payerAccount, decimal amount, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Purchase()
            {
                AmountPaid = amount,
                PurchaserAccountId = payerAccount.ID.Value,
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Purchase Create(Account payerAccount, decimal amount, int registrantId, int registrationId, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Purchase()
            {
                AmountPaid = amount,
                PurchaserAccountId = payerAccount.ID.Value,
                RegistrantId = registrantId,
                RegistrationId = registrationId
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
