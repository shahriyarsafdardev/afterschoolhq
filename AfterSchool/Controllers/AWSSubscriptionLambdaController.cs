using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using RestSharp.Extensions;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;
using SubscriptionScheduleService = AfterSchoolLibrary.Services.SubscriptionScheduleService;

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

            if (subscriptionSchedulespendingToday != null)
            {
                foreach (var schedule in subscriptionSchedulespendingToday)
                {
                    // Check if the schedule has a valid StripeSourceId
                    if (schedule.StripeSourceId.HasValue())
                    {
                        // Create a charge for the current schedule
                        var charge = StripeHelper.GetStripeHelperConnectAccount().CreateCharge(
                            fromCustomerId: "cus_O3CX9NkjA7u96Y",
                            destinationAccountId: null,
                            platformFee: null,
                            tokenOrSourceId: schedule.StripeSourceId, // Use the StripeSourceId from the schedule
                            totalAmount: schedule.AmountInCents / 100.0m, // Convert cents to dollars
                            description: schedule.Description,
                            statementDescriptor: "ABCD",
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

                        // Handle the charge result as needed
                        // You can check if the charge was successful and process any errors or notifications
                        if (charge != null && charge.Status == LastChargeStatus.succeeded.ToString())
                        {
                            // Charge was successful, handle the result
                            // You can log, update the schedule status, etc.

                            //have to calculate amountToStripeInCents
                            var subscriptionPaymentHistory = SubscriptionPaymentHistoryService.CreateNew(schedule.ID.Value, schedule.AmountInCents, DateTime.UtcNow,
                                charge.Id, false, LastChargeStatus.succeeded, charge.StripeResponse.ToString(), schedule.AmountToConnectAccountInCents, 50, 0);

                            var webhookPayload = new WebhookPayload
                            {
                                PaymentHistory = subscriptionPaymentHistory,
                                Schedule = schedule
                            };
                        }
                        else
                        {
                            // Charge failed, handle the error
                            // You can log the error, notify the user, or take appropriate action
                        }
                    }
                    else
                    {
                        msgs.Add("Can not run the charge. StripeSourceId is empty", MessageType.Error);
                    }
                }
            }
        }

        public class WebhookPayload
        {
            public SubscriptionPaymentHistory PaymentHistory { get; set; }
            public SubscriptionSchedule Schedule { get; set; }
        }
    }
}