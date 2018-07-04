using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class TransactionFactory
    {
        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, TransactionType transactionType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = 13.95.ToDecimal(),
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                TransactionTypeId = transactionType,
                Description = "User purchase"
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, TransactionType transactionType, decimal amount, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                TransactionTypeId = transactionType,
                Description = "User purchase"
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, TransactionType transactionType, int ParticipationObjectId, ParticipationObjectType ParticipationObjectType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = 13.95.ToDecimal(),
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                TransactionTypeId = transactionType,
                Description = "User purchase",
                ParticipationObjectId = ParticipationObjectId,
                ParticipationObjectTypeId = ParticipationObjectType
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, TransactionType transactionType, int ParticipationObjectId, ParticipationObjectType ParticipationObjectType, int billSourceId, BillSourceType billSourceType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = 13.95.ToDecimal(),
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                TransactionTypeId = transactionType,
                Description = "User purchase",
                ParticipationObjectId = ParticipationObjectId,
                ParticipationObjectTypeId = ParticipationObjectType,
                BillSourceId = billSourceId,
                BillSourceTypeId = billSourceType
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
