using System;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantSubscriptionPlanService
    {
        public static RegistrantSubscriptionPlan CreateNew(Registrant registrant, SubscriptionPlan plan, DateTime startDate, int? purchaseId, string stripeSubscriptionId, FeeObject feeObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var rpp = new RegistrantSubscriptionPlan()
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                SubscriptionPlanId = plan.ID,
                RegistrantId = registrant.ID,
                StartDate = startDate,
                PurchaseId = purchaseId,
                StripeSubscriptionId = stripeSubscriptionId,
                ConvenienceFee = feeObject.ConvenienceFee,
                PaymentTypeFee = feeObject.PaymentTypeFee,
                ConvenienceFeesPaidBy = feeObject.ConvenienceFeesPaidBy,
                PaymentTypeFeesPaidBy = feeObject.PaymentTypeFeesPaidBy,
                Amount = feeObject.Amount
            };

            msgs.AddRange(rpp.Save(dc));

            return rpp;
        }

        public static List<ActivityPayment> GetPayments(this RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var plan = registrantSubscriptionPlan.GetSubscriptionPlan(msgs, dc);
            return ActivityPayment.LoadByParticipationObjectByRegistrant(
                plan.ParticipationObjectId.Value,
                plan.ParticipationObjectTypeId,
                registrantSubscriptionPlan.RegistrantId.Value,
                msgs,
                dc);
        }

        public static int GetPaymentCount(this RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var plan = registrantSubscriptionPlan.GetSubscriptionPlan(msgs, dc);
            return ActivityPayment.LoadByParticipationObjectByRegistrant(
                plan.ParticipationObjectId.Value,
                plan.ParticipationObjectTypeId,
                registrantSubscriptionPlan.RegistrantId.Value,
                msgs,
                dc)
                .Count(payment => payment.Amount.Value > 0);
        }

        public static ActivityPayment AddPayment(this RegistrantSubscriptionPlan registrantSubscriptionPlan, Transaction transaction, decimal amount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var plan = registrantSubscriptionPlan.GetSubscriptionPlan(msgs, dc);

            var payment = new ActivityPayment()
            {
                ParticipationObjectId = plan.ParticipationObjectId,
                ParticipationObjectTypeId = plan.ParticipationObjectTypeId,
                Amount = amount,
                RegistrantId = registrantSubscriptionPlan.RegistrantId,
                AccountId = plan.PlanOwnerAccountId,
                TransactionId = transaction.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0)
            };
            msgs.AddRange(payment.Save(dc));

            return payment;
        }
    }
}
