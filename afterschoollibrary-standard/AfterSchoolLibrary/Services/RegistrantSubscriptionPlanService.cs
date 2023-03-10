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
        public static RegistrantSubscriptionPlan CreateNew(Registrant registrant, SubscriptionPlan plan, DateTime startDate, int? purchaseId, string stripeSubscriptionId, PlanFeeObject feeObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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
                Amount = feeObject.Amount,
                TotalPeriods = feeObject.TotalPeriods,
                PaymentTypeId = feeObject.PaymentType,
                BillingPeriodTypeId = feeObject.BillingPeriodType
            };

            msgs.AddRange(rpp.Save(dc));

            return rpp;
        }

        public static List<ActivityPayment> GetPayments(this RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByRegistrantSubscriptionPlanIdByIsDeleted(
                registrantSubscriptionPlan.ID.Value,
                false,
                msgs,
                dc);
        }

        public static int GetPaymentCount(this RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantSubscriptionPlan.GetPayments(msgs, dc).Count(payment => payment.Amount.Value > 0);
        }

        public static ActivityPayment AddPayment(this RegistrantSubscriptionPlan registrantSubscriptionPlan, Transaction transaction, decimal amount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var plan = registrantSubscriptionPlan.GetSubscriptionPlan(msgs, dc);

            var payment = new ActivityPayment()
            {
                ParticipationObjectId = plan.ParticipationObjectId,
                ParticipationObjectTypeId = plan.ParticipationObjectTypeId,
                Amount = amount,
                AmountPaidByParent = null,
                RegistrantId = registrantSubscriptionPlan.RegistrantId,
                AccountId = plan.PlanOwnerAccountId,
                TransactionId = transaction.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                RegistrantSubscriptionPlanId = registrantSubscriptionPlan.ID,
            };
            msgs.AddRange(payment.Save(dc));

            return payment;
        }

        public static DateTime? GetLastPaymentDate(this RegistrantSubscriptionPlan rsp)
        {
            if (!rsp.TotalPeriods.HasValue) 
                return new DateTime?();
            return rsp.BillingPeriodTypeId switch
            {
                BillingPeriodType.Day => rsp.StartDate.Value.AddDays(rsp.TotalPeriods.Value),
                BillingPeriodType.Month => rsp.StartDate.Value.AddMonths(rsp.TotalPeriods.Value),
                BillingPeriodType.Week => rsp.StartDate.Value.AddWeeks(rsp.TotalPeriods.Value),
                BillingPeriodType.Year => rsp.StartDate.Value.AddYears(rsp.TotalPeriods.Value),
                _ => new DateTime?()
            };
        }

        public static DateTime? GetValidUntilDate(this RegistrantSubscriptionPlan rsp, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var lastPayment = rsp.GetPayments(msgs, dc).LastOrDefault();

            if (lastPayment.IsNotEmpty())
            {
                var billingPeriodType = rsp.BillingPeriodTypeId;

                if (billingPeriodType == BillingPeriodType.NotSet)
                    billingPeriodType = rsp.GetSubscriptionPlan(msgs, dc).BillingPeriodTypeId;

                switch(billingPeriodType)
                {
                    case BillingPeriodType.Day:
                        return lastPayment.CreateDate.Value.AddDays(1).AddMilliseconds(-1).Date;
                    case BillingPeriodType.Month:
                        return lastPayment.CreateDate.Value.AddMonths(1).AddMilliseconds(-1).Date;
                    case BillingPeriodType.Week:
                        return lastPayment.CreateDate.Value.AddWeeks(1).AddMilliseconds(-1).Date;
                    case BillingPeriodType.Year:
                        return lastPayment.CreateDate.Value.AddYears(1).AddMilliseconds(-1).Date;
                }
            }

            return null;
        }

        public static void MarkAsCancelled(this RegistrantSubscriptionPlan rsp, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {            
            rsp.DateCancelled = DateTime.UtcNow;
            rsp.ValidUntil = rsp.GetValidUntilDate(msgs, dc);
            rsp.LastModifiedDate = DateTime.UtcNow;
            rsp.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(rsp.Save(dc));
        }

        public static ParticipationObject GetParticipationObject(this RegistrantSubscriptionPlan registrantSubscriptionPlan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantSubscriptionPlan.GetSubscriptionPlan(msgs, dc).GetParticipationObject(msgs, dc);
        }

        public static bool IsForParticipationObject(this RegistrantSubscriptionPlan registrantSubscriptionPlan, ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var plan = registrantSubscriptionPlan.GetSubscriptionPlan(msgs, dc);

            if (plan.ParticipationObjectId == participationObject.ObjectID && plan.ParticipationObjectTypeId == participationObject.ObjectType)
                return true;

            return false;
        }
    }
}
