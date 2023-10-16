using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
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
    public class AWSSubscriptionLambdaController : Controller
    {
        // GET: AWSSubscriptionLambdaController/Lambda
        [HttpGet]
        public void Lambda()
        {
            SystemMessages msgs = new SystemMessages();

            var subscriptionSchedulespendingToday = SubscriptionScheduleService.GetSubscriptionSchedulePendingToday(msgs);
            var subscriptionSchedulesFailed = SubscriptionScheduleService.GetFailedSubscriptionSchedules(msgs);
            var subscriptionPaymentHistoryPendingStatus = SubscriptionPaymentHistoryService.GetPendingCharges(msgs);
            var subscriptionSchedulesAfterThreeDays = SubscriptionScheduleService.GetSubscriptionSchedulePendingInThreeDays(msgs);

            ProcessSubscriptionSchedulesPendingToday(subscriptionSchedulespendingToday, msgs);
            ProcessPaymentHistoryWithPendingStatus(subscriptionPaymentHistoryPendingStatus, msgs);
            ProcessFailedSubscriptionSchedules(subscriptionSchedulesFailed, msgs);

            if (subscriptionSchedulesAfterThreeDays.IsNotEmpty())
            {
                foreach (var schedule in subscriptionSchedulesAfterThreeDays) 
                {
                    EmailHelper.SendUpcomingSubscriptionPaymentReminder(schedule, null);
                }
            }
        }

        private void ProcessSubscriptionSchedulesPendingToday(IEnumerable<SubscriptionSchedule> schedules, SystemMessages msgs)
        {
            foreach (var schedule in schedules)
            {
                if (schedule.StripeSourceId.HasValue())
                {
                    var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);

                    var amount = schedule.AmountInCents / 100.0m;

                    amount = GetProratedAmount(schedule, amount);

                    var charge = CreateCharge(schedule, rsp, amount, msgs);

                    if (charge.IsEmpty())
                        msgs.Add("Error in running the charge", MessageType.Error);
                    else
                        ProcessCharge(charge, schedule, null, rsp, msgs);
                }
                else
                    msgs.Add("StripeSourceId is empty", MessageType.Error);
            }
        }

        private Decimal GetProratedAmount(SubscriptionSchedule schedule, Decimal amount)
        {
            //if this is last payment
            if (schedule.LastDateToBill.HasValue && schedule.NextBillDate.Equals(schedule.LastDateToBill) && (schedule.ProrateLastCharge ?? false))
                return CalculateProratedAmount(DateTime.UtcNow, (DateTime) schedule.LastDateToBill, amount);

            //if this is first payment
            if (schedule.PaymentsDone < 1 && schedule.ProrateFirstCharge.HasValue && (schedule.ProrateFirstCharge ?? false))
                return CalculateProratedAmount(DateTime.UtcNow, (DateTime) schedule.FirstRenewal, amount);

            return amount;
        }

        private decimal CalculateProratedAmount(DateTime startDate, DateTime endDate, Decimal amount)
        {
            return SubscriptionScheduleService.CalculateProratedAmount(endDate, startDate, amount);
        }

        private Charge CreateCharge(SubscriptionSchedule schedule, RegistrantSubscriptionPlan rsp, decimal amount, SystemMessages msgs)
        {
            var billingAccount = rsp.GetSubscriptionPlan().GetActivity().GetParticipationObject().PaymentAccount;
            var useStripeConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, null);
            var metaData = JsonConvert.DeserializeObject<Dictionary<string, string>>(schedule.Metadata);

            metaData["PaymentNumber"] = (SubscriptionPaymentHistory.GetCountBySubscriptionScheduleIdByStatus(schedule.Uid,
                LastChargeStatus.succeeded, msgs) + 1).ToString();

            var paymentNumber = metaData["PaymentNumber"].ToInteger();
            if (paymentNumber > 1)
            {
                metaData["WasProrated"] = schedule.NextBillDate.Equals(schedule.LastDateToBill)
                    ? schedule.ProrateLastCharge.ToString()
                    : "false";
            }
            else
                metaData["WasProrated"] = schedule.ProrateFirstCharge.ToString() ?? "false";

            // TODO: Platform fees
            //var platformFeeAmount = checkoutObject.CalculatePlatformFeeAmountDuringCheckout(msgs, null);

                // Create a charge for the current schedule
            return StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                fromCustomerId: schedule.StripeCustomerId,
                destinationAccountId: useStripeConnectedAccount ? billingAccount.StripeUserId : null,
                platformFee: null, //useStripeConnectedAccount ? platformFeeAmount : new decimal?(),
                tokenOrSourceId: schedule.StripeSourceId,
                totalAmount: amount,
                description: schedule.Description,
                statementDescriptor: "AHQ Subscription charge",
                metaData: metaData,
                msgs: msgs
            );
        }

        private void ProcessCharge(Charge charge, SubscriptionSchedule schedule, SubscriptionPaymentHistory? subscriptionPaymentHistory,
            RegistrantSubscriptionPlan rsp, SystemMessages msgs)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
            var billingAccount = rsp.GetSubscriptionPlan().GetActivity().GetParticipationObject().PaymentAccount;
            var amountToStripe = StripeHelper.CalculateStripeFee(schedule.AmountInCents, billingAccount.IsCreditCardFeeTakenOutOfCost);

            if (subscriptionPaymentHistory.IsEmpty())
            {
                subscriptionPaymentHistory = SubscriptionPaymentHistoryService.CreateNew(
                schedule.ID.Value,
                schedule.AmountInCents,
                DateTime.UtcNow,
                charge.Id,
                charge.Metadata["WasProrated"].ToBoolean(),
                chargeStatus,
                charge.StripeResponse.ToString(),
                schedule.AmountToConnectAccountInCents,
                amountToStripe,
                0
                );
            }

            var webhookPayload = new WebhookPayload
            {
                PaymentHistory = subscriptionPaymentHistory,
                Schedule = schedule,
                Charge = charge,
            };

            var jsonPayload = JsonConvert.SerializeObject(webhookPayload);
            var response = SendWebhook(jsonPayload);

            if (response.IsNotEmpty() /*&& response.IsSuccessStatusCode*/) //have to make Lambda Async
            {
                // TODO: What if webhook is successful
            }
            else
            {
                // TODO: What if webhook is failed
            }
        }

        private void ProcessPaymentHistoryWithPendingStatus(IEnumerable<SubscriptionPaymentHistory> paymentHistories, SystemMessages msgs)
        {
            foreach (var paymentHistory in paymentHistories)
            {
                var schedule = SubscriptionSchedule.Load(paymentHistory.SubscriptionScheduleId, msgs, null);
                var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);
                var charge = StripeHelper.GetStripeHelperConnectAccount().GetCharge(paymentHistory.StripeChargeId, msgs);

                if (charge.IsEmpty())
                    msgs.Add("Error in running the charge", MessageType.Error);
                
                else
                {
                    paymentHistory.Status = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
                    msgs.AddRange(paymentHistory.Save());

                    ProcessCharge(charge, schedule, paymentHistory, rsp, msgs);
                }
            }
        }

        private void ProcessFailedSubscriptionSchedules(IEnumerable<SubscriptionSchedule> schedules, SystemMessages msgs)
        {
            foreach (var schedule in schedules)
            {
                if (schedule.StripeSourceId.HasValue())
                {
                    var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);

                    var amount = schedule.AmountInCents / 100.0m;

                    amount = GetProratedAmount(schedule, amount);

                    var charge = CreateCharge(schedule, rsp, amount, msgs);
                    var subscriptionPaymentHistory = SubscriptionPaymentHistory.LoadBySubscriptionScheduleId((int) schedule.ID, msgs, null);

                    if (charge.IsEmpty())
                        msgs.Add("Error in running the charge", MessageType.Error);

                    else
                        ProcessCharge(charge, schedule, subscriptionPaymentHistory, rsp, msgs);
                }
                else
                    msgs.Add("Can not run the charge. StripeSourceId is empty", MessageType.Error);
            }
        }

        private async Task<HttpResponseMessage> SendWebhook(string payload)
        {
            var msgs = new SystemMessages();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set the base URL of your Web API
                    string baseUrl = Utilities.SurveyBaseUrl;

                    string apiUrl = $"{baseUrl}/webhooks/ChargeSubscription/Updated";

                    var content = new StringContent(payload, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("User-Agent", "AFTRSCHL/.1");

                    var response = await client.PostAsync(apiUrl, content);

                    return response;
                }
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return null;
            }
        }

        public class WebhookPayload
        {
            public SubscriptionPaymentHistory PaymentHistory { get; set; }
            public SubscriptionSchedule Schedule { get; set; }
            public Charge Charge { get; set; }
        }
    }
}