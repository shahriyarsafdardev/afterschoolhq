using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class TransactionFactory
    {
        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = 13.95.ToDecimal(),
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase"
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, decimal amount, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase"
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
