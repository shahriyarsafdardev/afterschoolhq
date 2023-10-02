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
                        var amount = schedule.AmountInCents / 100.0m;

                        //will only prorate last charge if subscription end date exists???
                        if (schedule.LastDateToBill.HasValue && schedule.NextBillDate.Equals(schedule.LastDateToBill) && (schedule.ProrateLastCharge ?? false))
                        {
                            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);
                            var enddate = rsp.GetSubscriptionPlan()?.EndDate;
                            amount = SubscriptionScheduleService.CalculateProratedAmount((DateTime)enddate, DateTime.UtcNow, amount);
                        }

                        // Create a charge for the current schedule
                        var charge = StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                            fromCustomerId: schedule.StripeCustomerId,
                            destinationAccountId: null,
                            platformFee: null,
                            tokenOrSourceId: schedule.StripeSourceId, 
                            totalAmount: amount, // Convert cents to dollars
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

                        if (charge.IsEmpty())
                            msgs.Add("Error in running the charge", MessageType.Error);
                        else
                        {
                            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);

                            //have to calculate amountToStripeInCents
                            var subscriptionPaymentHistory = SubscriptionPaymentHistoryService.CreateNew(schedule.ID.Value, schedule.AmountInCents, DateTime.UtcNow,
                                    charge.Id, false, chargeStatus, charge.StripeResponse.ToString(), schedule.AmountToConnectAccountInCents, 50, 0);

                            if (charge.Status.Equals(LastChargeStatus.succeeded))
                            {
                                var webhookPayload = new WebhookPayload
                                {
                                    //pass Uid instead of whole object
                                    PaymentHistory = subscriptionPaymentHistory,
                                    Schedule = schedule
                                };

                                var jsonPayload = JsonConvert.SerializeObject(webhookPayload);

                                var response = SendWebhook(jsonPayload);

                                if (response.IsNotEmpty() /*&& response.IsSuccessStatusCode*/) //have to make Lambda Async
                                {
                                    UpdateNextBillDateOnSuccess(schedule, msgs);
                                }
                                else
                                {
                                    //what to do if webhook fails
                                }
                            }
                            else if (charge.Status.Equals(LastChargeStatus.failed))
                            {
                                msgs.Add("Charge failed", MessageType.Error);

                                if (subscriptionPaymentHistory.RetryNumber < 3)
                                    subscriptionPaymentHistory.RetryNumber += 1;

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
                    }
                    else
                        msgs.Add("StripeSourceId is empty", MessageType.Error);
                }
            }

            if (subscriptionPaymentHistoryPendingStatus.Any())
            {
                foreach (var paymentHistory in subscriptionPaymentHistoryPendingStatus)
                {
                    var schedule = SubscriptionSchedule.Load(paymentHistory.SubscriptionScheduleId, msgs, null);
                    var charge = StripeHelper.GetStripeHelperConnectAccount().GetCharge(paymentHistory.StripeChargeId, msgs);
                    if (charge.IsEmpty())
                        msgs.Add("Unable to get the charge", MessageType.Error);
                    else
                    {
                        var chargeStatus = Enum.Parse(typeof(LastChargeStatus), charge.Status);

                        paymentHistory.Status = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
                        msgs.AddRange(paymentHistory.Save());

                        if (chargeStatus.Equals(LastChargeStatus.succeeded))
                        {
                            var webhookPayload = new WebhookPayload
                            {
                                //pass Uid instead of whole object
                                PaymentHistory = paymentHistory,
                                Schedule = schedule
                            };

                            var jsonPayload = JsonConvert.SerializeObject(webhookPayload);

                            var response = SendWebhook(jsonPayload);

                            if (response.IsNotEmpty() /*&& response.IsSuccessStatusCode*/) //have to make Lambda Async
                            {
                                UpdateNextBillDateOnSuccess(schedule, msgs);
                            }
                            else
                            {
                                //what to do if webhook fails
                            }
                        }
                        else if (chargeStatus.Equals(LastChargeStatus.failed))
                        {
                            if (paymentHistory.RetryNumber < 3)
                                paymentHistory.RetryNumber += 1;

                            msgs.AddRange(paymentHistory.Save());
                            subscriptionSchedulesFailed.Add(schedule);
                        }
                        else
                        {
                            //what if charge is still pending and status didn't updated??
                        }
                    }
                }
            }

            if (subscriptionSchedulesFailed.Any())
            {
                foreach (var schedule in subscriptionSchedulesFailed)
                {
                    // Check if the schedule has a valid StripeSourceId
                    if (schedule.StripeSourceId.HasValue())
                    {
                        var amount = schedule.AmountInCents / 100.0m;

                        //will only prorate last charge if subscription end date exists???
                        if (schedule.NextBillDate.Equals(schedule.LastDateToBill) && (schedule.ProrateLastCharge ?? false))
                        {
                            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(schedule.Uid, msgs);
                            var enddate = rsp.GetSubscriptionPlan()?.EndDate;
                            amount = SubscriptionScheduleService.CalculateProratedAmount((DateTime) enddate, DateTime.UtcNow, amount);
                        }

                        // Create a charge for the current schedule
                        var charge = StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                            fromCustomerId: schedule.StripeCustomerId,
                            destinationAccountId: null,
                            platformFee: null,
                            tokenOrSourceId: schedule.StripeSourceId, 
                            totalAmount: amount, // Convert cents to dollars
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

                        var subscriptionPaymentHistory = SubscriptionPaymentHistory.LoadBySubscriptionScheduleId((int)schedule.ID, msgs, null);

                        if (charge.IsEmpty())
                            msgs.Add("Error in running the charge", MessageType.Error);
                        else
                        {
                            if (charge.Status.Equals(LastChargeStatus.succeeded))
                            {
                                var webhookPayload = new WebhookPayload
                                {
                                    //pass Uid instead of whole object
                                    PaymentHistory = subscriptionPaymentHistory,
                                    Schedule = schedule
                                };

                                var jsonPayload = JsonConvert.SerializeObject(webhookPayload);

                                var response = SendWebhook(jsonPayload);

                                if (response.IsNotEmpty() /*&& response.IsSuccessStatusCode*/) //have to make Lambda Async
                                {
                                    UpdateNextBillDateOnSuccess(schedule, msgs);
                                }
                                else
                                {
                                    //what to do if webhook fails
                                }
                            }
                            else if (charge.Status.Equals(LastChargeStatus.failed))
                            {
                                msgs.Add("Charge failed", MessageType.Error);

                                if (subscriptionPaymentHistory.RetryNumber < 3)
                                    subscriptionPaymentHistory.RetryNumber += 1;

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
                    }
                    else
                        msgs.Add("Can not run the charge. StripeSourceId is empty", MessageType.Error);
                }
            }

            if (subscriptionSchedulesAfterThreeDays.IsNotEmpty())
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

        public void UpdateNextBillDateOnSuccess(SubscriptionSchedule subscriptionSchedule, SystemMessages msgs, DatabaseConnection dc = null)
        {
            //What to do for ongoing activities as they don't have LastDateToBill?
            if (subscriptionSchedule.NextBillDate.HasValue && subscriptionSchedule.LastDateToBill.HasValue)
            {
                //check if it was the final payment
                if (subscriptionSchedule.NextBillDate.Value.Date.Equals(subscriptionSchedule.LastDateToBill.Value.Date))
                    subscriptionSchedule.NextBillDate = null;
                else
                {
                    subscriptionSchedule.NextBillDate = new SubscriptionPlan
                    {
                        BillingPeriodTypeId = subscriptionSchedule.BillingPeriodType
                    }.CalculateEndDateFromNumberOfPeriods((DateTime) subscriptionSchedule.NextBillDate, 1);
                }
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