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
                var stripeEvent = AfterSchoolLibrary.Helpers.Utilities.IsLocalHost ?
                    (StripeEvent)StripeEventUtility.ParseEvent(serializedJson) :
                    (StripeEvent)StripeEventUtility.ParseEvent(
                        serializedJson);

                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        msgs.Add("Getting AccountUpdated", MessageType.Notice);
                        return AccountUpdated(stripeEvent, msgs);
                    case "invoice.payment_succeeded":
                        msgs.Add("Getting payment_succeeded", MessageType.Notice);
                        return InvoicePaymentSucceeded(stripeEvent, msgs);
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

        private IHttpActionResult AccountUpdated(StripeEvent stripeEvent, SystemMessages msgs)
        {
            msgs.Add("Getting stripeAccountId", MessageType.Notice);
            var stripeAccountId = (string)stripeEvent.Data.Object.id;
            msgs.Add("Getting account", MessageType.Notice);
            var account = Account.LoadByStripeUserId(stripeAccountId);

            if (account.IsEmpty())
            {
                msgs.Add("Account not found for some reason", MessageType.Notice);
                return ReturnOK(msgs);
            }

            msgs.Add("Getting stripeAccount", MessageType.Notice);
            var stripeAccount = account.GetStripeAccount(msgs);

            if (stripeAccount.IsEmpty())
            {
                msgs.Add("No stripe account yet", MessageType.Notice);
                return ReturnOK(msgs);
            }

            msgs.Add("Getting serializedAccountJson", MessageType.Notice);
            var serializedAccountJson = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            msgs.Add("Getting startingStatus", MessageType.Notice);
            var startingStatus = stripeAccount.VerificationStatusId;

            msgs.Add("Getting ParseAndUpdateStripeCustomAccount", MessageType.Notice);
            stripeAccount.ParseAndUpdateStripeCustomAccount((string)serializedAccountJson, null, msgs);

            msgs.Add("Getting SendStripeAccountVerified", MessageType.Notice);
            if (msgs.HasNoErrors() && startingStatus != stripeAccount.VerificationStatusId && stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                account.SendStripeAccountVerified(null);

            if (msgs.HasErrors())
                return Return500(msgs);

            return ReturnOK(msgs);
        }

        private IHttpActionResult InvoicePaymentSucceeded(StripeEvent stripeEvent, SystemMessages msgs)
        {
            var customerId = (string)stripeEvent.Data.Object.customer;
            var subscriptionId = (string)stripeEvent.Data.Object.subscription;
            var chargeId = (string)stripeEvent.Data.Object.charge;
            var totalAmountInCents = ((string)stripeEvent.Data.Object.total).ToDecimal();
            var registrantSubscription = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(subscriptionId, msgs);
            var plan = registrantSubscription.GetSubscriptionPlan(msgs);
            var registrant = registrantSubscription.GetRegistrant(msgs);
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
                    // send payment succeeded email;
                    purchasingAccount.SendSubscriptionPaymentSucceeded(plan, null);

                    // save transaction info and track payment to the activity as well.
                    Database.ExecuteTransaction
                    (
                        action: (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            var totalAmount = totalAmountInCents / 100;                            

                            // manage transaction
                            AccountService.CreatePurchaseTransactionPayment(
                                new CheckoutObject()
                                {
                                    PurchasingAccount = purchasingAccount,
                                    BillingAccount = billingAccount,
                                    OverrideAmount = totalAmount,
                                    ParticipationObject = participationObject,
                                    PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                                    PaymentType = PaymentType.CreditCard,
                                    StripeChargeId = chargeId,
                                    SubscriptionPlan = plan,
                                    MasterRegistrant = masterRegistrant,
                                    NumberOfSeats = 1,
                                    Registration = registration,
                                    RegistrantSubscriptionPlan = registrantSubscription
                                },
                                null,
                                amsgs,
                                adc
                            );

                            // cancel stripe subscription at end of term
                            if (plan.PlanTypeId == SubscriptionPlanType.PaymentPlan &&
                                registrantSubscription.GetPaymentCount(amsgs, adc) >= plan.TotalPeriods.Value)
                                amsgs.AddRange(AfterSchoolLibrary.Billing.StripeHelper.CancelSubscription(subscriptionId, billingAccount.StripeUserId));
                        },
                        onSuccess: (SystemMessages smsgs) =>
                        {
                            msgs.AddRange(smsgs);
                        },
                        onFailure: (SystemMessages fmsgs) =>
                        {
                            msgs.AddRange(fmsgs);
                        },
                        onException: (SystemMessages emsgs) =>
                        {
                            AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs);
                        }
                    );
                }
            }

            if (msgs.HasErrors())
                return Return500(msgs);

            return ReturnOK(msgs);
        }

        private IHttpActionResult InvoicePaymentFailed(StripeEvent stripeEvent, SystemMessages msgs)
        {
            var customerId = (string)stripeEvent.Data.Object.customer;
            var account = Account.LoadByStripeCustomerId(customerId, msgs);
            var subscriptionId = (string)stripeEvent.Data.Object.subscription;
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
