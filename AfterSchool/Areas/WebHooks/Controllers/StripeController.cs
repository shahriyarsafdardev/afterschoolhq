using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
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
        msgs.Add("Getting serializedJson", MessageType.Notice);
        var serializedJson = JsonConvert.SerializeObject(json);
        msgs.Add("Getting stripeEvent", MessageType.Notice);
        var stripeEvent = (Stripe.Event)Stripe.EventUtility.ParseEvent(serializedJson, throwOnApiVersionMismatch: false);

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
          case "payment_intent.processing":
            return PaymentIntentProcessing(stripeEvent, msgs);
          case "customer.subscription.deleted":
            return SubscriptionCompleted(stripeEvent, msgs);
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

    private void SendLogEmail(SystemMessages msgs)
    {
      string toEmail = "dev@afterschoolhq.com";
      string subject = $"Stripe Webhook Errors - {AfterSchoolLibrary.Helpers.Utilities.HostName}";
      string body = msgs.RenderMessageList(MessageOutputType.Html);

      AfterSchoolLibrary.Helpers.EmailHelper.SendEmail(toEmail, subject, body, true);
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

      CheckStripeAccountRequirements(stripeAccount, account, msgs);

      var updateChargesEnabled = !account.StripeChargesEnabled && stripeAccount.ChargesEnabled;
      var updatePayoutsEnabled = !account.StripePayoutsEnabled && stripeAccount.PayoutsEnabled;
      msgs.Add("Getting SendStripeAccountVerified", MessageType.Notice);

      if (msgs.HasNoErrors() && (updateChargesEnabled || updatePayoutsEnabled))
        account.UpdateStripeChargesAndPayouts(stripeAccount, msgs);

      if (msgs.HasErrors())
        return Return500(msgs);

      return ReturnOK(msgs);
    }

    private void CheckStripeAccountRequirements(Stripe.Account stripeAccount, AfterSchoolLibrary.Models.Account account, SystemMessages msgs)
    {
      var requirements = stripeAccount.Requirements;
      var requirementStatus = GetStripeRequirementStatus(requirements);
      if (requirementStatus > StripeRequirementStatus.NotSet)
      {
        var requirementList = new List<string>();
        var mostUrgentRequirements = requirements.PastDue;

        if (requirementStatus > StripeRequirementStatus.PastDue)
          mostUrgentRequirements = requirementStatus == StripeRequirementStatus.CurrentlyDue ? requirements.CurrentlyDue : requirements.EventuallyDue;

        mostUrgentRequirements.ForEach(req =>
        {
          var splitReq = req.Split('.');
          switch (splitReq[splitReq.Length - 1])
          {
            case "city":
              requirementList.Add("Address: City");
              break;
            case "line1":
              requirementList.Add("Address: Line1");
              break;
            case "postal_code":
              requirementList.Add("Address: Postal Code");
              break;
            case "state":
              requirementList.Add("Address: State");
              break;
            case "day":
              requirementList.Add("DOB: Day");
              break;
            case "month":
              requirementList.Add("DOB: Month");
              break;
            case "year":
              requirementList.Add("DOB: Year");
              break;
            case "title":
              requirementList.Add("Job Title");
              break;
            default:
              requirementList.Add(req);
              break;
          }
        });

        AfterSchoolLibrary.Helpers.EmailHelper.SendStripeHasUnfulFilledRequirementsEmail(stripeAccount, requirementList, requirementStatus, account, msgs);
      }

    }

    private StripeRequirementStatus GetStripeRequirementStatus(Stripe.AccountRequirements requirements)
    {
      if (requirements.PastDue.Count > 0)
        return StripeRequirementStatus.PastDue;

      if (requirements.CurrentlyDue.Count > 0)
        return StripeRequirementStatus.CurrentlyDue;

      if (requirements.EventuallyDue.Count > 0)
        return StripeRequirementStatus.EventuallyDue;

      return StripeRequirementStatus.NotSet;
    }

    private IHttpActionResult PaymentIntentProcessing(Stripe.Event stripeEvent, SystemMessages msgs)
    {
      var paymentIntent = (Stripe.PaymentIntent)stripeEvent.Data.Object;
      var stripeInvoice = paymentIntent.Invoice;

      if (stripeInvoice.IsNotEmpty())
        return ProcessInvoiceDetails(stripeInvoice, msgs);

      return ReturnOK(msgs);
    }

    private IHttpActionResult InvoicePaymentSucceeded(Stripe.Event stripeEvent, SystemMessages msgs)
    {
      var stripeInvoice = (Stripe.Invoice)stripeEvent.Data.Object;

      if (stripeInvoice.IsNotEmpty())
        return ProcessInvoiceDetails(stripeInvoice, msgs);

      return Return500(msgs);
    }

    private IHttpActionResult ProcessInvoiceDetails(Stripe.Invoice stripeInvoice, SystemMessages msgs)
    {
      var stripeSubscriptionId = stripeInvoice.SubscriptionId;
      var chargeId = stripeInvoice.ChargeId;
      var rsp = RegistrantSubscriptionPlan.LoadByStripeSubscriptionId(stripeSubscriptionId, msgs);

      if (rsp.IsEmpty())
      {
        msgs.Add($"Ignoring - Registrant Subscription Plan not found - Stripe Sub ID {stripeSubscriptionId}", MessageType.Notice);
        return ReturnOK(msgs);
      }

      var status = "succeeded";

      if (stripeInvoice.PaymentIntent.IsNotEmpty())
      {
        msgs.Add($"Checking Payment Intent Status {stripeInvoice.PaymentIntent.Status} - Stripe Sub ID {stripeSubscriptionId}", MessageType.Notice);
        status = stripeInvoice.PaymentIntent.Status;
      }

      if (stripeInvoice.AmountPaid <= 0 && status.NotIn(new string[] { "processing", "succeeded" }))
      {
        msgs.Add($"Ignoring - $0 was paid with no processing payment intent", MessageType.Notice);
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
      var isStripeInvoiceTransferData = stripeInvoice.TransferData.IsNotEmpty();
      if (transaction.IsNotEmpty())
      {
        msgs.Add($"Ignoring - Transaction has already been processed - Stripe Sub ID {stripeSubscriptionId}", MessageType.Notice);
        msgs.Add($"TransactionID {transaction.ID.Value}", MessageType.Notice);
        return ReturnOK(msgs);
      }

      // send payment succeeded email, unless payment was made before today (for previously failed webhooks)
      var wasPaidToday = (bool)stripeInvoice.StatusTransitions?.PaidAt.IsNotEmpty() && stripeInvoice.StatusTransitions.PaidAt.Value.Date >= DateTime.Today.Date;
      if (wasPaidToday)
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

      var charge = AfterSchoolLibrary.Billing.StripeHelper.GetCharge(stripeInvoice.ChargeId, msgs);
      var amountRefunded = charge.IsNotEmpty() && charge.AmountRefunded.IsNotEmpty() ? charge.AmountRefunded : 0m;
      //we will need this logic to ensure we are adding pro-rated amount in ActivityPayment and RegistrantSubscriptionPlan
      var amountPaidByParent = stripeInvoice.AmountPaid > 0 ? ((decimal)stripeInvoice.AmountPaid / 100) : 0m;
      // NOTE: need to do a check if our data is different than stripe. What if stripe.InvoiceAmount != rsp.Amount ?
      var amountReceivedByProvider = amountPaidByParent - (rsp.ConvenienceFee + rsp.PaymentTypeFee);

      if (isStripeInvoiceTransferData)
        amountReceivedByProvider = (decimal)stripeInvoice.TransferData.Amount / 100;

      var isAmountProRated = amountReceivedByProvider < plan.CostPerPeriod;

      // save transaction info and track payment to the activity as well.
      var checkoutObject = new CheckoutObject()
      {
        PayingAccount = purchasingAccount,
        BillingAccount = billingAccount,
        OverrideAmount = amountPaidByParent,
        AmountPaidByParent = amountPaidByParent,
        AmountReceivedByProvider = amountReceivedByProvider,
        IsAmountProRated = isAmountProRated,
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

      // dont process if we made a full refund, otherwise create a transaction with amount - amount refunded
      if (amountRefunded != charge.Amount)
      {
        billingAccount.ProcessSubscriptionPlanPayment(checkoutObject, plan, rsp, msgs);
        msgs.Add($"New Transaction Created {checkoutObject.Transaction.ID.Value}", MessageType.Notice);
        // only want to create a transfer if there isnt an existing one from stripe
        if (!isStripeInvoiceTransferData && msgs.HasNoErrors())
          checkoutObject.Transaction.CreateStripeTransfer(WebUser.Load(1, msgs), msgs);

        if (amountRefunded > 0 && msgs.HasNoErrors())
        {
          msgs.Add($"Partial Refund: {amountRefunded}", MessageType.Notice);

          var stripeRefunds = StripeHelper.GetRefunds(chargeId, msgs);
          stripeRefunds.ForEach(stripeRefund =>
          {
            if (stripeRefund.TransferReversal.IsEmpty())
            {
                        // NOTE: check if refund has a transfer, not sure how/if to handle
                      }

                      // check if transaction exists for refund
                      var refundOriginalTransaction = Transaction.LoadByStripeChargeOrRefundId(stripeRefund.Id, msgs);

                      // if no refund record exists in our db, create it from the transaction added to the checkoutObject
                      // NOTE: not sure how to handle fees on partial refunds??
                      if (refundOriginalTransaction.IsEmpty())
              refundOriginalTransaction = checkoutObject.Transaction;

            refundOriginalTransaction.CreateNewRefund(
                          stripeRefund.Amount,
                          0,
                          0,
                          stripeRefund.Id,
                          TransactionType.Refund,
                          refundOriginalTransaction.PaymentTypeId,
                          refundOriginalTransaction.PaymentSourceTypeId,
                          WebUser.Load(1, msgs),
                          msgs
                          );

          });
        }
      }
      else
      {
        msgs.Add("Full refund already Made", MessageType.Notice);
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

    private IHttpActionResult SubscriptionCompleted(Stripe.Event stripeEvent, SystemMessages msgs)
    {
      var subscription = (Stripe.Subscription)stripeEvent.Data.Object;

      if (subscription.Plan.IsNotEmpty() && subscription.Plan.Id.IsNotEmpty())
      {
        var dbSubscription = SubscriptionPlan.LoadByStripePlanId(subscription.Plan.Id, msgs);
        dbSubscription.IsActive = false;
        dbSubscription.Save();
      }

      return ReturnOK(msgs);
    }
  }
}