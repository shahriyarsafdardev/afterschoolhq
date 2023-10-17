using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
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
using Account = AfterSchoolLibrary.Models.Account;
using SubscriptionSchedule = AfterSchoolLibrary.Models.SubscriptionSchedule;
using SubscriptionScheduleService = AfterSchoolLibrary.Services.SubscriptionScheduleService;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

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
                var serializedJson = JsonConvert.SerializeObject(json);
                WebhookPayload webhookPayload = JsonConvert.DeserializeObject<WebhookPayload>(serializedJson);

                var subscriptionPaymentHistory = webhookPayload.PaymentHistory;
                var subscriptionSchedule = webhookPayload.Schedule;
                var charge = webhookPayload.Charge;
                var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);

                ChargePendingToday(charge, subscriptionSchedule, subscriptionPaymentHistory, msgs, null);

                if (msgs.HasNoErrors())
                    return ReturnOK(msgs);
                else
                    return Return500(msgs);
            }
            catch (Exception ex)
            {
                Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                return Return500(msgs);
            }
        }

        private void ChargePendingToday(Charge charge, SubscriptionSchedule subscriptionSchedule,
            SubscriptionPaymentHistory subscriptionPaymentHistory, SystemMessages msgs, DatabaseConnection dc)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    HandleCharge(charge, subscriptionSchedule, subscriptionPaymentHistory, chargeStatus, msgs);
                    CreatePaymentRecords(subscriptionSchedule, subscriptionPaymentHistory, charge, msgs, dc);
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
            );
        }

        private void CreatePaymentRecords(SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory subscriptionPaymentHistory,
            Charge charge, SystemMessages msgs, DatabaseConnection dc)
        {
            var chargeStatus = (LastChargeStatus) Enum.Parse(typeof(LastChargeStatus), charge.Status);
            var rsp = RegistrantSubscriptionPlan.LoadBySubscriptionScheduleUid(subscriptionSchedule.Uid, msgs, dc);

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

            Account convenienceFeePayer;
            Account creditCardFeePayer;

            if (rsp.ConvenienceFeesPaidBy == FeesPaidByType.Parent)
                convenienceFeePayer = purchasingAccount;
            else
                convenienceFeePayer = billingAccount;
            if (rsp.PaymentTypeFeesPaidBy == FeesPaidByType.Parent)
                creditCardFeePayer = purchasingAccount;
            else
                creditCardFeePayer = billingAccount;


            var useConnectedAccount = billingAccount.UseStripeConnectDuringCheckout(msgs, dc);
            var usePlatformFees = useConnectedAccount && subscriptionPaymentHistory.AmountInCents.IsEmpty();
            var platFormFlatFee = (!participationObject.IsRegistrationFree) ? billingAccount.GetPlatformConvenienceFlatFee(msgs, dc) : 0;
            var platformPercentFee = billingAccount.GetPlatformConveniencePercentFee(msgs, dc);

            var amountPaidByParent = subscriptionSchedule.AmountInCents;

            var stripeSubscription = StripeHelper.GetStripeHelperConnectAccount().GetSubscription(rsp.StripeSubscriptionId, msgs);
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

            // TODO: commenting for now as dont have stripeInvoice
            //if (stripeInvoice.TransferData.IsNotEmpty())
            //{
            //    amountReceivedByProvider = (decimal) stripeInvoice.TransferData.Amount / 100;
            //    if (amountPaidByParent - paymentTypeFeeOverrideAmount - convenienceFeeOverrideAmount != amountReceivedByProvider)
            //        convenienceFeeOverrideAmount = amountPaidByParent - paymentTypeFeeOverrideAmount.Value - amountReceivedByProvider.Value;
            //}

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                OverrideAmount = amountPaidByParent,
                AmountPaidByParent = amountPaidByParent,
                AmountReceivedByProvider = amountReceivedByProvider,
                IsAmountProRated = isAmountProrated,
                ConvenienceFeeOverrideAmount = convenienceFeeOverrideAmount,
                PaymentTypeFeeOverrideAmount = paymentTypeFeeOverrideAmount,
                ParticipationObject = participationObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = rsp.PaymentTypeId,
                StripeChargeId = charge.Id,
                SubscriptionPlan = plan.PlanTypeId == SubscriptionPlanType.Subscription ? plan : null,
                PaymentPlan = plan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? plan : null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                RegistrantSubscriptionPlan = rsp,
                ConvenienceFeePayer = convenienceFeePayer,
                CreditCardFeePayer = creditCardFeePayer,
            };

            if (chargeStatus.Equals(LastChargeStatus.succeeded))
            {
                billingAccount.ProcessSubscriptionPlanPayment(checkoutObject, plan, rsp, msgs);

                if (msgs.HasNoErrors())
                    checkoutObject.PayingAccount.SendSubscriptionPaymentSucceeded(checkoutObject);
                else
                    msgs.Add("Error creating payment records", MessageType.Error);
            }
            else
            {
                TransactionService.CreateNew(
                purchasingAccount,
                billingAccount,
                convenienceFeePayer,
                creditCardFeePayer,
                subscriptionPaymentHistory.AmountInCents,
                participationObject?.Name,
                null, // as we have'nt created purchase yet.
                TransactionType.Charge,
                rsp.PaymentTypeId,
                PaymentSourceType.AfterSchoolPlatform,
                usePlatformFees ? platFormFlatFee : new decimal?(),
                usePlatformFees ? platformPercentFee : new decimal?(),
                convenienceFeeOverrideAmount, //need to revisit
                paymentTypeFeeOverrideAmount ?? 0, // need to revisit
                subscriptionPaymentHistory.StripeChargeId,
                subscriptionSchedule.Description,
                DateTime.UtcNow,
                useConnectedAccount,
                null,
                null,
                null,
                msgs,
                dc);

                if (msgs.HasNoErrors())
                    checkoutObject.PayingAccount.SendSubscriptionPaymentFailed(checkoutObject.SubscriptionPlan, dc);
                else
                    msgs.Add("Error creating Transaction record", MessageType.Error);
            }
        }

        private void HandleCharge(Charge charge, SubscriptionSchedule subscriptionSchedule, SubscriptionPaymentHistory? subscriptionPaymentHistory,
            LastChargeStatus chargeStatus, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (chargeStatus.Equals(LastChargeStatus.succeeded))
            {
                subscriptionSchedule.PaymentsDone += 1;

                subscriptionSchedule.Metadata = JsonConvert.SerializeObject(charge.Metadata);

                //UpdateNextBillDateOnSuccess(subscriptionSchedule);

                if (subscriptionPaymentHistory.Status.Equals(LastChargeStatus.failed))
                    subscriptionPaymentHistory.Status = LastChargeStatus.succeeded;
            }
            else if (charge.Status.Equals(LastChargeStatus.failed))
            {
                msgs.Add("Charge failed", MessageType.Error);

                if (subscriptionPaymentHistory.RetryNumber < 3)
                {
                    subscriptionSchedule.LastChargeStatus = LastChargeStatus.failed;
                    subscriptionPaymentHistory.RetryNumber += 1;
                }
                else
                {
                    subscriptionSchedule.IsCancelled = true;
                    subscriptionSchedule.NextBillDate = null;
                    subscriptionSchedule.EndDate = DateTime.UtcNow;
                    subscriptionSchedule.CancelledReason = "Payment for subscription failed";
                }
            }
            else
            {
                subscriptionPaymentHistory.Status = LastChargeStatus.pending;
                EmailHelper.SendUpcomingSubscriptionPaymentReminder(subscriptionSchedule, true);
            }

            msgs.AddRange(subscriptionSchedule.Save());
            msgs.AddRange(subscriptionPaymentHistory.Save());
        }

        private void UpdateNextBillDateOnSuccess(SubscriptionSchedule subscriptionSchedule)
        {
            // was first payment 
            if (subscriptionSchedule.PaymentsDone.Equals(1))
                subscriptionSchedule.NextBillDate = subscriptionSchedule.FirstRenewal;

            //was last payment
            else if (subscriptionSchedule.NextBillDate.Value.Date.Equals(subscriptionSchedule.LastDateToBill.Value.Date))
                subscriptionSchedule.NextBillDate = null;

            else
                subscriptionSchedule.NextBillDate = new SubscriptionPlan
                {
                    BillingPeriodTypeId = subscriptionSchedule.BillingPeriodType
                }.CalculateEndDateFromNumberOfPeriods((DateTime) subscriptionSchedule.NextBillDate, 1);
        }

        private IHttpActionResult ReturnOK(SystemMessages msgs)
        {
            return Ok(msgs.ToJSON());
        }
        private IHttpActionResult Return500(SystemMessages msgs)
        {
            return InternalServerError(new Exception(msgs.RenderMessageList(MessageOutputType.Text)));
        }
    }
}