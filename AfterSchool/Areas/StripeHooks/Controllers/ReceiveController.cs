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
                {
                    msgs.Add("Invalid Signature", MessageType.Error);
                    return Unauthorized();
                }

                msgs.Add("Getting serializedJson", MessageType.Notice);
                var serializedJson = JsonConvert.SerializeObject(json);
                msgs.Add("Getting stripeEvent", MessageType.Notice);
                var stripeEvent = AfterSchoolLibrary.Utilities.IsLocalHost ?
                    (StripeEvent)StripeEventUtility.ParseEvent(serializedJson) :
                    (StripeEvent)StripeEventUtility.ConstructEvent(
                        serializedJson,
                        stripeSignature, 
                        "whsec_ImO7GlHXHiYs8pityQDZbBWJvF0eLEas");

                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        msgs.Add("Getting AccountUpdated", MessageType.Notice);
                        return AccountUpdated(stripeEvent, msgs);
                    case "invoice.payment_succeeded":
                        return Ok(msgs.ToJSON());
                        msgs.Add("Getting payment_succeeded", MessageType.Notice);
                        return InvoicePaymentSucceeded(stripeEvent, msgs);
                    default:
                        return Ok(msgs.ToJSON());
                }
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                SendLogEmail(msgs);
                return InternalServerError(new Exception(msgs.RenderMessageList(MessageOutputType.Text)));
            }
        }

        private void SendLogEmail(SystemMessages msgs)
        {
            string toEmail = "darye+stripe+errors@afterschoolhq.com";
            string subject = $"Stripe Webhook Errors - {AfterSchoolLibrary.Utilities.HostName(true)}";
            string body = msgs.RenderMessageList(MessageOutputType.Html);

            BackgroundJob.Enqueue(() => EmailHelper.SendEmail(toEmail, subject, body, true));
        }

        private IHttpActionResult AccountUpdated(StripeEvent stripeEvent, SystemMessages msgs)
        {
            msgs.Add("Getting stripeAccountId", MessageType.Notice);
            var stripeAccountId = (string)stripeEvent.Data.Object.id;
            msgs.Add("Getting account", MessageType.Notice);
            var account = Account.LoadByStripeUserId(stripeAccountId);

            if (account.IsEmpty())
            {
                msgs.Add("Account not found for some reason", MessageType.Notice);
                SendLogEmail(msgs);
                return Ok(msgs.ToJSON());
            }

            msgs.Add("Getting stripeAccount", MessageType.Notice);
            var stripeAccount = account.GetStripeAccount(msgs);
            msgs.Add("Getting serializedAccountJson", MessageType.Notice);
            var serializedAccountJson = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            msgs.Add("Getting startingStatus", MessageType.Notice);
            var startingStatus = stripeAccount.VerificationStatusId;

            msgs.Add("Getting ParseAndUpdateStripeCustomAccount", MessageType.Notice);
            stripeAccount.ParseAndUpdateStripeCustomAccount((string)serializedAccountJson, null, msgs);

            msgs.Add("Getting SendStripeAccountVerified", MessageType.Notice);
            if (msgs.HasNoErrors() && startingStatus != stripeAccount.VerificationStatusId && stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                BackgroundJob.Enqueue(() => account.SendStripeAccountVerified(null));

            if (msgs.HasErrors())
            {
                SendLogEmail(msgs);
                return InternalServerError();
            }

            SendLogEmail(msgs);
            return Ok(msgs.ToJSON());
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
