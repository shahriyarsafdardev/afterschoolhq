using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Stripe;
using Newtonsoft.Json;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;

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
                StripeConnectId = checkoutObject.BillingAccount.StripeUserId,
                AmountToConnectAccountInCents = feeObject.Amount - feeObject.ConvenienceFee,
                LastDateToBill = lastDateToBill,
                Description = "Subscription for " + checkoutObject.ParticipationObject.Name,
                NextBillDate = nextBillDate,
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

        public static Decimal GetProratedAmount(SubscriptionSchedule schedule, Decimal amount)
        {
            //if this is last payment
            if (schedule.LastDateToBill.HasValue && schedule.NextBillDate.Equals(schedule.LastDateToBill) && (schedule.ProrateLastCharge ?? false))
                return CalculateProratedAmount((DateTime) schedule.LastDateToBill, DateTime.UtcNow, amount);

            //if this is first payment
            if (schedule.StartDate.Equals(DateTime.UtcNow) && schedule.ProrateFirstCharge.HasValue && (schedule.ProrateFirstCharge ?? false))
                return CalculateProratedAmount((DateTime) schedule.FirstRenewal, DateTime.UtcNow, amount);

            return amount;
        }

        public static SystemMessages UpdateSubscriptionAndPaymentRecordsAfterCharge(Charge charge, SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);

            if (chargeStatus.Equals(LastChargeStatus.succeeded))
            {
                subscriptionSchedule.PaymentsDone += 1;

                subscriptionSchedule.Metadata = JsonConvert.SerializeObject(charge.Metadata);

                //UpdateNextBillDateOnSuccess(subscriptionSchedule);

                if (subscriptionPaymentHistory.Status.Equals(LastChargeStatus.failed))
                    subscriptionPaymentHistory.Status = LastChargeStatus.succeeded;
            }
            else if (charge.Status.Equals(LastChargeStatus.failed))
            {
                msgs.Add("Charge failed", MessageType.Error);

                if (subscriptionPaymentHistory.RetryNumber < 3)
                {
                    subscriptionSchedule.LastChargeStatus = LastChargeStatus.failed;
                    subscriptionPaymentHistory.RetryNumber += 1;
                }
                else
                {
                    subscriptionSchedule.IsCancelled = true;
                    subscriptionSchedule.NextBillDate = null;
                    subscriptionSchedule.EndDate = DateTime.UtcNow;
                    subscriptionSchedule.CancelledReason = "Payment for subscription failed";
                }
            }
            else
            {
                subscriptionPaymentHistory.Status = LastChargeStatus.pending;
            }

            msgs.AddRange(subscriptionSchedule.Save());
            msgs.AddRange(subscriptionPaymentHistory.Save());

            return msgs;
        }

        private static void UpdateNextBillDateOnSuccess(SubscriptionSchedule subscriptionSchedule)
        {
            // was first payment 
            if (subscriptionSchedule.PaymentsDone.Equals(1))
                subscriptionSchedule.NextBillDate = subscriptionSchedule.FirstRenewal;

            //was last payment
            else if (subscriptionSchedule.NextBillDate.Value.Date.Equals(subscriptionSchedule.LastDateToBill.Value.Date))
                subscriptionSchedule.NextBillDate = null;

            else
                subscriptionSchedule.NextBillDate = new SubscriptionPlan
                {
                    BillingPeriodTypeId = subscriptionSchedule.BillingPeriodType
                }.CalculateEndDateFromNumberOfPeriods((DateTime) subscriptionSchedule.NextBillDate, 1);
        }
    }
}
