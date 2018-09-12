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
    public static class TransactionService
    {
        public static decimal GetPlatformFee(this Transaction transaction)
        {
            return (transaction.Amount.Value * transaction.PlatformPercentFee.Value) + transaction.PlatformFlatFee.Value;
        }

        public static Purchase GetPurchase(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.PurchaseId.HasValue)
                return Purchase.Load(transaction.PurchaseId.Value, msgs, dc);

            return null;
        }

        public static List<ParticipationObject> GetParticipationObjects(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByTransactionId(transaction.ID.Value, msgs, dc)
                .Select(payment => 
                    new ParticipationObject(payment.ParticipationObjectId.Value, payment.ParticipationObjectTypeId, msgs, dc)
                )
                .ToList();
        }

        public static Account GetBillerAccount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(transaction.BillerAccountId.Value, msgs, dc);
        }
        public static Account GetPayerAccount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(transaction.PayerAccountId.Value, msgs, dc);
        }

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
