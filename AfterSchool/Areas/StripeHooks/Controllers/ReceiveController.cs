using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Hangfire;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Linq;
using System.Web.Http;

namespace AfterSchool.Areas.StripeHooks.Controllers
{
    public class ReceiveController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Updated([FromBody] dynamic json)
        {
            var msgs = new SystemMessages();
            try
            {
                var stripeSignature = Request.Headers.Contains("Stripe-Signature") ? Request.Headers.GetValues("Stripe-Signature").First() : null;

                if (!AfterSchoolLibrary.Utilities.IsLocalHost && stripeSignature.IsEmpty())
                    return Unauthorized();

                var serializedJson = JsonConvert.SerializeObject(json);
                var stripeEvent = AfterSchoolLibrary.Utilities.IsLocalHost ?
                    (StripeEvent)StripeEventUtility.ParseEvent(serializedJson) :
                    (StripeEvent)StripeEventUtility.ConstructEvent(
                        serializedJson,
                        stripeSignature, 
                        "whsec_ImO7GlHXHiYs8pityQDZbBWJvF0eLEas");

                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        return AccountUpdated(stripeEvent, msgs);
                    case "invoice.payment_succeeded":
                        return InvoicePaymentSucceeded(stripeEvent, msgs);
                    default:
                        return Ok();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private IHttpActionResult AccountUpdated(StripeEvent stripeEvent, SystemMessages msgs)
        {
            var stripeAccountId = (string)stripeEvent.Data.Object.id;
            var account = Account.LoadByStripeUserId(stripeAccountId);
            var stripeAccount = account.GetStripeAccount(msgs);
            var serializedAccountJson = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            var startingStatus = stripeAccount.VerificationStatusId;

            stripeAccount.ParseAndUpdateStripeCustomAccount((string)serializedAccountJson, null, msgs);

            if (msgs.HasNoErrors() && startingStatus != stripeAccount.VerificationStatusId && stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                BackgroundJob.Enqueue(() => account.SendStripeAccountVerified(null));

            if (msgs.HasErrors())
                return InternalServerError();

            return Ok();
        }

        private IHttpActionResult InvoicePaymentSucceeded(StripeEvent stripeEvent, SystemMessages msgs)
        {
            var customerId = (string)stripeEvent.Data.Object.customer;
            var payingAccount = Account.LoadByStripeCustomerId(customerId, msgs);
            var subscriptionId = (string)stripeEvent.Data.Object.subscription;
            var totalAmount = ((string)stripeEvent.Data.Object.total).ToDecimal();
            var registrantSubscription = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(subscriptionId, msgs);
            var plan = registrantSubscription.GetSubscriptionPlan(msgs);
            var planOwnerAccount = plan.GetAccount(msgs);

            if (msgs.HasErrors())
                BackgroundJob.Enqueue(() => payingAccount.SendSubscriptionPaymentSucceeded(plan, null));

            if (msgs.HasErrors())
            {
                var payment = registrantSubscription.AddPayment(totalAmount, null, msgs);

                // cancel stripe subscription at end of term
                if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan && registrantSubscription.GetPaymentCount(msgs) == plan.TotalPeriods.Value)
                    msgs.AddRange(AfterSchoolLibrary.Billing.StripeHelper.CancelSubscription(subscriptionId, planOwnerAccount.StripeUserId));
            }

            if (msgs.HasErrors())
                return InternalServerError();

            return Ok();
        }

        private IHttpActionResult InvoicePaymentFailed(StripeEvent stripeEvent, SystemMessages msgs)
        {
            var customerId = (string)stripeEvent.Data.Object.customer;
            var account = Account.LoadByStripeCustomerId(customerId, msgs);
            var subscriptionId = (string)stripeEvent.Data.Object.subscription;
            var registrantSubscription = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(subscriptionId, msgs);
            var plan = registrantSubscription.GetSubscriptionPlan(msgs);

            if (msgs.HasErrors())
                BackgroundJob.Enqueue(() => account.SendSubscriptionPaymentFailed(plan, null));

            if (msgs.HasErrors())
                return InternalServerError();

            return Ok();
        }
    }
}
