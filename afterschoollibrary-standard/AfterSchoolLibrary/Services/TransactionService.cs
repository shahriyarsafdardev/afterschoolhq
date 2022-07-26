using System;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Billing;

namespace AfterSchoolLibrary.Services
{
    public static class TransactionService
    {

        public static decimal GetFeesChargedToBiller(this Transaction transaction)
        {
            if (transaction.PaymentSourceTypeId != PaymentSourceType.AfterSchoolPlatform)
                return 0;

            // handle old transactions
            if (!transaction.ConvenienceFeeCharged.HasValue && !transaction.ConvenienceFeePayerAccountId.HasValue)
                return (transaction.Amount.Value * transaction.PlatformPercentFee.Value) + transaction.PlatformFlatFee.Value;

            var total = 0M;

            if (transaction.ConvenienceFeePayerAccountId == transaction.BillerAccountId && transaction.ConvenienceFeeCharged.HasValue)
                total += transaction.ConvenienceFeeCharged.Value;

            if (transaction.PaymentTypeFeePayerAccountId == transaction.BillerAccountId && transaction.PaymentTypeFeeCharged.HasValue)
                total += transaction.PaymentTypeFeeCharged.Value;

            return total;
        }

        public static decimal GetFeesChargedToPayer(this Transaction transaction)
        {
            return transaction.GetPaymentTypeFeesChargedToPayer() + transaction.GetConvenienceFeesChargedToPayer();
        }

        public static decimal GetFees(this Transaction transaction)
        {
            return transaction.ConvenienceFeeCharged.IfEmpty(0) + transaction.PaymentTypeFeeCharged.IfEmpty(0);
        }

        public static decimal GetConvenienceFeesChargedToPayer(this Transaction transaction)
        {
            if (transaction.ConvenienceFeePayerAccountId == transaction.PayerAccountId && transaction.ConvenienceFeeCharged.HasValue)
                return transaction.ConvenienceFeeCharged.Value;

            return 0;
        }

        public static decimal GetPaymentTypeFeesChargedToPayer(this Transaction transaction)
        {            
            if (transaction.PaymentTypeFeePayerAccountId == transaction.PayerAccountId && transaction.PaymentTypeFeeCharged.HasValue)
                return transaction.PaymentTypeFeeCharged.Value;

            return 0;
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
            {
                var originalBillerAmount = transaction.GetOriginalTransaction(msgs, dc).GetBillerAmount(msgs, dc);

                if (transaction.Amount.Value > originalBillerAmount)
                    return originalBillerAmount;

                return transaction.Amount.Value;
            };

            return Math.Max(transaction.Amount.Value - transaction.GetFees() - transaction.GetRefundedAmountAHQ(msgs, dc), 0);
        }

        public static decimal GetPayerAmount(this Transaction transaction)
        {
            return transaction.Amount.Value;
        }

        public static decimal GetBillerPayout(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
                return transaction.GetBillerAmount(msgs, dc) - transaction.GetRefundedAmountAHQWithoutFees(msgs, dc);

            if (transaction.IsCredit())
            {
                var totalCredited = Transaction.GetTotalAmountCreditedToAccount(
                                            transaction.BillerAccountId.Value,
                                            null,
                                            null,
                                            transaction.CreateDate.Value.AddSeconds(1),
                                            false,
                                            msgs,
                                            dc);

                var totalDebited = Transaction.GetTotalAmountDebitedFromAccount(
                                    transaction.BillerAccountId.Value,
                                    null,
                                    null,
                                    transaction.CreateDate.Value,
                                    false,
                                    msgs,
                                    dc);

                return totalCredited - totalDebited;
            }

            return 0;
        }

        public static bool HasBillerPayout(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return transaction.GetBillerPayout(msgs, dc) > 0;
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

        public static bool IsCredit(this Transaction transaction)
        {
            return transaction.TransactionTypeId == TransactionType.Credit;
        }

        public static bool IsDebit(this Transaction transaction)
        {
            return transaction.TransactionTypeId == TransactionType.Debit;
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
            var useConnectedAccount = checkoutObject.BillingAccount.UseStripeConnectDuringCheckout(msgs, dc);
            var usePlatformFees = useConnectedAccount && !checkoutObject.OverrideAmount.HasValue;

            return CreateNew(
                checkoutObject.PayingAccount,
                checkoutObject.BillingAccount,
                checkoutObject.ConvenienceFeePayer,
                checkoutObject.CreditCardFeePayer,
                checkoutObject.OverrideAmount ?? checkoutObject.GetTotalCostToPurchaseTodayWithFees(msgs, dc),
                checkoutObject.ParticipationObject?.Name,
                checkoutObject.Purchase?.ID,
                transactionType,
                checkoutObject.PaymentType,
                checkoutObject.PaymentSourceType,
                usePlatformFees ? checkoutObject.GetPlatformFlatFeeWithCoupon(msgs, dc) : new decimal?(), 
                usePlatformFees ? checkoutObject.GetPlatformPercentFeeWithCoupon(msgs, dc) : new decimal?(),
                checkoutObject.ConvenienceFeeOverrideAmount ?? checkoutObject.GetTotalConvenienceFeesToday(msgs, dc), //NOTE: Should be 0 with 100% off coupon applied
                checkoutObject.PaymentTypeFeeOverrideAmount ?? checkoutObject.GetTotalCreditCardFeesToday(msgs, dc),
                checkoutObject.StripeChargeId,
                checkoutObject.Description,
                checkoutObject.Date,
                useConnectedAccount,
                loggedInUser,
                checkoutObject.CouponValue(msgs, dc),
                checkoutObject.Coupon,
                msgs,
                dc);
        }

        public static Transaction CreateNew(Account payingAccount, Account billingAccount, Account ConvenienceFeePayer, Account PaymentTypeFeePayer, decimal? amount, string participationObjectName, int? purchaseId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, decimal? platformFee, decimal? platformPercentFee, decimal convenienceFee, decimal paymentTypeFee, string stripeChargeOrRefundId, string description, DateTime createDate, bool usedStripeConnect, WebUser loggedInUser, decimal? couponValue, Coupon coupon = null,  SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount.ID.Value,
                Description = description,
                PayerAccountId = payingAccount?.ID,
                PaymentTypeId = paymentType,
                PurchaseId = purchaseId,
                CreateDate = createDate,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = createDate,
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
                UsedStripeConnect = usedStripeConnect,
                CouponId = coupon?.ID,
                CouponValue = couponValue
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }

        public static Transaction CreateNew(Account payingAccount, Account billingAccount, decimal amount, decimal? convenienceFeeAmount, int? convenienceFeePayerId, decimal? paymentTypeFeeAmount, int? paymentTypeFeePayerId, Purchase purchase, string refundDescription, TransactionType transactionType, string stripeChargeOrRefundId, Transaction originalTransaction, PaymentType paymentType, PaymentSourceType paymentSourceType, bool usedStripeConnect, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount?.ID,
                Description = refundDescription,
                PayerAccountId = payingAccount?.ID,
                PurchaseId = purchase?.ID,
                PaymentTypeId = paymentType,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                StripeChargeOrRefundId = stripeChargeOrRefundId,
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = paymentSourceType,
                OriginalTransactionId = originalTransaction?.ID,
                ConvenienceFeeCharged = convenienceFeeAmount,
                PaymentTypeFeeCharged = paymentTypeFeeAmount,
                ConvenienceFeePayerAccountId = convenienceFeePayerId,
                PaymentTypeFeePayerAccountId = paymentTypeFeePayerId,
                UsedStripeConnect = usedStripeConnect,
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }

        public static Transaction CreateNew(Account payingAccount, Account billingAccount, decimal amount, string description, TransactionType transactionType, PaymentSourceType paymentSourceType, bool usedStripeConnect, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var transaction = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billingAccount?.ID,
                Description = description,
                PayerAccountId = payingAccount?.ID,
                PaymentTypeId = PaymentType.NotSet,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = paymentSourceType,
                UsedStripeConnect = usedStripeConnect,
            };
            msgs.AddRange(transaction.Save(dc));

            return transaction;
        }

        public static bool HasUnfulfilledRefundRequests(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RefundRequest.GetCountByTransactionId(
                        transaction.ID.Value,
                        RefundRequest.InProcessStatuses,
                        msgs,
                        dc) > 0;
        }

        public static bool HasManuallyApprovedRefundRequest(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RefundRequest.GetCountByTransactionId(
                        transaction.ID.Value,
                        RefundRequestStatus.ApprovedManualRefund.AsList(),
                        msgs,
                        dc) > 0;
        }

        public static bool IsTotallyRefunded(this Transaction transaction, bool includeFees, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
            {
                var amount = transaction.GetPayerAmount();

                if (!includeFees)
                    amount = transaction.GetBillerAmount(msgs, dc);

                return transaction.GetRefundedAmount(msgs, dc) >= amount;
            }

            return false;
        }

        public static bool IsAmountRefundable(this Transaction transaction, decimal amount, bool allowFullAmountRefund = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return amount >= 1 && amount <= transaction.GetRefundableAmount(allowFullAmountRefund, msgs, dc);
        }

        public static decimal GetRefundableAmount(this Transaction transaction, bool allowFullAmountRefund = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
            {
                if (allowFullAmountRefund)
                    return transaction.GetPayerAmount() - transaction.GetRefundedAmountAHQ(msgs, dc);

                return transaction.GetBillerPayout(msgs, dc) + transaction.GetFeesChargedToBiller() - transaction.GetRefundedAmountAHQ(msgs, dc);
            }

            return 0;
        }

        public static bool IsAmountRefundable(this Transaction transaction, decimal amount, bool allowFullAmountRefund = false, bool isPartialRefundWithConvenienceFee = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return amount >= 1 && amount <= transaction.GetRefundableAmount(allowFullAmountRefund, isPartialRefundWithConvenienceFee, msgs, dc);
        }

        public static decimal GetRefundableAmount(this Transaction transaction, bool allowFullAmountRefund = false, bool isPartialRefundWithConvenienceFee = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
            {
                if (allowFullAmountRefund)
                    return transaction.GetPayerAmount() - transaction.GetRefundedAmountAHQ(msgs, dc);

                if (isPartialRefundWithConvenienceFee)
                    return transaction.GetParentRefundableAmount(msgs, dc);

                return transaction.GetBillerPayout(msgs, dc) + transaction.GetFeesChargedToBiller() - transaction.GetRefundedAmountAHQ(msgs, dc);
            }

            return 0;
        }

        public static bool HasRefundableAmount(this Transaction transaction, bool allowFullAmountRefund = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return transaction.GetRefundableAmount(allowFullAmountRefund, msgs, dc) > 0;
        }

        public static bool CanRefundTransactionFee(this Transaction transaction)
        {
            return false;
        }

        public static bool CanRefundConvenienceFee(this Transaction transaction)
        {
            return true; // transaction.CreateDate.Value.IsAfter(DateTime.UtcNow.AddDays(-2));
        }

        public static decimal GetParentRefundableAmount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var amount = transaction.GetPayerAmount();

            if (!transaction.CanRefundConvenienceFee())
                amount -= transaction.ConvenienceFeeCharged.Value;

            if (!transaction.CanRefundTransactionFee())
                amount -= transaction.PaymentTypeFeeCharged.Value;

            //partial refund, if any then deduct that too.
            var refunded = transaction.GetRefundedAmount(msgs, dc);
            if (refunded.IsNotEmpty())
                amount -= refunded;

            return amount;
        }

        public static decimal GetRefundedAmount(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalRefundByOriginalTransaction(transaction.ID.Value, false, msgs, dc);
        }

        public static decimal GetRefundedAmountManual(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalRefundByOriginalTransaction(transaction.ID.Value, PaymentSourceType.ManualProvider, false, msgs, dc);
        }

        public static decimal GetRefundedAmountAHQ(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalRefundByOriginalTransaction(
                        transaction.ID.Value, 
                        PaymentSourceType.AfterSchoolPlatform, 
                        false, 
                        msgs, 
                        dc) 
                    + 
                    Transaction.GetTotalRefundByOriginalTransaction(
                        transaction.ID.Value,
                        PaymentSourceType.ManualAfterSchoolHQ,
                        false,
                        msgs,
                        dc);
        }

        public static decimal GetRefundedAHQFees(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.GetTotalRefundFeesByOriginalTransaction(
                        transaction.ID.Value,
                        null,
                        PaymentSourceType.AfterSchoolPlatform,
                        false,
                        msgs,
                        dc)
                    +
                    Transaction.GetTotalRefundFeesByOriginalTransaction(
                        transaction.ID.Value,
                        null,
                        PaymentSourceType.ManualAfterSchoolHQ,
                        false,
                        msgs,
                        dc);
        }

        public static decimal GetRefundedAmountAHQWithoutFees(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return transaction.GetRefundedAmountAHQ(msgs, dc) - transaction.GetRefundedAHQFees(msgs, dc);
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

            if (!transaction.IsCharge())
                return "Processed";

            if (!transaction.HasRefundableAmount(false, msgs, dc))
                return "Refunded";

            var currentPayout = GetPayout(transaction, msgs, dc);

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

            if (transaction.HasRefundsAHQ(msgs, dc) && transaction.HasRefundableAmount(false, msgs, dc))
                return "Partially Refunded / Ready for Payout";

            if (transaction.HasRefundsManual(msgs, dc))
                return "Manual Refund / Ready For Payout";

            var billerAccount = transaction.GetBillerAccount(msgs, dc);
            if (billerAccount.IsNotEmpty() && billerAccount.HasStripeConnectedAccount)
                return "Payout Completed";

            return "Ready For Payout";
        }

        public static bool CanRequestPayout(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!transaction.HasPayout && transaction.HasBillerPayout(msgs, dc))
            {
                if (transaction.PaymentSourceTypeId == PaymentSourceType.AfterSchoolPlatform && transaction.IsCharge())
                    return
                        DateTime.UtcNow.IsOnOrAfter(transaction.CreateDate.Value.AddDays(transaction.GetWaitPeriodInDays())) &&
                        !transaction.UsedStripeConnect &&
                        !transaction.HasUnfulfilledRefundRequests(msgs, dc) &&
                        transaction.HasRefundableAmount(false, msgs, dc);

                if ((transaction.PaymentSourceTypeId == PaymentSourceType.ManualAfterSchoolHQ && transaction.IsCredit()))
                {
                    var totalCredited = Transaction.GetTotalAmountCreditedToAccount(
                                            transaction.BillerAccountId.Value,
                                            null,
                                            null,
                                            transaction.CreateDate.Value.AddSeconds(1),
                                            false,
                                            msgs,
                                            dc);

                    var totalDebited = Transaction.GetTotalAmountDebitedFromAccount(
                                        transaction.BillerAccountId.Value,
                                        null,
                                        null,
                                        transaction.CreateDate.Value,
                                        false,
                                        msgs,
                                        dc);

                    return totalCredited - totalDebited > 0;
                }
            }

            return false;
        }

        public static Payout CreatePayout(this Transaction transaction, PayoutStatus payoutStatus, PaymentType paymentType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!transaction.PayoutId.HasValue)
                return PayoutService.CreateNew(
                    account: transaction.GetBillerAccount(msgs, dc),
                    statementDescriptor: null,
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

        public static bool CanRefundViaStripe(this Transaction transaction)
        {
            if (transaction.PaymentTypeId == PaymentType.ACH && transaction.CreateDate.Value.AddDays(90) < DateTime.UtcNow)
                return false;

            return true;
        }

        public static RefundRequest RequestRefund(this Transaction transaction, decimal refundAmount, RefundRequestStatus refundRequestStatus, string reason, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            RefundRequest request = null;

            if (!transaction.IsAmountRefundable(refundAmount, refundRequestStatus == RefundRequestStatus.RequestedByParent, true, msgs, dc))
                msgs.Add("Amount is not refundable", MessageType.UserError);

            if (msgs.HasNoErrors())
                request = RefundRequestService.CreateNew(transaction, refundAmount, refundRequestStatus, reason, loggedInUser, msgs, dc);

            if (msgs.HasNoErrors())
                Helpers.EmailHelper.SendRefundRequestToProvider(request, loggedInUser, msgs, dc);

            return request;
        }

        public static Transaction Refund(this Transaction transaction, decimal refundAmount, bool allowFullAmountRefund, decimal? convenienceFeeAmount, decimal? paymentTypeFeeAmount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return transaction.Refund(
                        refundAmount, 
                        allowFullAmountRefund, 
                        convenienceFeeAmount, 
                        paymentTypeFeeAmount, 
                        PaymentSourceType.AfterSchoolPlatform, 
                        loggedInUser, 
                        msgs, 
                        dc);
        }

        public static Transaction Refund(this Transaction transaction, decimal refundAmount, bool allowFullAmountRefund, decimal? convenienceFeeAmount, decimal? paymentTypeFeeAmount, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Transaction refund = null;

            if (!transaction.IsAmountRefundable(refundAmount, allowFullAmountRefund, msgs))
                msgs.Add("Amount is not refundable", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                string stripeRefundId = null;

                if (paymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                {                    
                    //.. when refunding, platform will pay all the amount to parent first
                    //.. then based on that and after deducting fee, we will charge remaining amount to provider
                    //.. lets say activity charge is 100$, and convenience charge is 6$, then parent pays 110$ (106$ + 4$ cc fee).
                    //.. so while refunding, parent can refund only 106$(cc fee is not refundable),

                    var stripeHelper = StripeHelper.GetStripeHelperConnectAccount();
                    var stripeRefund = stripeHelper.CreateRefund(
                                    chargeId: transaction.StripeChargeOrRefundId,
                                    totalAmount: refundAmount,
                                    reason: Billing.StripeRefundReason.RequestedByCustomer.ToString().SplitByUpperCase().Replace(" ", "_").ToLower(),
                                    reverseTransfer: false,
                                    msgs: msgs);

                    if (stripeRefund.IsNotEmpty())
                    {
                        // set stripe refund id, so we can use that later for create new refund
                        stripeRefundId = stripeRefund.Id;

                        // get stripe transfer from original payment
                        var stripeOriginalPaymentChargeId = stripeRefund.ChargeId;
                        var stripeOriginalCharge = stripeHelper.GetCharge(stripeOriginalPaymentChargeId, msgs);
                        var stripeTransferId = stripeOriginalCharge.TransferId;

                        // create transfer reversal
                        // calculate amount to reverse
                        var billerPayout = transaction.GetBillerPayout(msgs, dc);
                        var amountToReverse = Math.Min(billerPayout, refundAmount);

                        if (stripeTransferId.IsNotEmpty() && amountToReverse > 0)
                           stripeHelper.CreateTransferReversal(stripeTransferId, amountToReverse, msgs);
                    }
                    else
                        msgs.Add("Error creating refund", MessageType.UserError);
                }

                var paymentType = transaction.PaymentTypeId;
                var refundingAccount = transaction.GetBillerAccount(msgs, dc);
                var refundedAccount = transaction.GetPayerAccount(msgs, dc);

                if (paymentSourceType == PaymentSourceType.ManualProvider)
                    paymentType = PaymentType.NotSet;
                else if (paymentSourceType == PaymentSourceType.ManualAfterSchoolHQ)
                {
                    paymentType = PaymentType.Check;
                    refundingAccount = AccountService.GetAfterSchoolAccount(msgs, dc);
                }

                if (msgs.HasNoErrors())
                    refund = transaction.CreateNewRefund(
                        fullAmount: refundAmount,
                        convenienceFeeAmount: convenienceFeeAmount,
                        paymentTypeFeeAmount: paymentTypeFeeAmount,
                        refundId: stripeRefundId,
                        transactionType: TransactionType.Refund,
                        paymentType: paymentType,
                        paymentSourceType: paymentSourceType,
                        refundingAccount: refundingAccount,
                        refundedAccount: refundedAccount,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);
            }

            return refund;
        }

        public static Transaction Refund(this Transaction transaction, decimal refundAmount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return transaction.Refund(refundAmount, false, null, null, loggedInUser, msgs, dc);
        }

        public static Transaction ManualRefund(this Transaction transaction, decimal refundAmount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return transaction.Refund(refundAmount, false, null, null, PaymentSourceType.ManualProvider, loggedInUser, msgs, dc);
        }

        public static Transaction RefundPayerFullAmount(this Transaction transaction, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {            
            return transaction.Refund(
                    transaction.GetPayerAmount(), 
                    true, 
                    transaction.GetConvenienceFeesChargedToPayer(),
                    transaction.GetPaymentTypeFeesChargedToPayer(),
                    loggedInUser, 
                    msgs, 
                    dc);
        }

        public static Transaction CreateNewRefund(this Transaction originalTransaction, decimal fullAmount, decimal? convenienceFeeAmount, decimal? paymentTypeFeeAmount, string refundId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return originalTransaction.CreateNewRefund(
                fullAmount,
                convenienceFeeAmount,
                paymentTypeFeeAmount,
                refundId,
                transactionType,
                paymentType,
                paymentSourceType,
                originalTransaction.GetBillerAccount(),
                originalTransaction.GetPayerAccount(),
                loggedInUser,
                msgs,
                dc);
        }

        public static Transaction CreateNewRefund(this Transaction originalTransaction, decimal fullAmount, decimal? convenienceFeeAmount, decimal? paymentTypeFeeAmount, string refundId, TransactionType transactionType, PaymentType paymentType, PaymentSourceType paymentSourceType, Account refundingAccount, Account refundedAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var refundDescription = "Payment refund";
            var participationObjects = originalTransaction.GetParticipationObjects(msgs, dc);
            var useStripeConnect = refundingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

            if (participationObjects.Any())
                refundDescription = $"{participationObjects.FirstOrDefault().Name} refund";

            return CreateNew(
                refundingAccount,
                refundedAccount,
                fullAmount,
                convenienceFeeAmount,
                originalTransaction.ConvenienceFeePayerAccountId,
                paymentTypeFeeAmount,
                originalTransaction.PaymentTypeFeePayerAccountId,
                originalTransaction.GetPurchase(),
                refundDescription,
                transactionType,
                refundId,
                originalTransaction,
                paymentType,
                paymentSourceType,
                useStripeConnect,
                loggedInUser,
                msgs,
                dc);
        }

        public static bool HasRefunds(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByOriginalTransactionIdByIsDeleted(transaction.ID.Value, false, msgs, dc).Any();
        }

        public static bool HasRefundsAHQ(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByOriginalTransactionIdByIsDeleted(transaction.ID.Value, PaymentSourceType.AfterSchoolPlatform, false, msgs, dc).Any();
        }

        public static bool HasRefundsManual(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByOriginalTransactionIdByIsDeleted(transaction.ID.Value, PaymentSourceType.ManualProvider, false, msgs, dc).Any();
        }   

        public static bool CanParentRequestRefund(this Transaction transaction, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (transaction.IsCharge())
            {
                if (transaction.PaymentTypeId == PaymentType.CreditCard)
                    return true;

                if (transaction.PaymentTypeId == PaymentType.ACH && transaction.CreateDate.Value.IsOnOrAfter(DateTime.UtcNow.AddDays(-90)))
                    return true;
            }

            return false;
        }

        public static Stripe.Charge GetStripeCharge(this Transaction transaction, SystemMessages msgs)
        {
            if (transaction.TransactionTypeId == TransactionType.Charge)
                return StripeHelper.GetStripeHelperConnectAccount().GetCharge(transaction.StripeChargeOrRefundId, msgs);

            return null;
        }

        public static bool HasStripeTransfer(this Transaction transaction, SystemMessages msgs)
        {
            var charge = transaction.GetStripeCharge(msgs);

            if (charge.IsNotEmpty())
                return charge.TransferId.IsNotEmpty() || charge.TransferGroup.IsNotEmpty();

            return false;
        }

        public static Stripe.Transfer CreateStripeTransfer(this Transaction transaction, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var billerAccount = transaction.GetBillerAccount(msgs, dc);
            var canCreateStripeTransfer = billerAccount.HasStripeConnectedAccount && billerAccount.StripePayoutsEnabled;
            // cant create a transfer if the billerAccount doesnt have stripe connected
            // there could be cases where the transaction occured before the biller setup stripe and we want to create a transfer afterwards
            if (transaction.UsedStripeConnect && !canCreateStripeTransfer)
                return null;

            if (transaction.HasStripeTransfer(msgs))
            {
                msgs.Add("Already has associated transfer", MessageType.Notice);
                return null;
            }

            var billerAmount = transaction.GetBillerAmount(msgs, dc);
            var hasStripeUserId = billerAccount.StripeUserId.IsNotEmpty();

            if (hasStripeUserId && !transaction.PayoutId.HasValue)
            {
                var transfer = StripeHelper.GetStripeHelperConnectAccount().CreateTransfer(
                                billerAmount,
                                transaction.Description,
                                transaction.Uid.ToString(),
                                billerAccount.StripeUserId,
                                transaction.StripeChargeOrRefundId,
                                new Dictionary<string, string>()
                                {
                                    { "TransactionUid", transaction.Uid.ToString() },
                                    { "StripeCharge", transaction.StripeChargeOrRefundId },
                                    { "LoggedInUser", loggedInUser.Uid.ToString() },
                                },
                                msgs);

                if (transfer.IsEmpty())
                    msgs.Add("Error creating transfer", MessageType.UserError);

                return transfer;
            }

            return null;
        }

        public static decimal GetStripeFee(this Transaction transaction) 
        {
            if (transaction.IsCharge() && transaction.PaymentSourceTypeId == PaymentSourceType.AfterSchoolPlatform)
            {
                if (transaction.PaymentTypeId == PaymentType.CreditCard)
                    return StripeHelper.CalculateStripeFee(transaction.GetPayerAmount(), true);
                
                if (transaction.PaymentTypeId == PaymentType.ACH)
                    return StripeHelper.CalculateStripeAchFee(transaction.GetPayerAmount(), true);
            }

            return 0;
        }

        public static void MoveToAccount(this Transaction transaction, Account toAccount, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (transaction.IsEmpty() || toAccount.IsEmpty())
                return;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // update transaction
                    if (transaction.BillerAccountId == transaction.ConvenienceFeePayerAccountId)
                        transaction.ConvenienceFeePayerAccountId = toAccount.ID;

                    if (transaction.BillerAccountId == transaction.PaymentTypeFeePayerAccountId)
                        transaction.PaymentTypeFeePayerAccountId = toAccount.ID;

                    transaction.BillerAccountId = toAccount.ID;
                    amsgs.AddRange(transaction.Save(adc));

                    // update payments
                    var payments = transaction.GetPayments(amsgs, adc);

                    payments.ForEach(payment =>
                    {
                        payment.AccountId = toAccount.ID;
                        amsgs.AddRange(payment.Save(adc));
                    });

                    // update registrant & registration
                    var purchase = transaction.GetPurchase(amsgs, adc);

                    if (purchase.IsNotEmpty())
                    {
                        var registrant = purchase.GetRegistrant(amsgs, adc);
                        var registration = registrant.GetRegistrations(amsgs, adc).First();

                        registrant.AccountId = toAccount.ID;
                        amsgs.AddRange(registrant.Save(adc));

                        registration.AccountId = toAccount.ID;
                        amsgs.AddRange(registration.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

        }
    }
}
