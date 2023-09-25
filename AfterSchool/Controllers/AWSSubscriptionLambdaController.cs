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

            if (subscriptionSchedulespendingToday != null)
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
                                PaymentHistory = subscriptionPaymentHistory,
                                Schedule = schedule
                            };

                            var jsonPayload = JsonConvert.SerializeObject(webhookPayload);

                            SendWebhook(jsonPayload);
                        }
                        else if(charge.Status == LastChargeStatus.failed.ToString())
                        {
                            msgs.Add("Charge failed", MessageType.Error);

                            subscriptionPaymentHistory.Status = LastChargeStatus.failed;

                            var retryNumber = subscriptionPaymentHistory.RetryNumber;

                            if(retryNumber < 3)
                                subscriptionPaymentHistory.RetryNumber = retryNumber + 1;

                            msgs.AddRange(subscriptionPaymentHistory.Save());

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

        private async Task SendWebhook(string payload)
        {
            var msgs = new SystemMessages();
            try
            {
                //HttpResponseMessage response;
                using (HttpClient client = new HttpClient())
                {
                    // Set the base URL of your Web API
                    string baseUrl = Utilities.SurveyBaseUrl; 

                    string apiUrl = $"{baseUrl}/webhooks/ChargeSubscription/Updated";

                    var content = new StringContent(payload, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("User-Agent", "AFTRSCHL/.1");

                    var response = client.PostAsync(apiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        //if successful, send email to customer and AHQ 
                    }
                    else
                    {
                        msgs.Add("Webhook was unsuccessful", MessageType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
            }
        }


        public class WebhookPayload
        {
            public SubscriptionPaymentHistory PaymentHistory { get; set; }
            public SubscriptionSchedule Schedule { get; set; }
        }
    }
}