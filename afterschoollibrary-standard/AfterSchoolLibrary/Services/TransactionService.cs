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

        public static decimal GetFeesChargedToBiller(this Transaction transaction)
        {            
            if (transaction.PlatformFlatFee.HasValue || transaction.PlatformPercentFee.HasValue)
                return (transaction.Amount.Value * transaction.PlatformPercentFee.Value) + transaction.PlatformFlatFee.Value;

            var total = 0M;

            if (transaction.ConvenienceFeePayerAccountId == transaction.BillerAccountId)
                total += transaction.ConvenienceFeeCharged.Value;

            if (transaction.PaymentTypeFeePayerAccountId == transaction.BillerAccountId)
                total += transaction.PaymentTypeFeeCharged.Value;

            return total;
        }

        public static decimal GetFeesChargedToPayer(this Transaction transaction)
        {
            var total = 0M;

            if (transaction.ConvenienceFeePayerAccountId == transaction.PayerAccountId)
                total += transaction.ConvenienceFeeCharged.Value;

            if (transaction.PaymentTypeFeePayerAccountId == transaction.PayerAccountId)
                total += transaction.PaymentTypeFeeCharged.Value;

            return total;
        }

        public static decimal GetBillerAmount(this Transaction transaction)
        {
            return transaction.Amount.Value - transaction.GetFeesChargedToPayer();
        }

        public static decimal GetPayerAmount(this Transaction transaction)
        {
            return transaction.Amount.Value;
        }

        public static decimal GetBillerPayout(this Transaction transaction)
        {
            return transaction.GetBillerAmount() - transaction.GetFeesChargedToBiller();
        }

        public static Purchase GetPurchase(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.PurchaseId.HasValue)
                return Purchase.Load(transaction.PurchaseId.Value, msgs, dc);

            return null;
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
            var useConnectedAccount = checkoutObject.BillingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

            return CreateNew(
                checkoutObject.PayingAccount,
                checkoutObject.BillingAccount,
                checkoutObject.ConvenienceFeePayer,
                checkoutObject.CreditCardFeePayer,
                checkoutObject.OverrideAmount ?? checkoutObject.GetTuitionCostWithFees(msgs, dc),
                checkoutObject.ParticipationObject.Name,
                checkoutObject.Purchase?.ID,
                transactionType,
                checkoutObject.PaymentType,
                checkoutObject.PaymentSourceType,
                useConnectedAccount ? checkoutObject.GetPlatformFlatFee(msgs, dc) : new decimal?(), // deprecated property
                useConnectedAccount ? checkoutObject.GetPlatformPercentFee(msgs, dc) : new decimal?(), // deprecated property
                checkoutObject.ConvenienceFeeOverrideAmount ?? checkoutObject.GetTuitionConvenienceFee(msgs, dc),
                checkoutObject.PaymentTypeFeeOverrideAmount ?? checkoutObject.GetTuitionCreditCardFee(msgs, dc),
                checkoutObject.StripeChargeId,
                loggedInUser,
                msgs,
                dc);
        }

        public static Transaction CreateNew(Account payingAccount, Account billingAccount, Account ConvenienceFeePayer, Account PaymentTypeFeePayer, decimal? amount, string participationObjectName, int? purchaseId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, decimal? platformFee, decimal? platformPercentFee, decimal convenienceFee, decimal paymentTypeFee, string stripeChargeOrRefundId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount.ID.Value,
                Description = $"{participationObjectName} registration",
                PayerAccountId = payingAccount?.ID,
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
                PaymentSourceTypeId = paymentSourceType,
                ConvenienceFeePayerAccountId = ConvenienceFeePayer?.ID,
                PaymentTypeFeePayerAccountId = PaymentTypeFeePayer?.ID,
                ConvenienceFeeCharged = convenienceFee,
                PaymentTypeFeeCharged = paymentTypeFee,
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }


        public static Transaction CreateNewRefund(Transaction originalTransaction, decimal amount, string refundId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // inverse billing and paying for refund
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

        public static Transaction CreateNew(Account purchasingAccount, Account billingAccount, decimal amount, Purchase purchase, string participationObjectName, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, string stripeChargeOrRefundId, Transaction originalTransaction, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static bool IsTotallyRefunded(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
            {
                var totalRefund = -Transaction.GetTotalRefundByOriginalTransaction(transaction.ID.Value, msgs, dc);

                return totalRefund == transaction.Amount;
            }

            return false;
        }

        public static decimal GetRefundedAmount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return -Transaction.GetTotalRefundByOriginalTransaction(transaction.ID.Value, msgs, dc);
        }

        public static Payout GetPayout(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.PayoutId != null)
                Payout.Load(transaction.PayoutId.Value, msgs, dc);

            return null;
        }

        public static int GetWaitPeriodInDays(this Transaction transaction)
        {
            if (transaction.PaymentTypeId == PaymentType.ACH)
                return 5;

            return 2;
        }

        public static string GetStatus(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var currentPayout = GetPayout(transaction, msgs);

            if (currentPayout != null)
                switch (currentPayout.PayoutStatusId)
                {
                    case PayoutStatus.Completed:
                        return "Paid";
                    case PayoutStatus.Requested:
                        return "Payout Requested";
                    default:
                        return currentPayout.PayoutStatusId.ToString();
                }
            
            if (DateTime.UtcNow.IsBefore(transaction.CreateDate.Value.AddDays(transaction.GetWaitPeriodInDays())))
                return "Incoming";

            return null;
        }

        public static bool CanRequestPayout(this Transaction transaction)
        {
            return transaction.PayoutId == null &&
                DateTime.UtcNow.IsOnOrAfter(transaction.CreateDate.Value.AddDays(transaction.GetWaitPeriodInDays())) &&
                transaction.PlatformPercentFee == 0 &&
                transaction.PaymentSourceTypeId != PaymentSourceType.Manual;
        }
    }
}
