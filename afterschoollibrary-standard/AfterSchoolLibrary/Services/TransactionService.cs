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

        public static Transaction CreateNew(CheckoutObject checkoutObject, TransactionType transactionType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return CreateNew(
                checkoutObject.PurchasingAccount, 
                checkoutObject.BillingAccount, 
                checkoutObject.OverrideAmount ?? checkoutObject.TuitionFee, 
                checkoutObject.ParticipationObject.Name, 
                checkoutObject.Purchase?.ID, 
                transactionType, 
                checkoutObject.PaymentType,
                checkoutObject.PaymentSourceType,
                checkoutObject.GetPlatformFlatFee(msgs, dc),
                checkoutObject.GetPlatformPercentFee(msgs, dc),
                checkoutObject.StripeChargeId,
                loggedInUser, 
                msgs, 
                dc);
        }

        public static Transaction CreateNew(Account purchasingAccount, Account billingAccount, decimal? amount, string participationObjectName, int? purchaseId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, decimal platformFee, decimal platformPercentFee, string stripeChargeOrRefundId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();            

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount.ID.Value,
                Description = $"{participationObjectName} registration",
                PayerAccountId = purchasingAccount?.ID,
                PaymentTypeId = paymentType,
                PurchaseId = purchaseId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                PlatformFlatFee = platformFee,
                PlatformPercentFee = platformPercentFee,
                StripeChargeOrRefundId = stripeChargeOrRefundId,
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = paymentSourceType
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }
    }
}
