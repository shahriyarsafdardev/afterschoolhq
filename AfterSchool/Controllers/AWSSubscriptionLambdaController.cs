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
//using System.Web.Http;
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

            var subscriptionSchedulespendingToday = SubscriptionScheduleService.GetSubscriptionSchedulePendingToday(msgs, null);
            var subscriptionSchedulesAfterThreeDays = SubscriptionScheduleService.GetSubscriptionSchedulePendingInThreeDays(msgs, null);

            //if (!subscriptionSchedulespendingToday.Any())
            //{
            //    return;
            //}
            if (subscriptionSchedulespendingToday.Any())
            {
                foreach (var schedule in subscriptionSchedulespendingToday)
                {
                    // Check if the schedule has a valid StripeSourceId
                    if (schedule.StripeSourceId.HasValue())
                    {
                        // Create a charge for the current schedule
                        var charge = StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                            fromCustomerId: schedule.StripeCustomerId,
                            destinationAccountId: null,
                            platformFee: null,
                            tokenOrSourceId: schedule.StripeSourceId, // Use the StripeSourceId from the schedule
                            totalAmount: schedule.AmountInCents / 100.0m, // Convert cents to dollars
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

                        //have to calculate amountToStripeInCents
                        var subscriptionPaymentHistory = SubscriptionPaymentHistoryService.CreateNew(schedule.ID.Value, schedule.AmountInCents, DateTime.UtcNow,
                                charge.Id, false, LastChargeStatus.succeeded, charge.StripeResponse.ToString(), schedule.AmountToConnectAccountInCents, 50, 0);

                        if (charge != null && charge.Status == LastChargeStatus.succeeded.ToString())
                        {
                            var webhookPayload = new WebhookPayload
                            {
                                //pass Uid instead of whole object
                                PaymentHistory = subscriptionPaymentHistory,
                                Schedule = schedule
                            };

                            var jsonPayload = JsonConvert.SerializeObject(webhookPayload);

                            var response = SendWebhook(jsonPayload);

                            if(response != null /*&& response.IsSuccessStatusCode*/) //have to make Lambda Async
                            {
                                UpdateRecordOnSuccess(schedule, msgs, null);
                            }
                            else
                            {
                                //what to do if webhook fails
                            }
                        }
                        else if(charge.Status == LastChargeStatus.failed.ToString())
                        {
                            msgs.Add("Charge failed", MessageType.Error);

                            subscriptionPaymentHistory.Status = LastChargeStatus.failed;

                            var retryNumber = subscriptionPaymentHistory.RetryNumber;

                            if(retryNumber < 3)
                                subscriptionPaymentHistory.RetryNumber = retryNumber + 1;

                            msgs.AddRange(subscriptionPaymentHistory.Save());

                            //what should be updated in Subscription Schedule in this case????

                            //send email to customer and provider notifying user of charge failure

                            //if Bad card or insufficient funds??????
                            //redirect to Card details which should require login

                            //if Need to cancel?    
                            //Cancel subscription, notify main AHQ system
                        }
                        else
                        {
                            //pending status?????
                        }
                    }
                    else
                        msgs.Add("Can not run the charge. StripeSourceId is empty", MessageType.Error);
                }
            }

            if(subscriptionSchedulesAfterThreeDays != null)
            {
                //send email that you have upcoming payment
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

        public void UpdateRecordOnSuccess(SubscriptionSchedule subscriptionSchedule, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (subscriptionSchedule.NextBillDate.HasValue && subscriptionSchedule.LastDateToBill.HasValue &&
                subscriptionSchedule.NextBillDate.Value.Date != subscriptionSchedule.LastDateToBill.Value.Date)
            {
                subscriptionSchedule.NextBillDate = subscriptionSchedule.BillingPeriodType switch
                {
                    BillingPeriodType.Week => subscriptionSchedule.NextBillDate?.AddDays(7),
                    BillingPeriodType.Month => subscriptionSchedule.NextBillDate?.AddMonths(1),
                    BillingPeriodType.Year => subscriptionSchedule.NextBillDate?.AddYears(1),
                    _ => throw new ArgumentException("Invalid subscription interval specified.")
                };
            }
            msgs.AddRange(subscriptionSchedule.Save(dc));
        }

        public class WebhookPayload
        {
            public SubscriptionPaymentHistory PaymentHistory { get; set; }
            public SubscriptionSchedule Schedule { get; set; }
        }
    }
}