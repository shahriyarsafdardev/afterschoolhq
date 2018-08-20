using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public class TransactionService
    {
        public static Transaction CreateNew(Account fromAccount, Account toAccount, CheckoutObject checkoutObject, int? purchaseId, TransactionType transactionType, string stripeChargeOrRefundId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return CreateNew(fromAccount, toAccount, checkoutObject.RegistrationFee, checkoutObject.ParticipationObject.Name, purchaseId, transactionType, stripeChargeOrRefundId, loggedInUser, msgs, dc);
        }

        public static Transaction CreateNew(Account fromAccount, Account toAccount, decimal? amount, string activityName, int? purchaseId, TransactionType transactionType, string stripeChargeOrRefundId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = toAccount.ID.Value,
                Description = $"{activityName} registration",
                PayerAccountId = fromAccount.ID,
                PaymentTypeId = PaymentType.CreditCard,
                PurchaseId = purchaseId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                PlatformFlatFee = toAccount.GetPlatformFlatFee(msgs, dc),
                PlatformPercentFee = toAccount.GetPlatformPercentFee(msgs, dc),
                StripeChargeOrRefundId = stripeChargeOrRefundId,
                TransactionTypeId = transactionType
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }
    }
}
