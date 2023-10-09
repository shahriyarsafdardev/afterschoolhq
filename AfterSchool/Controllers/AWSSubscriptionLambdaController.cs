using AfterSchoolLibrary.Billing;
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
                // Send an email for upcoming payments.
            }
        }

        private void ProcessSubscriptionSchedulesPendingToday(IEnumerable<SubscriptionSchedule> schedules, SystemMessages msgs)
        {
            foreach (var schedule in schedules)
            {
                if (schedule.StripeSourceId.HasValue())
                {
                    var amount = schedule.AmountInCents / 100.0m;

                    if (schedule.LastDateToBill.HasValue && schedule.NextBillDate.Equals(schedule.LastDateToBill) && (schedule.ProrateLastCharge ?? false))
                    {
                        amount = CalculateProratedAmount(schedule, msgs);
                    }

                    var charge = CreateCharge(schedule, amount, msgs);

                    if (charge.IsEmpty())
                        msgs.Add("Error in running the charge", MessageType.Error);
                    else
                        ProcessCharge(charge, schedule, null, msgs);
                }
                else
                    msgs.Add("StripeSourceId is empty", MessageType.Error);
            }
        }

        private decimal CalculateProratedAmount(SubscriptionSchedule schedule, SystemMessages msgs)
        {
            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);
            var enddate = rsp.GetSubscriptionPlan()?.EndDate;
            return SubscriptionScheduleService.CalculateProratedAmount((DateTime) enddate, DateTime.UtcNow, schedule.AmountInCents / 100.0m);
        }

        private Charge CreateCharge(SubscriptionSchedule schedule, decimal amount, SystemMessages msgs)
        {
            // Create a charge for the current schedule
            return StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                fromCustomerId: schedule.StripeCustomerId,
                destinationAccountId: null,
                platformFee: null,
                tokenOrSourceId: schedule.StripeSourceId,
                totalAmount: amount,
                description: schedule.Description,
                statementDescriptor: "AHQ Subscription charge",
                metaData: new Dictionary<string, string>
                {
                    //{"ActivityUid", ""},
                    //{"BillingAccountUid", ""},
                    //{"PayingAccountUid", ""},
                    //{"RegistrationUid", ""},
                    //{"CreditCardFeePayer", ""},
                    //{"ConvenienceFeePayer", ""},
                    //{"PurchasedBy", ""},
                    //{"BilledBy", ""},
                },
                msgs: msgs
            );
        }

        private void ProcessCharge(Charge charge, SubscriptionSchedule schedule, SubscriptionPaymentHistory? subscriptionPaymentHistory,  SystemMessages msgs)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);

            if(subscriptionPaymentHistory.IsEmpty())
            {
                subscriptionPaymentHistory = SubscriptionPaymentHistoryService.CreateNew(
                schedule.ID.Value,
                schedule.AmountInCents,
                DateTime.UtcNow,
                charge.Id,
                false,
                chargeStatus,
                charge.StripeResponse.ToString(),
                schedule.AmountToConnectAccountInCents,
                50,
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
                //?????
            }
            else
            {
                //????
            }
        }

        private void ProcessPaymentHistoryWithPendingStatus(IEnumerable<SubscriptionPaymentHistory> paymentHistories, SystemMessages msgs)
        {
            foreach (var paymentHistory in paymentHistories)
            {
                var schedule = SubscriptionSchedule.Load(paymentHistory.SubscriptionScheduleId, msgs, null);
                var charge = StripeHelper.GetStripeHelperConnectAccount().GetCharge(paymentHistory.StripeChargeId, msgs);

                if (charge.IsEmpty())
                    msgs.Add("Error in running the charge", MessageType.Error);
                
                else
                {
                    paymentHistory.Status = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
                    msgs.AddRange(paymentHistory.Save());

                    ProcessCharge(charge, schedule, paymentHistory, msgs);
                }
            }
        }

        private void ProcessFailedSubscriptionSchedules(IEnumerable<SubscriptionSchedule> schedules, SystemMessages msgs)
        {
            foreach (var schedule in schedules)
            {
                if (schedule.StripeSourceId.HasValue())
                {
                    var amount = schedule.AmountInCents / 100.0m;

                    if (schedule.NextBillDate.Equals(schedule.LastDateToBill) && (schedule.ProrateLastCharge ?? false))
                        amount = CalculateProratedAmount(schedule, msgs);

                    var charge = CreateCharge(schedule, amount, msgs);
                    var subscriptionPaymentHistory = SubscriptionPaymentHistory.LoadBySubscriptionScheduleId((int) schedule.ID, msgs, null);

                    if (charge.IsEmpty())
                        msgs.Add("Error in running the charge", MessageType.Error);

                    else
                        ProcessCharge(charge, schedule, subscriptionPaymentHistory, msgs);
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