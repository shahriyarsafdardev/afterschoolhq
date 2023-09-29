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
        //public static SubscriptionSchedule CreateNew(CheckoutObject checkoutObject, DateTime? startDateTime, PlanFeeObject feeObject, Purchase purchase, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();
        //    var subscriptionSchedule = new SubscriptionSchedule()
        //    {
        //        CreateDate = DateTime.UtcNow,
        //        CreatedBy = loggedInUser?.ID.IfEmpty(0),
        //        LastModifiedDate = DateTime.UtcNow,
        //        LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
        //        StartDate = startDateTime.IfEmpty(DateTime.UtcNow),
        //        AmountInCents = (decimal) purchase.AmountPaid,
        //        EndDate = checkoutObject.SubscriptionPlan.EndDate, //calculate and also can be null
        //        FirstRenewal = firstRenewal,//????
        //        ProrateFirstCharge = false, //FE
        //        ProrateLastCharge = false, //FE
        //        BillingPeriodType = feeObject.BillingPeriodType,
        //        StripeCustomerId = checkoutObject.PayingAccount.StripeCustomerId,
        //        StripeSourceId = checkoutObject.StripeSourceId,
        //        LastChargeStatus = lastChargeStatus, //run the charge and then pass the status
        //        StripeConnectId = checkoutObject.BillingAccount.GetStripeConnectedAccount(msgs).Id,
        //        AmountToConnectAccountInCents = amountToConnectAccountInCents,
        //        LastDateToBill = lastDateToBill,
        //        Description = "description", //???
        //        NextBillDate = nextBillDate, //
        //        LastChargeFailed = lastChargeFailed,
        //        CancelledReason = "reason",
        //        Metadata = "metadata", //?????
        //    };
        //    msgs.AddRange(subscriptionSchedule.Save(dc));
        //    return subscriptionSchedule;
        //}

        public static SubscriptionSchedule CreateNew(CheckoutObject checkoutObject, DateTime? startDateTime, PlanFeeObject feeObject,
            DateTime firstRenewalDate, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            //remove hard coded values
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var subscriptionSchedule = new SubscriptionSchedule()
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                StartDate = startDateTime.IfEmpty(DateTime.UtcNow),
                AmountInCents = feeObject.Amount,
                EndDate = DateTime.UtcNow, //calculate and also can be null
                FirstRenewal = firstRenewalDate,
                ProrateFirstCharge = (bool)checkoutObject.SubscriptionPlan.ProrateFirstCharge, 
                ProrateLastCharge = (bool) checkoutObject.SubscriptionPlan.ProrateLastCharge,
                BillingPeriodType = feeObject.BillingPeriodType,
                StripeCustomerId = checkoutObject.PayingAccount.StripeCustomerId,
                StripeSourceId = checkoutObject.StripeSourceId,
                LastChargeStatus = LastChargeStatus.succeeded, //run the charge and then pass the status
                StripeConnectId = "scid123",
                AmountToConnectAccountInCents = feeObject.Amount - feeObject.ConvenienceFee,
                LastDateToBill = DateTime.UtcNow,
                Description = "description", //???
                NextBillDate = firstRenewalDate,
                LastChargeFailed = false,
                CancelledReason = "reason",
                Metadata = "metadata", //?????
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

                currentPaymentDate = billingPeriodType switch
                {
                    BillingPeriodType.Week => currentPaymentDate.AddDays(7),
                    BillingPeriodType.Month => currentPaymentDate.AddMonths(1),
                    BillingPeriodType.Year => currentPaymentDate.AddYears(1),
                    _ => throw new ArgumentException("Invalid subscription interval specified.")
                };
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
                if (subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnCheckout ||
                    (subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart && schedule == null) ||
                     subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart && schedule != null && schedule.IsOngoing)
                {
                    if (subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart)
                        firstRenewalDate = (DateTime) (activity.HasSchedule ? schedule.IsOngoing ? subscriptionPlan.StartDate : participationObject.StartDate : checkoutDateProviderTimeZone);
                    else
                        firstRenewalDate = checkoutDateProviderTimeZone;
                    while (firstRenewalDate.IsOnOrBefore(checkoutDateProviderTimeZone))
                    {
                        firstRenewalDate = billingPeriodType switch
                        {
                            BillingPeriodType.Week => firstRenewalDate.AddDays(7),
                            BillingPeriodType.Month => firstRenewalDate.AddMonths(1),
                            BillingPeriodType.Year => firstRenewalDate.AddYears(1),
                            _ => throw new ArgumentException("Invalid subscription interval specified.")
                        };
                    }
                }
                else
                {
                    var startDate = subscriptionPlan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart ? subscriptionPlan.StartDate : participationObject.StartDate;
                    var chargeDates = GetPaymentDates((DateTime) startDate, (DateTime) endDate, billingPeriodType);
                    firstRenewalDate = chargeDates.FirstOrDefault(chargeDate => chargeDate.IsAfter(checkoutDateProviderTimeZone));
                }
            }
            else
                msgs.Add("No subscription plan found", MessageType.Error);

            return firstRenewalDate;
        }

        public static decimal CalculateProratedAmount(DateTime nextChargeDate, DateTime purchaseDate, decimal fullChargeAmount)
        {
            var diffInDays = (nextChargeDate.Date - purchaseDate.Date).TotalDays;
            var currentMonthTotalDays = DateTime.DaysInMonth(nextChargeDate.Year, nextChargeDate.Month);
            return (diffInDays / currentMonthTotalDays).ToDecimal() * fullChargeAmount;
        }
    }
}
