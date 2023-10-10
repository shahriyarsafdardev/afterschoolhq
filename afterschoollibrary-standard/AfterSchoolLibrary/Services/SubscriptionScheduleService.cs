using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionScheduleService
    {
        public static SubscriptionSchedule CreateNew(CheckoutObject checkoutObject, DateTime startDateTime, DateTime? endDateTime,
            DateTime? nextBillDate, DateTime? lastDateToBill, PlanFeeObject feeObject, DateTime? firstRenewalDate, 
            string metaData, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // TODO: remove hard coded values
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var subscriptionSchedule = new SubscriptionSchedule()
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                StartDate = startDateTime,
                AmountInCents = feeObject.Amount,
                EndDate = endDateTime,
                FirstRenewal = firstRenewalDate,
                ProrateFirstCharge = checkoutObject.SubscriptionPlan.ProrateFirstCharge,
                ProrateLastCharge = checkoutObject.SubscriptionPlan.ProrateLastCharge,
                BillingPeriodType = feeObject.BillingPeriodType,
                StripeCustomerId = checkoutObject.PayingAccount.StripeCustomerId,
                StripeSourceId = checkoutObject.StripeSourceId,
                LastChargeStatus = LastChargeStatus.succeeded, //run the charge and then pass the status
                StripeConnectId = "scid123",
                AmountToConnectAccountInCents = feeObject.Amount - feeObject.ConvenienceFee,
                LastDateToBill = lastDateToBill,
                Description = "description", //???
                NextBillDate = nextBillDate,
                LastChargeFailed = false,
                CancelledReason = "reason",
                Metadata = metaData,
            };
            msgs.AddRange(subscriptionSchedule.Save(dc));
            return subscriptionSchedule;
        }

        public static List<SubscriptionSchedule> GetSubscriptionSchedulePendingToday(SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var subscriptionSchedulesNotCancelled = SubscriptionSchedule.LoadActiveSubscriptionSchedule(msgs, dc);
            var currentDate = DateTime.UtcNow.Date;
            var subscriptionSchedulespendingToday = subscriptionSchedulesNotCancelled
               .Where(x =>
                    x.NextBillDate.Value.Date.Equals(currentDate)
                )
                .ToList();

            return subscriptionSchedulespendingToday;
        }

        public static List<SubscriptionSchedule> GetFailedSubscriptionSchedules(SystemMessages msgs,
          DatabaseConnection dc = null)
        {
            var subscriptionPaymentsFailed = SubscriptionPaymentHistory.LoadByStatus(LastChargeStatus.failed, msgs, dc);
            var subscriptionSchedulesNotCancelled = SubscriptionSchedule.LoadActiveSubscriptionSchedule(msgs, dc);
            var currentDate = DateTime.UtcNow.Date;

            var subscriptionSchedulesFailed = subscriptionSchedulesNotCancelled
                .Where(schedule =>
                   subscriptionPaymentsFailed.Any(paymentFailed =>
                   paymentFailed.SubscriptionScheduleId == schedule.ID &&
                    schedule.NextBillDate.Value.Date == currentDate.AddDays(-paymentFailed.RetryNumber) &&
                    paymentFailed.RetryNumber < 3)
                )
                .ToList(); 

            return subscriptionSchedulesFailed;
        }

        public static List<SubscriptionSchedule> GetSubscriptionSchedulePendingInThreeDays(SystemMessages msgs,
           DatabaseConnection dc = null)
        {
            var subscriptionSchedulesNotCancelled = SubscriptionSchedule.LoadActiveSubscriptionSchedule(msgs, dc);
            var date = DateTime.UtcNow.AddDays(3).Date;
            var subscriptionSchedules = subscriptionSchedulesNotCancelled
               .Where(x =>
                    x.NextBillDate.Value.Date.Equals(date) &&
                    (x.BillingPeriodType == BillingPeriodType.Month || x.BillingPeriodType == BillingPeriodType.Year)
                )
                .ToList();
            return subscriptionSchedules;
        }

        public static List<DateTime> GetPaymentDates(DateTime startDate, DateTime endDate, BillingPeriodType billingPeriodType)
        {
            List<DateTime> paymentDates = new List<DateTime>();

            var currentPaymentDate = startDate;

            while (currentPaymentDate.IsOnOrBefore(endDate))
            {
                paymentDates.Add(currentPaymentDate);

                currentPaymentDate = (DateTime)new SubscriptionPlan
                {
                    BillingPeriodTypeId = billingPeriodType
                }.CalculateEndDateFromNumberOfPeriods(currentPaymentDate, 1);
            }

            return paymentDates;
        }

        public static DateTime GetFirstRenewalDate(ParticipationObject participationObject, DateTime? endDate, BillingPeriodType billingPeriodType)
        {
            var msgs = new SystemMessages();
            var checkoutDateProviderTimeZone = participationObject.OwnerAccount.ConvertToAccountTimeZone(DateTime.UtcNow).Date;
            var firstRenewalDate = new DateTime();
            var activity = participationObject.GetActivity();
            var schedule = activity.GetSchedule();
            if (activity?.GetSubscriptionPlan() != null)
            {
                var subscriptionPlan = participationObject.GetActivity()?.GetSubscriptionPlan();
                if (subscriptionPlan.PaymentStartTypeId.Equals(PlanPaymentStartType.OnCheckout) ||
                    (subscriptionPlan.PaymentStartTypeId.Equals(PlanPaymentStartType.OnPeriodStart) && schedule.IsEmpty()) ||
                     subscriptionPlan.PaymentStartTypeId.Equals(PlanPaymentStartType.OnPeriodStart) && schedule.IsNotEmpty() && schedule.IsOngoing)
                {
                    if (subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart)
                    {
                        if (activity.HasSchedule)
                            firstRenewalDate = (DateTime)(schedule.IsOngoing
                                ? subscriptionPlan.StartDate
                                : participationObject.StartDate);
                        else
                            firstRenewalDate = (DateTime) (subscriptionPlan.StartDate > checkoutDateProviderTimeZone
                                ? subscriptionPlan.StartDate
                                : checkoutDateProviderTimeZone);
                    }
                    else
                        firstRenewalDate = checkoutDateProviderTimeZone;

                    if(firstRenewalDate.IsAfter(checkoutDateProviderTimeZone))
                        firstRenewalDate = (DateTime) new SubscriptionPlan
                        {
                            BillingPeriodTypeId = billingPeriodType
                        }.CalculateEndDateFromNumberOfPeriods(firstRenewalDate, 1);
                    else
                    {
                        while (firstRenewalDate.IsOnOrBefore(checkoutDateProviderTimeZone))
                        {
                            firstRenewalDate = (DateTime) new SubscriptionPlan
                            {
                                BillingPeriodTypeId = billingPeriodType
                            }.CalculateEndDateFromNumberOfPeriods(firstRenewalDate, 1);
                        }
                    }
                }
                else
                {
                    var startDate = subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart ? subscriptionPlan.StartDate : participationObject.StartDate;
                    var chargeDates = GetPaymentDates((DateTime) startDate, (DateTime) endDate, billingPeriodType);
                    firstRenewalDate = chargeDates.FirstOrDefault(chargeDate => chargeDate.IsAfter((DateTime)(startDate > checkoutDateProviderTimeZone ? startDate : checkoutDateProviderTimeZone)));
                }
            }
            else
                msgs.Add("No subscription plan found", MessageType.Error);

            return firstRenewalDate;
        }

        public static decimal CalculateProratedAmount(DateTime nextChargeDate, DateTime purchaseDate, decimal fullChargeAmount)
        {
            var diffInDays = (nextChargeDate.Date - purchaseDate.Date).TotalDays;
            var currentMonthTotalDays = DateTime.DaysInMonth(purchaseDate.Year, purchaseDate.Month);
            return (diffInDays / currentMonthTotalDays).ToDecimal() * fullChargeAmount;
        }
    }
}
