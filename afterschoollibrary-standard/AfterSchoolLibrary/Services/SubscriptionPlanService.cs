using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionPlanService
    {
        public static SubscriptionPlan CreateNewSubscription(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscription(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscription(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewSubscription(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, doesAutoRenew, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewSubscription(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, bool doesAutoRenew, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var plan = new SubscriptionPlan()
            {
                PlanOwnerAccountId = planOwner.ID,
                BillingPeriodTypeId = billingPeriodType,
                CostPerPeriod = costPerPeriod,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                IsActive = true,
                ParticipationObjectId = participationObjectId,
                ParticipationObjectTypeId = participationObjectType,
                DoesAutoRenew = doesAutoRenew,
                PlanTypeId = SubscriptionPlanType.Subscription
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return CreateNewPaymentPlan(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public static SubscriptionPlan CreateNewPaymentPlan(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var plan = new SubscriptionPlan()
            {
                PlanOwnerAccountId = planOwner.ID,
                BillingPeriodTypeId = billingPeriodType,
                CostPerPeriod = costPerPeriod,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                IsActive = true,
                ParticipationObjectId = participationObjectId,
                ParticipationObjectTypeId = participationObjectType,
                TotalPeriods = totalPeriods,
                PlanTypeId = SubscriptionPlanType.PaymentPlan
            };

            msgs.AddRange(plan.SaveWithStripe(dc));

            return plan;
        }

        public static ParticipationObject GetParticipationObject(this SubscriptionPlan plan, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(plan.ParticipationObjectId.Value, plan.ParticipationObjectTypeId, msgs, dc);
        }
    }
}
