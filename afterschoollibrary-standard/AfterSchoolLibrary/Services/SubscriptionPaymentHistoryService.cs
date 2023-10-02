using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public class SubscriptionPaymentHistoryService
    {
        public static SubscriptionPaymentHistory CreateNew(int subscriptionScheduleId, decimal amountInCents, DateTime dateCharged, string stripeChargeId,
            bool proratedCharge, LastChargeStatus status, string stripeRawResponse, decimal amountToConnectAccountInCents, decimal amountToStripeInCents, int retryNumber,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var subscriptionPaymentHistory = new SubscriptionPaymentHistory()
            {
                SubscriptionScheduleId = subscriptionScheduleId,
                AmountInCents = amountInCents,
                DateCharged = dateCharged,
                StripeChargeId = stripeChargeId,
                ProratedCharge = proratedCharge,
                Status = status,
                StripeRawResponse = stripeRawResponse,
                AmountToConnectAccountInCents = amountToConnectAccountInCents,
                AmountToStripeInCents = amountToStripeInCents,
                RetryNumber = retryNumber
            };
            msgs.AddRange(subscriptionPaymentHistory.Save(dc));

            return subscriptionPaymentHistory;
        }

        public static List<SubscriptionPaymentHistory> GetPendingCharges(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var subscriptionPaymentHistoryPendingStatus = SubscriptionPaymentHistory.LoadByStatus(LastChargeStatus.pending, msgs, null);

            var targetDate = DateTime.UtcNow.AddDays(-5);

            //Retrieve the pending records for which 5 days have passed since the charge
            var filteredPaymentHistories = subscriptionPaymentHistoryPendingStatus
                .Where(paymentHistory =>
                    paymentHistory.DateCharged.Date.Equals(targetDate.Date))
                .ToList();

            return filteredPaymentHistories;
        }
    }
}
