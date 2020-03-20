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

        public static Transaction GetOriginalTransaction(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.OriginalTransactionId.HasValue)
                return Transaction.Load(transaction.OriginalTransactionId.Value, msgs, dc);

            return null;
        }

        public static decimal GetBillerAmount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsRefund())
                return transaction.GetOriginalTransaction(msgs, dc).GetBillerAmount(msgs, dc);

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

        public static bool IsRefund(this Transaction transaction)
        {
            return transaction.TransactionTypeId == TransactionType.Refund;
        }


        public static List<ParticipationObject> GetParticipationObjects(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByTransactionIdByIsDeleted(transaction.ID.Value, false, msgs, dc)
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

        public static Transaction CreateNew(Account purchasingAccount, Account billingAccount, decimal amount, Purchase purchase, string participationObjectName, TransactionType transactionType, string stripeChargeOrRefundId, Transaction originalTransaction, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount.ID.Value,
                Description = $"{participationObjectName} refund",
                PayerAccountId = purchasingAccount.ID.Value,
                PurchaseId = purchase.ID.Value,
                PaymentTypeId = originalTransaction.PaymentTypeId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                StripeChargeOrRefundId = stripeChargeOrRefundId,
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = originalTransaction.PaymentSourceTypeId,
                OriginalTransactionId = originalTransaction.ID.Value,
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }

        public static bool HasUnfulfilledRefundRequests(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RefundRequest.GetCountByTransactionIdByRequestFulfilled(transaction.ID.Value, false, msgs, dc) > 0;
        }

        public static bool IsTotallyRefunded(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
            {
                var totalRefund = -Transaction.GetTotalRefundByOriginalTransaction(transaction.ID.Value, false, msgs, dc);

                return totalRefund == transaction.Amount;
            }

            return false;
        }

        public static bool IsAmountRefundable(this Transaction transaction, decimal amount, bool allowFullAmountRefund = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return amount >= 1 && amount <= transaction.GetRefundableAmount(allowFullAmountRefund, msgs, dc);
        }

        public static decimal GetRefundableAmount(this Transaction transaction, bool allowFullAmountRefund = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (allowFullAmountRefund)
                return transaction.GetPayerAmount() - transaction.GetRefundedAmount(msgs, dc);

            return transaction.GetBillerAmount(msgs, dc) - transaction.GetRefundedAmount(msgs, dc);
        }

        public static decimal GetRefundedAmount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalRefundByOriginalTransaction(transaction.ID.Value, false, msgs, dc);
        }

        public static Payout GetPayout(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.PayoutId.HasValue)
                return Payout.Load(transaction.PayoutId.Value, msgs, dc);

            return null;
        }

        public static int GetWaitPeriodInDays(this Transaction transaction)
        {
            return GetWaitPeriodInDays(transaction.PaymentTypeId);
        }

        public static int GetWaitPeriodInDays(PaymentType paymentType)
        {
            if (paymentType == PaymentType.ACH)
                return 5;

            return 2;
        }

        public static string GetStatus(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.PaymentSourceTypeId != PaymentSourceType.AfterSchoolPlatform)
                return "N/A";

            if (transaction.IsRefund())
                return "Processed";

            if (transaction.PlatformPercentFee.HasValue)
                return "Paid";

            if (transaction.HasRefunds())
                return "Refunded";

            var currentPayout = GetPayout(transaction, msgs);

            if (currentPayout.IsNotEmpty())
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

            return "Ready For Payout";
        }

        public static bool CanRequestPayout(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return !transaction.HasPayout &&
                DateTime.UtcNow.IsOnOrAfter(transaction.CreateDate.Value.AddDays(transaction.GetWaitPeriodInDays())) &&
                !transaction.PlatformPercentFee.HasValue &&
                transaction.PaymentSourceTypeId == PaymentSourceType.AfterSchoolPlatform &&
                transaction.IsCharge() &&
                !transaction.HasRefunds(msgs, dc);
        }

        public static Payout CreatePayout(this Transaction transaction, PayoutStatus payoutStatus, PaymentType paymentType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!transaction.PayoutId.HasValue)
                return PayoutService.CreateNew(
                    account: transaction.GetBillerAccount(msgs, dc),
                    transactions: transaction.AsList(),
                    payoutStatus: payoutStatus,
                    paymentType: paymentType,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);

            return null;
        }

        public static void MarkAsDeleted(this Transaction transaction, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var payments = transaction.GetPayments(msgs, dc);

            payments.ForEach(payment => {
                payment.MarkAsDeleted(loggedInUser, msgs, dc);
            });

            transaction.IsDeleted = true;
            transaction.LastModifiedDate = DateTime.UtcNow;
            transaction.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(transaction.Save(dc));
        }

        public static List<ActivityPayment> GetPayments(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByTransactionIdByIsDeleted(transaction.ID.Value, false, msgs, dc);
        }

        public static RefundRequest RequestRefund(this Transaction transaction, decimal refundAmount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (!transaction.IsAmountRefundable(refundAmount, false, msgs))
                msgs.Add("Amount is not refundable", MessageType.UserError);

            if (msgs.HasNoErrors())
                return RefundRequestService.CreateNewRefundRequest(transaction, refundAmount, loggedInUser, msgs, dc);

            return null;
        }

        public static Transaction Refund(this Transaction transaction, decimal refundAmount, bool allowFullAmountRefund, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (!transaction.IsAmountRefundable(refundAmount, allowFullAmountRefund, msgs))
                msgs.Add("Amount is not refundable", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var refund = Billing.StripeHelper.CreateRefund(
                                chargeId: transaction.StripeChargeOrRefundId,
                                totalAmount: refundAmount,
                                reason: Billing.StripeRefundReason.RequestedByCustomer.ToString().SplitByUpperCase().Replace(" ", "_").ToLower(),
                                msgs: msgs);

                if (msgs.HasNoErrors())
                    return transaction.CreateNewRefund(
                        amount: refundAmount,
                        refundId: refund.Id,
                        transactionType: TransactionType.Refund,
                        paymentType: transaction.PaymentTypeId,
                        paymentSourceType: PaymentSourceType.AfterSchoolPlatform,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);
            }

            return null;
        }

        public static Transaction Refund(this Transaction transaction, decimal refundAmount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return transaction.Refund(refundAmount, false, loggedInUser, msgs, dc);
        }

        public static Transaction RefundPayerFullAmount(this Transaction transaction, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return transaction.Refund(transaction.GetPayerAmount(), true, loggedInUser, msgs, dc);
        }

        public static Transaction CreateNewRefund(this Transaction originalTransaction, decimal amount, string refundId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // inverse billing and paying for refund
            return CreateNew(
                originalTransaction.GetBillerAccount(),
                originalTransaction.GetPayerAccount(),
                amount,
                originalTransaction.GetPurchase(),
                originalTransaction.GetParticipationObjects().FirstOrDefault().Name,
                transactionType,
                refundId,
                originalTransaction,
                loggedInUser,
                msgs,
                dc);
        }

        public static bool HasRefunds(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByOriginalTransactionIdByIsDeleted(transaction.ID.Value, false, msgs, dc).Any();
        }

        public static bool CanRefund(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return !transaction.HasPayout && !transaction.HasRefunds(msgs, dc) && transaction.TransactionTypeId == TransactionType.Charge;
        }
    }
}
