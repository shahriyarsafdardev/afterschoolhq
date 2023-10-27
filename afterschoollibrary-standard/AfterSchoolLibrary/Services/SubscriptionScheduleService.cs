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
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using Account = AfterSchoolLibrary.Models.Account;
using RestSharp.Extensions;
using Stripe.FinancialConnections;

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
                StartDate = startDateTime.Date,
                AmountInCents = (feeObject.Amount + feeObject.PaymentTypeFee) * 100,
                EndDate = endDateTime,
                FirstRenewal = firstRenewalDate,
                ProrateFirstCharge = checkoutObject.SubscriptionPlan.ProrateFirstCharge,
                ProrateLastCharge = checkoutObject.SubscriptionPlan.ProrateLastCharge,
                BillingPeriodType = feeObject.BillingPeriodType,
                StripeCustomerId = checkoutObject.PayingAccount.StripeCustomerId,
                StripeSourceId = checkoutObject.StripeSourceId,
                StripeConnectId = checkoutObject.BillingAccount.StripeUserId,
                AmountToConnectAccountInCents = (feeObject.Amount - feeObject.ConvenienceFee) * 100,
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
                    x.NextBillDate.HasValue &&
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
                    x.NextBillDate.HasValue &&
                    x.NextBillDate.Value.Date.Equals(date) &&
                    (x.BillingPeriodType == BillingPeriodType.Month || x.BillingPeriodType == BillingPeriodType.Year)
                )
                .ToList();
            return subscriptionSchedules;
        }

        public static List<DateTime> GetPaymentDates(DateTime startDate, DateTime firstRenewalDate, DateTime endDate, BillingPeriodType billingPeriodType)
        {
            List<DateTime> paymentDates = new List<DateTime>()
            {
                startDate
            };

            var currentPaymentDate = firstRenewalDate;

            while (currentPaymentDate.IsOnOrBefore(endDate))
            {
                paymentDates.Add(currentPaymentDate);

                currentPaymentDate = (DateTime) new SubscriptionPlan
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
                            firstRenewalDate = (DateTime) (schedule.IsOngoing
                                ? subscriptionPlan.StartDate
                                : participationObject.StartDate);
                        else
                            firstRenewalDate = (DateTime) (subscriptionPlan.StartDate > checkoutDateProviderTimeZone
                                ? subscriptionPlan.StartDate
                                : checkoutDateProviderTimeZone);
                    }
                    else
                        firstRenewalDate = checkoutDateProviderTimeZone;

                    if (firstRenewalDate.IsAfter(checkoutDateProviderTimeZone))
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
                    var currentPaymentDate = (DateTime) startDate;

                    while (currentPaymentDate.IsOnOrBefore((DateTime) (startDate > checkoutDateProviderTimeZone ? startDate : checkoutDateProviderTimeZone)))
                    {
                        currentPaymentDate = (DateTime) new SubscriptionPlan
                        {
                            BillingPeriodTypeId = billingPeriodType
                        }.CalculateEndDateFromNumberOfPeriods(currentPaymentDate, 1);
                    }

                    firstRenewalDate = currentPaymentDate;
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
            if (schedule.StartDate.Equals(DateTime.UtcNow.Date) && schedule.ProrateFirstCharge.HasValue && (schedule.ProrateFirstCharge ?? false))
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

                UpdateNextBillDateOnSuccess(subscriptionSchedule);
            }

            if (charge.Status.Equals(LastChargeStatus.failed))
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
                    subscriptionSchedule.DateCancelled = DateTime.UtcNow;
                    subscriptionSchedule.CancelledReason = "Payment for subscription failed";
                }
            }

            subscriptionSchedule.LastChargeStatus = chargeStatus;
            subscriptionPaymentHistory.Status = chargeStatus;

            msgs.AddRange(subscriptionSchedule.Save());
            msgs.AddRange(subscriptionPaymentHistory.Save());

            return msgs;
        }

        public static SystemMessages CreatePaymentRecordsAfterCharge(SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory,
            Charge charge, SystemMessages msgs, bool pendingCase = false, DatabaseConnection dc = null)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(subscriptionSchedule.Uid, msgs, dc);

            var plan = rsp.GetSubscriptionPlan(msgs);
            var registrant = rsp.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var purchasingAccount = masterRegistrant.GetAccount(msgs);
            var billingAccount = plan.GetAccount(msgs);
            var participationObject = plan.GetParticipationObject(msgs);
            var registrantActivity = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObject.ObjectID.Value,
                participationObject.ObjectType,
                registrant.ID.Value, msgs);
            var registration = registrantActivity.GetRegistration(msgs);

            Account convenienceFeePayer;
            Account creditCardFeePayer;

            creditCardFeePayer = billingAccount.IsCreditCardFeeTakenOutOfCost ? billingAccount : purchasingAccount;

            convenienceFeePayer = billingAccount.IsConvenienceFeeTakenOutOfCost
                   ? billingAccount
                   : purchasingAccount;

            var useConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, dc);
            var usePlatformFees = useConnectedAccount && subscriptionPaymentHistory.AmountInCents.IsEmpty();
            var platFormFlatFee = (!participationObject.IsRegistrationFree) ? billingAccount.GetPlatformConvenienceFlatFee(msgs, dc) : 0;
            var platformPercentFee = billingAccount.GetPlatformConveniencePercentFee(msgs, dc);

            var stripeSubscription = StripeHelper.GetStripeHelperConnectAccount().GetSubscription(rsp.StripeSubscriptionId, msgs);
            var stripePrice = stripeSubscription.Items.FirstOrDefault(item => item.Price.IsNotEmpty()).Price;
            var stripeSubscriptionPrice = (decimal) stripePrice.UnitAmount / 100;

            var paymentType = rsp.PaymentTypeId;
            var isAmountProrated = subscriptionPaymentHistory.ProratedCharge;

            // TODO: potentially problematic as the plan may have changed
            var convenienceFee = rsp.ConvenienceFee ?? plan.GetConvenienceFee(billingAccount, msgs);
            var paymentTypeFee = rsp.PaymentTypeFee.HasValue ? rsp.PaymentTypeFee : paymentType == PaymentType.CreditCard ? plan.GetCreditCardFee(billingAccount, msgs) : 0;
            var calculatedFullCCFee = StripeHelper.CalculateStripeCCFeeOutOfTotalCost(stripeSubscriptionPrice);

            if (isAmountProrated)
            {
                convenienceFee = SubscriptionScheduleService.GetProratedAmount(subscriptionSchedule, convenienceFee);
                paymentTypeFee = SubscriptionScheduleService.GetProratedAmount(subscriptionSchedule, (decimal) paymentTypeFee);
            }

            var amountPaidByParent = subscriptionPaymentHistory.AmountInCents / 100m;

            if (paymentType == PaymentType.ACH)
                stripeSubscriptionPrice -= calculatedFullCCFee;

            var amountReceivedByProvider = amountPaidByParent - (convenienceFee + paymentTypeFee);

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                OverrideAmount = amountPaidByParent,
                AmountPaidByParent = amountPaidByParent,
                AmountReceivedByProvider = amountReceivedByProvider,
                IsAmountProRated = isAmountProrated,
                ConvenienceFeeOverrideAmount = convenienceFee,
                PaymentTypeFeeOverrideAmount = paymentTypeFee,
                ParticipationObject = participationObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = rsp.PaymentTypeId,
                StripeChargeId = charge.Id,
                SubscriptionPlan = plan.PlanTypeId == SubscriptionPlanType.Subscription ? plan : null,
                PaymentPlan = plan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? plan : null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                RegistrantSubscriptionPlan = rsp,
                ConvenienceFeePayer = convenienceFeePayer,
                CreditCardFeePayer = creditCardFeePayer,
                IsSubscriptionPayment = true,
            };

            if (chargeStatus.Equals(LastChargeStatus.succeeded))
            {
                billingAccount.ProcessSubscriptionPlanPayment(checkoutObject, plan, rsp, msgs);

                //transfer to provider stripe account
                if (participationObject.PaymentAccount.StripeUserId.HasValue())
                    checkoutObject.Transaction.CreateStripeTransfer(WebUser.Load(1, msgs), msgs);

                if (msgs.HasNoErrors())
                    checkoutObject.PayingAccount.SendSubscriptionPaymentSucceeded(checkoutObject);
                else
                    msgs.Add("Error creating payment records", MessageType.Error);
            }
            else
            {
                if (chargeStatus.Equals(LastChargeStatus.failed))
                {
                    if(pendingCase)
                    {
                        //creating negative transaction if pending charge status was updated to failed
                        TransactionService.CreateNew(
                        purchasingAccount,
                        billingAccount,
                        convenienceFeePayer,
                        creditCardFeePayer,
                        -amountPaidByParent,
                        participationObject?.Name,
                        null, // as we have'nt created purchase yet.
                        TransactionType.Charge,
                        rsp.PaymentTypeId,
                        PaymentSourceType.AfterSchoolPlatform,
                        usePlatformFees ? platFormFlatFee : new decimal?(),
                        usePlatformFees ? platformPercentFee : new decimal?(),
                        convenienceFee,
                        paymentTypeFee ?? 0,
                        subscriptionPaymentHistory.StripeChargeId,
                        subscriptionSchedule.Description,
                        DateTime.UtcNow,
                        useConnectedAccount,
                        null,
                        null,
                        null,
                        msgs,
                        dc);

                        if(msgs.HasErrors())
                            msgs.Add("Error creating negative transaction record", MessageType.Error);
                    }
                    if (msgs.HasNoErrors())
                        checkoutObject.PayingAccount.SendSubscriptionPaymentFailed(checkoutObject.SubscriptionPlan, dc);                        
                }
                else
                    EmailHelper.SendUpcomingSubscriptionPaymentReminder(subscriptionSchedule, true);
            }
            return msgs;
        }


        private static void UpdateNextBillDateOnSuccess(SubscriptionSchedule subscriptionSchedule)
        {
            // was first payment 
            if (subscriptionSchedule.PaymentsDone.Equals(1))
                subscriptionSchedule.NextBillDate = subscriptionSchedule.FirstRenewal;
            //was last payment
            else if (subscriptionSchedule.NextBillDate.Value.Date.Equals(subscriptionSchedule.LastDateToBill.Value.Date))
            {
                subscriptionSchedule.NextBillDate = null;
                subscriptionSchedule.IsCancelled = true;
                subscriptionSchedule.DateCancelled = subscriptionSchedule.LastDateToBill;
                subscriptionSchedule.CancelledReason = "Subscription Completed";
            }
            else
                subscriptionSchedule.NextBillDate = new SubscriptionPlan
                {
                    BillingPeriodTypeId = subscriptionSchedule.BillingPeriodType
                }.CalculateEndDateFromNumberOfPeriods((DateTime) subscriptionSchedule.NextBillDate, 1);
        }
    }
}
