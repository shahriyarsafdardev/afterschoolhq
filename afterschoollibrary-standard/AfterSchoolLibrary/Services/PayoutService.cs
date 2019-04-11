using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class PayoutService
    {
        public static Payout CreateNew(Account account, PayoutStatus payoutStatus, decimal amount, PaymentType paymentType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var payout = new Payout()
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
                RequestedBy = loggedInUser.ID
            };

            msgs.AddRange(payout.Save(dc));

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
            if (payout.PayoutStatusId != PayoutStatus.Approved)
            {
                payout.UpdateStatus(PayoutStatus.Approved, loggedInUser, msgs, dc);                
                payout.SendMoney(loggedInUser, msgs, dc);

                // TODO: Send approved payout email
            }

            return;
        }

        private static void SendMoney(this Payout payout, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = payout.GetAccount(msgs, dc);

            if (payout.PaymentTypeId == PaymentType.ACH)
            {
                // send money via stripe
                var stripeAccount = account.GetStripeAccount(msgs, dc);
                if (stripeAccount.IsNotEmpty())
                    stripeAccount.SendPayout(
                        account: account,
                        payout: payout,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);

                if (msgs.HasNoErrors())
                    payout.UpdateStatus(PayoutStatus.Completed, loggedInUser, msgs, dc);

                return;
            }
            else if (payout.PaymentTypeId == PaymentType.Check)
            {
                // send money via lob
                // get check payment account
                var checkAccount = account.GetCheckPaymentAccount(msgs, dc);

                if (checkAccount.IsNotEmpty())
                    checkAccount.SendPayment(
                        account: account,
                        payout: payout,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);

                if (msgs.HasNoErrors())
                    payout.UpdateStatus(PayoutStatus.InProgress, loggedInUser, msgs, dc);

                return;
            }

            msgs.Add("No valid payment account", MessageType.UserError);

            return;
        }

        public static WebUser GetRequestor(this Payout payout, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.Load(payout.RequestedBy.Value, msgs, dc);
        }

        public static Account GetAccount(this Payout payout, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(payout.AccountId.Value, msgs, dc);
        }
    }
}
