
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class PayoutService
    {
        public static Payout CreateNew(Account account, string statementDescriptor, List<Transaction> transactions, PayoutStatus payoutStatus, PaymentType paymentType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Payout payout = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var amount = transactions.Sum(transaction => transaction.GetBillerPayout(amsgs, adc));

                    payout = new Payout()
                    {
                        AccountId = account.ID,
                        Amount = amount,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUser?.ID.IfEmpty(0),
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                        PaymentTypeId = paymentType,
                        PayoutStatusId = payoutStatus,
                        DateRequested = DateTime.UtcNow,
                        RequestedBy = loggedInUser?.ID.IfEmpty(0),
                        StatementDescriptor = statementDescriptor
                    };

                    amsgs.AddRange(payout.Save(adc));

                    payout.AddTransactions(transactions, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return payout;
        }

        public static void UpdateStatus(this Payout payout, PayoutStatus payoutStatus, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            payout.PayoutStatusId = payoutStatus;
            payout.LastModifiedDate = DateTime.UtcNow;
            payout.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(payout.Save(dc));
        }

        public static void Approve(this Payout payout, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (payout.PayoutStatusId == PayoutStatus.Approved)
                return;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    payout.UpdateStatus(PayoutStatus.Approved, loggedInUser, amsgs, adc);
                    payout.SendMoney(loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => 
                {
                    msgs.AddRange(smsgs);
                    // TODO: Send approved payout email
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void Deny(this Payout payout, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (payout.PayoutStatusId == PayoutStatus.Denied)
                return;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    payout.UpdateStatus(PayoutStatus.Denied, loggedInUser, msgs, dc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    // TODO: Send Denied payout email

                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void Complete(this Payout payout, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (payout.PayoutStatusId == PayoutStatus.Completed)
                return;

            Database.ExecuteTransaction
           (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   payout.UpdateStatus(PayoutStatus.Completed, loggedInUser, msgs, dc);
               },
               onSuccess: (SystemMessages smsgs) =>
               {
                   msgs.AddRange(smsgs);
                    // TODO: Send completed payout email

                },
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
           );

        }

        public static void DeleteAndUpdateTransactions(this Payout payout, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (payout.IsEmpty())
                return;

            msgs.AddRange(payout.Delete(dc)); // this is hard deleting, but if we're deleting then this is a mistake anyway

            if (msgs.HasNoErrors())
                Transaction.LoadByPayoutIdByIsDeleted(payout.ID.Value, false, msgs, dc).ForEach(transaction => {
                    transaction.PayoutId = null;
                    transaction.LastModifiedBy = loggedInUser.ID;
                    transaction.LastModifiedDate = DateTime.UtcNow;
                    msgs.AddRange(transaction.Save(dc));
                });
        }

        private static void SendMoney(this Payout payout, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (payout.PayoutStatusId != PayoutStatus.Approved)
                return;

            var payoutAccount = payout.GetAccount(msgs, dc);

            if (payout.PaymentTypeId == PaymentType.ACH)
            {
                // send money via ACH
                if (payoutAccount.CanReceiveACHDeposits(msgs, dc))
                {
                    if (payoutAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc) && payoutAccount.HasStripeConnectedAccount)
                    {
                        payoutAccount.SendStripePayout(
                            payout: payout,
                            loggedInUser: loggedInUser,
                            msgs: msgs,
                            dc: dc);

                        if (msgs.HasNoErrors())
                            payout.UpdateStatus(PayoutStatus.Completed, loggedInUser, msgs, dc);
                    }
                    // transfer money to the main AHQ account to prep for sending an ACH
                    else
                    {
                        payout.MoveMoneyInStripeToAHQBankAccount(payoutAccount, loggedInUser, msgs, dc);

                        if (msgs.HasNoErrors())
                            payout.UpdateStatus(PayoutStatus.InProgress, loggedInUser, msgs, dc);
                    }
                }
                return;
            }
            else if (payout.PaymentTypeId == PaymentType.Check)
            {
                // send money via lob
                // get check payment account
                var checkAccount = payoutAccount.GetCheckPaymentAccount(msgs, dc);

                // lob only validates addresses on production
                if (Helpers.Utilities.IsProduction && !checkAccount.IsDeliverable(msgs, dc))
                    msgs.Add("Your address is invalid and not deliverable. Please update your address to a valid address", MessageType.UserError);

                if (msgs.HasNoErrors())
                    checkAccount.SendPayment(
                        account: payoutAccount,
                        payout: payout,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);

                if (msgs.HasNoErrors())
                {
                    payout.MoveMoneyInStripeToAHQBankAccount(payoutAccount, loggedInUser, msgs, dc);
                    payout.UpdateStatus(PayoutStatus.Completed, loggedInUser, msgs, dc);
                }

                return;
            }

            msgs.Add("No valid payment account", MessageType.UserError);            
        }

        public static void MoveMoneyInStripeToAHQBankAccount(this Payout payout, Account payoutAccount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Billing.StripeHelper.CreatePayout(
                amount: payout.Amount.Value,
                externalAccountId: null,
                connectedAccountId: null,
                statementDescriptor: payout.StatementDescriptor.IsNotEmpty() ? payout.StatementDescriptor : "AHQ Payout",
                metaData: new Dictionary<string, string>()
                {
                    { "AccountUid", payoutAccount.Uid.ToString() },
                    { "RequestedBy", payout.GetRequestor(msgs, dc).Uid.ToString() },
                    { "ApprovedBy", loggedInUser.Uid.ToString() },
                    { "DateRequested", payout.DateRequested.ToShortDateTimeString() }
                },
                msgs: msgs);
        }

        public static WebUser GetRequestor(this Payout payout, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.Load(payout.RequestedBy.Value, msgs, dc);
        }

        public static Account GetAccount(this Payout payout, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(payout.AccountId.Value, msgs, dc);
        }

        public static List<Transaction> GetTransactions(this Payout payout, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayoutIdByIsDeleted(payout.ID.Value, false, msgs, dc);
        }

        public static void AddTransactions(this Payout payout, List<Transaction> transactions, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            transactions.ForEach(transaction => payout.AddTransaction(transaction, loggedInUser, msgs, dc));
        }

        public static void AddTransaction(this Payout payout, Transaction transaction, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            transaction.PayoutId = payout.ID.Value;
            transaction.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            transaction.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(transaction.Save(dc));
        }

        public static void SetExternalPayoutId(this Payout payout, string externalPayoutId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            payout.ExternalPayoutId = externalPayoutId;
            payout.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            payout.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(payout.Save(dc));
        }

        public static void SendPayoutRequestNotification(this Payout payout, Account toAccount, SystemMessages msgs)
        {
            msgs.AddRange(Helpers.EmailHelper.SendPayoutRequestNotification(payout, toAccount));
        }
    }
}
