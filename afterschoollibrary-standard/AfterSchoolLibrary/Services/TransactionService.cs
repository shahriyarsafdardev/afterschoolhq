using System;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

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

        public static decimal GetPayout(this Transaction transaction)
        {
            return transaction.Amount.Value - transaction.GetPlatformFee();
        }

        public static string GetTransactionNumber(this Transaction transaction)
        {
            return transaction.ID.ToString().PadLeft(8, '0');
        }

        public static bool IsCharge(this Transaction transaction)
        {
            return transaction.TransactionTypeId == TransactionType.Charge;
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

        public static Transaction CreateNew(Transaction originalTransaction, decimal amount, string refundId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return CreateNew(
                originalTransaction.GetBillerAccount(),
                originalTransaction.GetPayerAccount(),
                amount,
                originalTransaction.GetPurchase(),
                originalTransaction.GetParticipationObjects().FirstOrDefault().Name,
                transactionType,
                paymentType,
                paymentSourceType,
                refundId,
                originalTransaction,
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

        public static Transaction CreateNew(Account purchasingAccount, Account billingAccount, decimal amount, Purchase purchase, string participationObjectName, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, string stripeChargeOrRefundId, Transaction originalTransaction , WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount.ID.Value,
                Description = $"{participationObjectName} refund",
                PayerAccountId = purchasingAccount.ID.Value,
                PurchaseId = purchase.ID.Value,
                PaymentTypeId = paymentType,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                StripeChargeOrRefundId = stripeChargeOrRefundId,
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = paymentSourceType,
                PlatformFlatFee = 0,
                PlatformPercentFee = 0,
                OriginalTransactionId = originalTransaction.ID.Value, 
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }
    }
}
