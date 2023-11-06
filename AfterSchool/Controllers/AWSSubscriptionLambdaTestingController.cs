using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR.Json;
using Newtonsoft.Json;
using RestSharp.Extensions;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;
using SubscriptionScheduleService = AfterSchoolLibrary.Services.SubscriptionScheduleService;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public class AWSSubscriptionLambdaTestingController : Controller
    {
        [HttpGet]
        public String Lambda(DateTime paymentDate, int status = 1, bool retryFailedPayment = false, bool pendingCase = false)
        {
            SystemMessages msgs = new SystemMessages();

            if (status.NotIn(new List<int> { 1, 2, 3 }))
                return "Invalid Status";

            //get lastest schedule and set its nextbilldate to the date passed to lambda
            var schedule = SubscriptionSchedule.LoadLatestSubscriptionSchedule(msgs);

            if (schedule.IsCancelled ?? false)
                return "Subscription payments are completed/cancelled";

            schedule.NextBillDate = paymentDate;

            if (retryFailedPayment)
                ProcessFailedSubscriptionSchedules(schedule, paymentDate, status, msgs);
            else if (pendingCase)
                ProcessPaymentHistoryWithPendingStatus(schedule, status, paymentDate, msgs);
            else
                ProcessSubscriptionSchedulesPendingToday(schedule, paymentDate.Date, status, msgs);
            
            return JsonConvert.SerializeObject(schedule);
        }

        private void ProcessSubscriptionSchedulesPendingToday(SubscriptionSchedule schedule, DateTime dateToday, int status, SystemMessages msgs)
        {
            if (schedule.StripeSourceId.HasValue())
            {
                var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);

                var amount = schedule.AmountInCents / 100.0m;

                amount = GetProratedAmount(schedule, amount, dateToday);

                var charge = CreateCharge(schedule, rsp, amount, msgs);
                if (status.Equals(2))
                    charge.Status = LastChargeStatus.failed.ToString();
                else if(status.Equals(3))
                    charge.Status = LastChargeStatus.pending.ToString();

                if (charge.IsEmpty())
                    msgs.Add("Error in running the charge", MessageType.Error);
                else
                    ProcessCharge(charge, schedule, null, rsp, dateToday, msgs);
            }
            else
                msgs.Add("StripeSourceId is empty", MessageType.Error);
        }

        private void ProcessFailedSubscriptionSchedules(SubscriptionSchedule schedule, DateTime dateToday, int status, SystemMessages msgs)
        {
            if (schedule.StripeSourceId.HasValue())
            {
                var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);

                var amount = schedule.AmountInCents / 100.0m;

                amount = GetProratedAmount(schedule, amount, dateToday);

                var charge = CreateCharge(schedule, rsp, amount, msgs);

                if (status.Equals(2))
                    charge.Status = LastChargeStatus.failed.ToString();
                else if (status.Equals(3))
                    charge.Status = LastChargeStatus.pending.ToString();

                var subscriptionPaymentHistory = SubscriptionPaymentHistory.LoadBySubscriptionScheduleId((int) schedule.ID, msgs, null);

                if (charge.IsEmpty())
                    msgs.Add("Error in running the charge", MessageType.Error);

                else
                    ProcessCharge(charge, schedule, subscriptionPaymentHistory, rsp, dateToday, msgs);
            }
            else
                msgs.Add("Can not run the charge. StripeSourceId is empty", MessageType.Error);
        }

        private void ProcessPaymentHistoryWithPendingStatus(SubscriptionSchedule schedule, int status, DateTime dateToday, SystemMessages msgs)
        {
            var paymentHistory = SubscriptionPaymentHistory.LoadBySubscriptionScheduleId((int)schedule.ID, msgs, null);
            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);
            var charge = StripeHelper.GetStripeHelperConnectAccount().GetCharge(paymentHistory.StripeChargeId, msgs);

            if (status.Equals(2))
                charge.Status = LastChargeStatus.failed.ToString();
            else if (status.Equals(3))
                charge.Status = LastChargeStatus.pending.ToString();

            if (charge.IsEmpty())
                msgs.Add("Error in retrieving the charge", MessageType.Error);

            else
                ProcessCharge(charge, schedule, paymentHistory, rsp, dateToday, msgs, true);
        }

        public static Decimal GetProratedAmount(SubscriptionSchedule schedule, Decimal amount, DateTime dateToday)
        {
            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid);
            var account = rsp.GetSubscriptionPlan().GetAccount();

            //if this is last payment
            if (schedule.LastDateToBill.HasValue && (schedule.NextBillDate.IsEmpty() || schedule.NextBillDate.Equals(schedule.LastDateToBill))
                && (schedule.ProrateLastCharge ?? false))
                return CalculateProratedAmount((DateTime) schedule.LastDateToBill, account.ConvertToAccountTimeZone(dateToday), amount);

            //if this is first payment
            if (schedule.StartDate.Equals(dateToday) && schedule.ProrateFirstCharge.HasValue && (schedule.ProrateFirstCharge ?? false))
                return CalculateProratedAmount((DateTime) schedule.FirstRenewal, account.ConvertToAccountTimeZone(dateToday), amount);

            return amount;
        }

        public static decimal CalculateProratedAmount(DateTime nextChargeDate, DateTime purchaseDate, decimal fullChargeAmount)
        {
            var diffInDays = (nextChargeDate.Date - purchaseDate.Date).TotalDays;
            var currentMonthTotalDays = DateTime.DaysInMonth(purchaseDate.Year, purchaseDate.Month);
            return (diffInDays / currentMonthTotalDays).ToDecimal() * fullChargeAmount;
        }

        private Charge CreateCharge(SubscriptionSchedule schedule, RegistrantSubscriptionPlan rsp, decimal amount, SystemMessages msgs)
        {
            var billingAccount = rsp.GetSubscriptionPlan().GetActivity().GetParticipationObject().PaymentAccount;
            var useStripeConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, null);
            var metaData = JsonConvert.DeserializeObject<Dictionary<string, string>>(schedule.Metadata);

            var paymentNumber = SubscriptionPaymentHistory.GetCountBySubscriptionScheduleIdByStatus((int) schedule.ID,
                LastChargeStatus.succeeded, msgs) + 1;

            metaData["PaymentNumber"] = paymentNumber.ToString();

            if (paymentNumber > 1)
            {
                metaData["WasProrated"] = schedule.NextBillDate.Equals(schedule.LastDateToBill)
                    ? schedule.ProrateLastCharge.ToString()
                    : "false";
            }
            else
                metaData["WasProrated"] = schedule.ProrateFirstCharge.ToString() ?? "false";

            // TODO: Platform fees
            var platformFeePercent = useStripeConnectedAccount
                                        ? billingAccount.GetSubscriptionPlatformFeePercent(rsp.GetSubscriptionPlan(), rsp.PaymentTypeId, msgs)
                                        : new decimal?();

            // Create a charge for the current schedule
            return StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                fromCustomerId: schedule.StripeCustomerId,
                destinationAccountId: useStripeConnectedAccount ? billingAccount.StripeUserId : null,
                platformFee: useStripeConnectedAccount ? platformFeePercent : new decimal?(),
                tokenOrSourceId: schedule.StripeSourceId,
                totalAmount: amount,
                description: schedule.Description,
                statementDescriptor: "AHQ Subscription charge",
                metaData: metaData,
                msgs: msgs
            );
        }

        private void ProcessCharge(Charge charge, SubscriptionSchedule schedule, SubscriptionPaymentHistory? subscriptionPaymentHistory,
            RegistrantSubscriptionPlan rsp, DateTime dateToday, SystemMessages msgs, bool pendingCase = false)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
            var billingAccount = rsp.GetSubscriptionPlan().GetActivity().GetParticipationObject().PaymentAccount;
            var amountToStripe = StripeHelper.CalculateStripeFee(schedule.AmountInCents, billingAccount.IsCreditCardFeeTakenOutOfCost);
            var amountToConnectAccount = GetProratedAmount(schedule, schedule.AmountToConnectAccountInCents, dateToday);

            if (subscriptionPaymentHistory.IsEmpty())
            {
                subscriptionPaymentHistory = SubscriptionPaymentHistoryService.CreateNew(
                schedule.ID.Value,
                charge.Amount,
                DateTime.UtcNow,
                charge.Id,
                charge.Metadata["WasProrated"].ToBoolean(),
                chargeStatus,
                charge.StripeResponse.ToString(),
                amountToConnectAccount,
                amountToStripe,
                0
                );
            }

            msgs.AddRange(SubscriptionScheduleService.UpdateSubscriptionAndPaymentRecordsAfterCharge(charge, schedule, subscriptionPaymentHistory, msgs));
            msgs.AddRange(CreatePaymentRecordsAfterCharge(schedule, subscriptionPaymentHistory, charge, dateToday, msgs, pendingCase));
        }

        private SystemMessages CreatePaymentRecordsAfterCharge(SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory,
            Charge charge, DateTime dateToday, SystemMessages msgs, bool pendingCase = false, DatabaseConnection dc = null)
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

            var creditCardFeePayer = billingAccount.IsCreditCardFeeTakenOutOfCost ? billingAccount : purchasingAccount;

            var convenienceFeePayer = billingAccount.IsConvenienceFeeTakenOutOfCost
                   ? billingAccount
                   : purchasingAccount;

            var useConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, dc);
            var usePlatformFees = useConnectedAccount && subscriptionPaymentHistory.AmountInCents.IsEmpty();
            var platFormFlatFee = (!participationObject.IsRegistrationFree) ? billingAccount.GetPlatformConvenienceFlatFee(msgs, dc) : 0;
            var platformPercentFee = billingAccount.GetPlatformConveniencePercentFee(msgs, dc);

            var paymentType = rsp.PaymentTypeId;
            var isAmountProrated = subscriptionPaymentHistory.ProratedCharge;

            // TODO: potentially problematic as the plan may have changed
            var convenienceFee = rsp.ConvenienceFee ?? plan.GetConvenienceFee(billingAccount, msgs);
            var paymentTypeFee = rsp.PaymentTypeFee.HasValue ? rsp.PaymentTypeFee : paymentType == PaymentType.CreditCard ? plan.GetCreditCardFee(billingAccount, msgs) : 0;

            if (isAmountProrated)
            {
                convenienceFee = GetProratedAmount(subscriptionSchedule, convenienceFee, dateToday);
                paymentTypeFee = GetProratedAmount(subscriptionSchedule, (decimal) paymentTypeFee, dateToday);
            }

            var amountPaidByParent = subscriptionPaymentHistory.AmountInCents / 100m;

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
                    if (pendingCase)
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

                        if (msgs.HasErrors())
                            msgs.Add("Error creating negative transaction record", MessageType.Error);
                    }
                    if (msgs.HasNoErrors())
                        checkoutObject.PayingAccount.SendSubscriptionPaymentFailed(checkoutObject.SubscriptionPlan, checkoutObject.OverrideAmount, dc);
                }
                else
                    EmailHelper.SendUpcomingSubscriptionPaymentReminder(subscriptionSchedule, true);
            }
            return msgs;
        }

        public String GetLatestSubscription()
        {
            SystemMessages msgs = new SystemMessages();

            var schedule = SubscriptionSchedule.LoadLatestSubscriptionSchedule(msgs);

            ScheduleDto scheduleAttributes = new ScheduleDto()
            {
                ID = schedule.ID,
                CreateDate = schedule.CreateDate,
                CreatedBy = schedule.CreatedBy,
                LastModifiedDate = schedule.LastModifiedDate,
                LastModifiedBy = schedule.LastModifiedBy,
                StartDate = schedule.StartDate,
                AmountInCents = schedule.AmountInCents,
                EndDate = schedule.EndDate,
                FirstRenewal = schedule.FirstRenewal,
                ProrateFirstCharge = schedule.ProrateFirstCharge,
                ProrateLastCharge = schedule.ProrateLastCharge,
                DateCancelled = schedule.DateCancelled,
                BillingPeriodType = schedule.BillingPeriodType,
                StripeCustomerId = schedule.StripeCustomerId,
                StripeSourceId = schedule.StripeSourceId,
                LastChargeStatus = schedule.LastChargeStatus,
                StripeConnectId = schedule.StripeConnectId,
                AmountToConnectAccountInCents = schedule.AmountToConnectAccountInCents,
                LastDateToBill = schedule.LastDateToBill,
                Description = schedule.Description,
                NextBillDate = schedule.NextBillDate,
                CancelledReason = schedule.CancelledReason,
                IsDeleted = schedule.IsDeleted,
                IsCancelled = schedule.IsCancelled,
                PaymentsDone = schedule.PaymentsDone
            };

            if (schedule.IsNotEmpty() && msgs.HasNoErrors())
                return JsonConvert.SerializeObject(scheduleAttributes);
            else
                return "Error in retrieving subscription";
        }

        public String GetLatestPaymentHistory()
        {
            SystemMessages msgs = new SystemMessages();

            var schedule = SubscriptionSchedule.LoadLatestSubscriptionSchedule(msgs);
            var paymentHistory = SubscriptionPaymentHistory.LoadBySubscriptionScheduleId((int)schedule.ID, msgs);

            PaymentDto paymentAttributes = new PaymentDto()
            {
                ID = paymentHistory.ID,
                CreateDate = paymentHistory.CreateDate,
                CreatedBy = paymentHistory.CreatedBy,
                LastModifiedDate = paymentHistory.LastModifiedDate,
                LastModifiedBy = paymentHistory.LastModifiedBy,
                SubscriptionScheduleId = paymentHistory.SubscriptionScheduleId,
                AmountInCents = paymentHistory.AmountInCents,
                DateCharged = paymentHistory.DateCharged,
                StripeChargeId = paymentHistory.StripeChargeId,
                ProratedCharge = paymentHistory.ProratedCharge,
                Status = paymentHistory.Status,
                AmountToConnectAccountInCents = paymentHistory.AmountToConnectAccountInCents,
                AmountToStripeInCents = paymentHistory.AmountToStripeInCents,
                RetryNumber = paymentHistory.RetryNumber,
            };

            if (schedule.IsNotEmpty() && msgs.HasNoErrors())
                return JsonConvert.SerializeObject(paymentAttributes);
            else
                return "Error in retrieving subscription payment history";
        }
    }
}

public class ScheduleDto
{
    public int? ID { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime StartDate { get; set; }
    public Decimal AmountInCents { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? FirstRenewal { get; set; }
    public bool? ProrateFirstCharge { get; set; }
    public bool? ProrateLastCharge { get; set; }
    public DateTime? DateCancelled { get; set; }
    public BillingPeriodType BillingPeriodType { get; set; }
    public string StripeCustomerId { get; set; }
    public string StripeSourceId { get; set; }
    public LastChargeStatus LastChargeStatus { get; set; }
    public string StripeConnectId { get; set; }
    public Decimal AmountToConnectAccountInCents { get; set; }
    public DateTime? LastDateToBill { get; set; }
    public string? Description { get; set; }
    public DateTime? NextBillDate { get; set; }
    public string CancelledReason { get; set; }
    public bool? IsDeleted { get; set; }
    public bool? IsCancelled { get; set; }
    public int PaymentsDone { get; set; }
}

public class PaymentDto
{
    public int? ID { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public int SubscriptionScheduleId { get; set; }
    public Decimal AmountInCents { get; set; }
    public DateTime DateCharged { get; set; }
    public string StripeChargeId { get; set; }
    public bool ProratedCharge { get; set; }
    public LastChargeStatus Status { get; set; }
    public Decimal AmountToConnectAccountInCents { get; set; }
    public Decimal AmountToStripeInCents { get; set; }
    public int RetryNumber { get; set; }
}

