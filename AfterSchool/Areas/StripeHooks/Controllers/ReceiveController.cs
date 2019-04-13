using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using Stripe;
using System;
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
                // msgs.Add("trying to get signature", MessageType.Notice);
                
                // return Ok(msgs.ToJSON());

                // var stripeSignature = Request.Headers.Contains("Stripe-Signature") ? Request.Headers.GetValues("Stripe-Signature").First() : null;


                //if (!AfterSchoolLibrary.Helpers.Utilities.IsLocalHost && stripeSignature.IsEmpty())
                //{
                //    msgs.Add("Invalid Signature", MessageType.Error);
                //    SendLogEmail(msgs);
                //    return Ok();
                //}
                // ,
                //stripeSignature, 
                //        "whsec_ImO7GlHXHiYs8pityQDZbBWJvF0eLEas"

                msgs.Add("Getting serializedJson", MessageType.Notice);
                var serializedJson = JsonConvert.SerializeObject(json);
                msgs.Add("Getting stripeEvent", MessageType.Notice);
                var stripeEvent = (Stripe.Event)Stripe.EventUtility.ParseEvent(serializedJson);

                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        msgs.Add("Getting AccountUpdated", MessageType.Notice);
                        return AccountUpdated(stripeEvent, msgs);
                    case "invoice.payment_succeeded":
                        msgs.Add("Getting payment_succeeded", MessageType.Notice);
                        return InvoicePaymentSucceeded(stripeEvent, msgs);
                    case "invoice.payment_failed":
                        return InvoicePaymentFailed(stripeEvent, msgs);
                    default:
                        return ReturnOK(msgs);
                }
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                return Return500(msgs);
            }
        }

        private void SendLogEmail(SystemMessages msgs)
        {
            string toEmail = "darye+stripe+errors@afterschoolhq.com";
            string subject = $"Stripe Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
            string body = msgs.RenderMessageList(MessageOutputType.Html);

            AfterSchoolLibrary.Helpers.EmailHelper.SendEmail(toEmail, subject, body, true);
        }

        private IHttpActionResult ReturnOK(SystemMessages msgs)
        {
            // SendLogEmail(msgs);
            return Ok(msgs.ToJSON());
        }

        private IHttpActionResult Return500(SystemMessages msgs)
        {
            SendLogEmail(msgs);
            return InternalServerError(new Exception(msgs.RenderMessageList(MessageOutputType.Text)));
        }

        private IHttpActionResult AccountUpdated(Stripe.Event stripeEvent, SystemMessages msgs)
        {
            msgs.Add("Getting stripeAccountId", MessageType.Notice);
            var stripeAccountId = stripeEvent.Account;
            msgs.Add("Getting account", MessageType.Notice);
            var account = AfterSchoolLibrary.Models.Account.LoadByStripeUserId(stripeAccountId);

            if (account.IsEmpty())
            {
                msgs.Add("Account not found for some reason", MessageType.Notice);
                return ReturnOK(msgs);
            }

            msgs.Add("Getting stripeAccount", MessageType.Notice);
            var ahqStripeAccount = account.GetStripeAccount(msgs);

            if (ahqStripeAccount.IsEmpty())
            {
                msgs.Add("No stripe account yet", MessageType.Notice);
                return ReturnOK(msgs);
            }            

            msgs.Add("Getting serializedAccountJson", MessageType.Notice);
            var serializedAccountJson = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            var stripeAccount = Stripe.EventUtility.ParseEventDataItem<Stripe.Account>(serializedAccountJson);
            msgs.Add("Getting startingStatus", MessageType.Notice);
            var startingStatus = ahqStripeAccount.VerificationStatusId;

            msgs.Add("Getting ParseAndUpdateStripeCustomAccount", MessageType.Notice);
            ahqStripeAccount.ParseAndUpdateStripeCustomAccount(stripeAccount, null, msgs);

            msgs.Add("Getting SendStripeAccountVerified", MessageType.Notice);
            if (msgs.HasNoErrors() && startingStatus != ahqStripeAccount.VerificationStatusId && ahqStripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                account.SendStripeAccountVerified(null);

            if (msgs.HasErrors())
                return Return500(msgs);

            return ReturnOK(msgs);
        }

        private IHttpActionResult InvoicePaymentSucceeded(Stripe.Event stripeEvent, SystemMessages msgs)
        {
            var stripeInvoice = (Stripe.Invoice)stripeEvent.Data.Object;
            // var customerId = stripeInvoice.CustomerId;
            var stripeSubscriptionId = stripeInvoice.SubscriptionId;
            var chargeId = stripeInvoice.ChargeId;
            // var totalAmountInCents = (decimal)stripeInvoice.Total;
            var registrantSubscriptionPlan = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(stripeSubscriptionId, msgs);
            var plan = registrantSubscriptionPlan.GetSubscriptionPlan(msgs);
            var registrant = registrantSubscriptionPlan.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var purchasingAccount = masterRegistrant.GetAccount(msgs);
            var billingAccount = plan.GetAccount(msgs);
            var participationObject = plan.GetParticipationObject(msgs);
            var registrantActivity = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObject.ObjectID.Value, 
                participationObject.ObjectType, 
                registrant.ID.Value, msgs);
            var registration = registrantActivity.GetRegistration(msgs);


            if (msgs.HasNoErrors())
            {
                // check if this same charge has been posted already and tracked.
                var transaction = Transaction.LoadByStripeChargeOrRefundId(chargeId, msgs);

                if (transaction.IsEmpty())
                {
                    // send payment succeeded email
                    purchasingAccount.SendSubscriptionPaymentSucceeded(plan);
                    
                    // save transaction info and track payment to the activity as well.
                    var checkoutObject = new CheckoutObject()
                    {
                        PayingAccount = purchasingAccount,
                        BillingAccount = billingAccount,
                        // OverrideAmount = totalAmountInCents / 100, // should be able to pull from the plan
                        ParticipationObject = participationObject,
                        PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                        PaymentType = PaymentType.CreditCard,
                        StripeChargeId = chargeId,
                        SubscriptionPlan = plan.PlanTypeId == SubscriptionPlanType.Subscription ? plan : null,
                        PaymentPlan = plan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? plan : null,
                        MasterRegistrant = masterRegistrant,
                        NumberOfSeats = 1,
                        Registration = registration,
                        RegistrantSubscriptionPlan = registrantSubscriptionPlan
                    };

                    billingAccount.ProcessSubscriptionPlanPayment(checkoutObject, plan, registrantSubscriptionPlan, msgs);
                }
            }

            if (msgs.HasErrors())
                return Return500(msgs);

            return ReturnOK(msgs);
        }

        private IHttpActionResult InvoicePaymentFailed(Stripe.Event stripeEvent, SystemMessages msgs)
        {
            var stripeInvoice = (Stripe.Invoice)stripeEvent.Data.Object;
            var customerId = stripeInvoice.CustomerId;
            var account = AfterSchoolLibrary.Models.Account.LoadByStripeCustomerId(customerId, msgs);
            var subscriptionId = stripeInvoice.SubscriptionId;
            var registrantSubscription = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(subscriptionId, msgs);
            var plan = registrantSubscription.GetSubscriptionPlan(msgs);

            if (msgs.HasNoErrors())
               account.SendSubscriptionPaymentFailed(plan, null);

            if (msgs.HasErrors())
                return Return500(msgs);

            return ReturnOK(msgs);
        }
    }
}
