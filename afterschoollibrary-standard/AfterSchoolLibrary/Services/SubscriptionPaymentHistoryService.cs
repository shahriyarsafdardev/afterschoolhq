using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
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
    }
}
