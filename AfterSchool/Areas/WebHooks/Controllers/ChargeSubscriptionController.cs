using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using static AfterSchool.Controllers.AWSSubscriptionLambdaController;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;

namespace AfterSchool.Areas.WebHooks.Controllers
{
    public class ChargeSubscriptionController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Updated(dynamic json)
        {
            var msgs = new SystemMessages();
            try
            {
                msgs.Add("Getting serializedJson", MessageType.Notice);
                var serializedJson = JsonConvert.SerializeObject(json);
                WebhookPayload webhookPayload = JsonConvert.DeserializeObject<WebhookPayload>(serializedJson);

                var subscriptionPaymentHistory = webhookPayload.PaymentHistory;
                var subscriptionSchedule = webhookPayload.Schedule;

                //msgs.Add("Getting stripeEvent", MessageType.Notice);
                //var stripeEvent = (Stripe.Event) Stripe.EventUtility.ParseEvent(serializedJson, throwOnApiVersionMismatch: false);
                var stripeEvent = "invoice.payment_succeeded";

                switch (stripeEvent)
                {
                    case "invoice.payment_succeeded":
                        msgs.Add("Getting payment_succeeded", MessageType.Notice);
                        return InvoicePaymentSucceeded(subscriptionSchedule, subscriptionPaymentHistory, msgs, dc);
                    
                    default:
                        return ReturnOK(msgs);
                }
            }
            catch (Exception ex)
            {
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                return Return500(msgs);
            }
        }
        
        private IHttpActionResult InvoicePaymentSucceeded(SubscriptionSchedule subscriptionSchedule, 
            SubscriptionPaymentHistory subscriptionPaymentHistory, SystemMessages msgs, DatabaseConnection dc)
        {
            //var paymentIntent = (Stripe.PaymentIntent) stripeEvent.Data.Object;
            //var stripeInvoice = paymentIntent.Invoice;
            
            // new logic

            var rsp = RegistrantSubscriptionPlan.Load(135, msgs, dc: dc);
            var sc = new AfterSchoolLibrary.Models.SubscriptionSchedule();
            var sph = new AfterSchoolLibrary.Models.SubscriptionPaymentHistory();
            var plan = rsp.GetSubscriptionPlan(msgs);
            var registrant = rsp.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);
            var purchasingAccount = masterRegistrant.GetAccount(msgs);
            var billingAccount = plan.GetAccount(msgs);
            var participationObject = plan.GetParticipationObject(msgs);


            AfterSchoolLibrary.Models.Account convenienceFeePayer;
            AfterSchoolLibrary.Models.Account creditCardFeePayer;

            if (rsp.ConvenienceFeesPaidBy == FeesPaidByType.Parent)
                convenienceFeePayer = purchasingAccount;
            else
                convenienceFeePayer = billingAccount;
            if (rsp.PaymentTypeFeesPaidBy == FeesPaidByType.Parent)
                creditCardFeePayer = purchasingAccount;
            else
                creditCardFeePayer = billingAccount;


            var useConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, dc);
            var usePlatformFees = useConnectedAccount && sph.AmountInCents.IsEmpty();
            var platFormFlatFee = (!participationObject.IsRegistrationFree) ? billingAccount.GetPlatformConvenienceFlatFee(msgs, dc) : 0;
            var platformPercentFee = billingAccount.GetPlatformConveniencePercentFee(msgs, dc);
            
         
            var amountPaidByParent = sc.AmountInCents;
            
            // get the original price
            var stripeSubscription = StripeHelper.GetStripeHelperConnectAccount().GetSubscription(plan.StripePlanId, msgs); //revisit
            var stripePrice = stripeSubscription.Items.FirstOrDefault(item => item.Price.IsNotEmpty()).Price;
            var stripeSubscriptionPrice = (decimal) stripePrice.UnitAmount / 100;
            var paymentType = rsp.PaymentTypeId;
            var isAmountProrated = false;
            
            // TODO: potentially problematic as the plan may have changed
            var convenienceFeeOverrideAmount = rsp.ConvenienceFee ?? plan.GetConvenienceFee(billingAccount, msgs);
            var paymentTypeFeeOverrideAmount = rsp.PaymentTypeFee.HasValue ? rsp.PaymentTypeFee : paymentType == PaymentType.CreditCard ? plan.GetCreditCardFee(billingAccount, msgs) : 0;
            var calculatedFullCCFee = StripeHelper.CalculateStripeCCFeeOutOfTotalCost(stripeSubscriptionPrice);


            if (paymentType == PaymentType.ACH)
                stripeSubscriptionPrice -= calculatedFullCCFee;

            isAmountProrated = stripeSubscriptionPrice > amountPaidByParent;
            
            if (isAmountProrated)
            {
                if (paymentType == PaymentType.CreditCard)
                    paymentTypeFeeOverrideAmount = StripeHelper.CalculateStripeCCFeeOutOfTotalCost(amountPaidByParent);
                else
                    paymentTypeFeeOverrideAmount = 0;
                var calcObj = new FeeCalculationObject(
                                amountPaidByParent - paymentTypeFeeOverrideAmount.Value,
                                billingAccount,
                                plan.IsConvenienceFeeTakenOutOfCost,
                                msgs);

                convenienceFeeOverrideAmount = calcObj.GetConvenienceFee();
            }

            var amountReceivedByProvider = amountPaidByParent - (convenienceFeeOverrideAmount + paymentTypeFeeOverrideAmount);

            //commenting for now as dont have stripeInvoice
            //if (stripeInvoice.TransferData.IsNotEmpty())
            //{
            //    amountReceivedByProvider = (decimal) stripeInvoice.TransferData.Amount / 100;
            //    if (amountPaidByParent - paymentTypeFeeOverrideAmount - convenienceFeeOverrideAmount != amountReceivedByProvider)
            //        convenienceFeeOverrideAmount = amountPaidByParent - paymentTypeFeeOverrideAmount.Value - amountReceivedByProvider.Value;
            //}

            TransactionService.CreateNew(
                purchasingAccount,
                billingAccount,
                convenienceFeePayer,
                creditCardFeePayer,
                sph.AmountInCents,
                participationObject?.Name,
                null, // as we have'nt created purchase yet.
                TransactionType.Charge,
                rsp.PaymentTypeId,
                PaymentSourceType.AfterSchoolPlatform,
                usePlatformFees ? platFormFlatFee : new decimal?(),
                usePlatformFees ? platformPercentFee : new decimal?(),
                convenienceFeeOverrideAmount, //need to revisit
                paymentTypeFeeOverrideAmount ?? 0, // need to revisit
                sph.StripeChargeId,
                sc.Description,
                DateTime.UtcNow,
                useConnectedAccount,
                SessionVariables.LoggedInUser,
                null,
                null,
                msgs,
                dc);
            return Return500(msgs);
        }



        private IHttpActionResult ReturnOK(SystemMessages msgs)
        {
            return Ok(msgs.ToJSON());
        }
        private IHttpActionResult Return500(SystemMessages msgs)
        {
            SendLogEmail(msgs);
            return InternalServerError(new Exception(msgs.RenderMessageList(MessageOutputType.Text)));
        }
        private void SendLogEmail(SystemMessages msgs)
        {
            string toEmail = "dev@afterschoolhq.com";
            string subject = $"Stripe Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
            string body = msgs.RenderMessageList(MessageOutputType.Html);

            AfterSchoolLibrary.Helpers.EmailHelper.SendEmail(toEmail, subject, body, true);
        }
    }
}