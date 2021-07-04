using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Web.Http;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class StripeController : ApiController
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

            if (!account.HasStripeConnectedAccount)
            {
                msgs.Add("No stripe account yet", MessageType.Notice);
                return ReturnOK(msgs);
            }

            msgs.Add("Getting serializedAccountJson", MessageType.Notice);
            var stripeAccount = stripeEvent.Data.Object as Stripe.Account;

            msgs.Add("Getting SendStripeAccountVerified", MessageType.Notice);
            if (msgs.HasNoErrors() && !account.StripeChargesEnabled && stripeAccount.ChargesEnabled)
            {
                account.UpdateStripeChargesAndPayouts(stripeAccount, msgs);
                account.SendStripeAccountVerified(null);
            }

            if (msgs.HasErrors())
                return Return500(msgs);

            return ReturnOK(msgs);
        }

        private IHttpActionResult InvoicePaymentSucceeded(Stripe.Event stripeEvent, SystemMessages msgs)
        {
            var stripeInvoice = (Stripe.Invoice)stripeEvent.Data.Object;
            var stripeSubscriptionId = stripeInvoice.SubscriptionId;
            var chargeId = stripeInvoice.ChargeId;
            var rsp = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(stripeSubscriptionId, msgs);

            if (rsp.IsEmpty())
            {
                msgs.Add($"Ignoring - Registrant Subscription Plan not found - Stripe Sub ID {stripeSubscriptionId}", MessageType.Notice);
                return ReturnOK(msgs);
            }

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

            if (msgs.HasErrors())
                return Return500(msgs);

            // check if this same charge has been posted already and tracked.
            var transaction = Transaction.LoadByStripeChargeOrRefundId(chargeId, msgs);

            if (transaction.IsNotEmpty())
            {
                msgs.Add($"Ignoring - Transaction has already been processed - Stripe Sub ID {stripeSubscriptionId}", MessageType.Notice);
                return ReturnOK(msgs);
            }

            // send payment succeeded email
            purchasingAccount.SendSubscriptionPaymentSucceeded(rsp, plan);

            AfterSchoolLibrary.Models.Account convenienceFeePayer;
            AfterSchoolLibrary.Models.Account transactionFeePayer;

            if (rsp.ConvenienceFeesPaidBy == FeesPaidByType.Parent)
                convenienceFeePayer = purchasingAccount;
            else
                convenienceFeePayer = billingAccount;

            if (rsp.PaymentTypeFeesPaidBy == FeesPaidByType.Parent)
                transactionFeePayer = purchasingAccount;
            else
                transactionFeePayer = billingAccount;

            //we will need this logic to ensure we are adding pro-rated amount in ActivityPayment and RegistrantSubscriptionPlan
            var overrideAmount = (decimal)stripeInvoice.AmountPaid / 100;

            // save transaction info and track payment to the activity as well.
            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                OverrideAmount = overrideAmount > 0 ? overrideAmount : rsp.TotalPaidWithFees,
                ConvenienceFeeOverrideAmount = rsp.ConvenienceFee,
                PaymentTypeFeeOverrideAmount = rsp.PaymentTypeFee,
                ParticipationObject = participationObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = rsp.PaymentTypeId,
                StripeChargeId = chargeId,
                SubscriptionPlan = plan.PlanTypeId == SubscriptionPlanType.Subscription ? plan : null,
                PaymentPlan = plan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? plan : null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                RegistrantSubscriptionPlan = rsp,
                ConvenienceFeePayer = convenienceFeePayer,
                CreditCardFeePayer = transactionFeePayer,
            };

            billingAccount.ProcessSubscriptionPlanPayment(checkoutObject, plan, rsp, msgs);

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