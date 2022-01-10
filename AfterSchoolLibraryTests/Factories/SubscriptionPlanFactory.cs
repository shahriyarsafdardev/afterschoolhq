using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class SubscriptionPlanFactory
    {
        public static SubscriptionPlan Create(Account planOwnerAccount, Activity activity, SubscriptionPlanType planType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                CostPerPeriod = 49.95.ToDecimal(),
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = planType,
                IsActive = true,
                BillingPeriodTypeId = BillingPeriodType.Month
            };
            msgs.AddRange(obj.SaveWithStripe(dc: dc));
            return obj;
        }
        public static SubscriptionPlan Create(Account planOwnerAccount, Activity activity, SubscriptionPlanType planType, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                CostPerPeriod = costPerPeriod,
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = planType,
                IsActive = true,
                BillingPeriodTypeId = BillingPeriodType.Month,
                PaymentStartTypeId = PlanPaymentStartType.WhenActivityStarts
                
            };
            msgs.AddRange(obj.SaveWithStripe(dc: dc));
            return obj;
        }
        public static SubscriptionPlan CreatePaymentPlan(Account planOwnerAccount, Activity activity, decimal costPerPeriod, int totalPeriods, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                CostPerPeriod = costPerPeriod,
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = SubscriptionPlanType.PaymentPlan,
                IsActive = true,
                TotalPeriods = totalPeriods,               
                BillingPeriodTypeId = BillingPeriodType.Month,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(totalPeriods)
            };
            msgs.AddRange(obj.SaveWithStripe(dc));
            return obj;
        }

        public static SubscriptionPlan Create(Account planOwnerAccount, Activity activity, decimal costPerPeriod, SubscriptionPlanType planType, BillingPeriodType billingPeriodType, PlanPaymentStartType planPaymentStartType,  SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                CostPerPeriod = costPerPeriod,
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = planType,
                IsActive = true,
                TotalPeriods = null,
                BillingPeriodTypeId = billingPeriodType,
                PaymentStartTypeId = planPaymentStartType
            };
            msgs.AddRange(obj.SaveWithStripe(dc));
            return obj;
        }

        public static SubscriptionPlan CreatePaymentPlan(Account planOwnerAccount, Activity activity, decimal costPerPeriod, int totalPeriods, BillingPeriodType billingPeriodType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                CostPerPeriod = costPerPeriod,
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = SubscriptionPlanType.PaymentPlan,
                IsActive = true,
                TotalPeriods = totalPeriods,
                BillingPeriodTypeId = billingPeriodType
            };
            msgs.AddRange(obj.SaveWithStripe(dc));
            return obj;
        }

        public static SubscriptionPlan CreatePaymentPlan(Account planOwnerAccount, Activity activity, decimal costPerPeriod, int totalPeriods, BillingPeriodType billingPeriodType, PlanPaymentStartType planPaymentStartType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                CostPerPeriod = costPerPeriod,
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = SubscriptionPlanType.PaymentPlan,
                IsActive = true,
                TotalPeriods = totalPeriods,
                BillingPeriodTypeId = billingPeriodType,
                PaymentStartTypeId = planPaymentStartType
            };
            msgs.AddRange(obj.SaveWithStripe(dc));
            return obj;
        }

        public static SubscriptionPlan Create(Account planOwnerAccount, ActivityGroup activityGroup, SubscriptionPlanType planType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activityGroup.ID,
                ParticipationObjectTypeId = ParticipationObjectType.ActivityGroup,
                CostPerPeriod = 49.95.ToDecimal(),
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = planType,
                IsActive = true,
                BillingPeriodTypeId = BillingPeriodType.Month
            };
            msgs.AddRange(obj.SaveWithStripe(dc: dc));
            return obj;
        }
        public static SubscriptionPlan Create(Account planOwnerAccount, ActivityGroup activityGroup, SubscriptionPlanType planType, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SubscriptionPlan()
            {
                ParticipationObjectId = activityGroup.ID,
                ParticipationObjectTypeId = ParticipationObjectType.ActivityGroup,
                CostPerPeriod = costPerPeriod,
                PlanOwnerAccountId = planOwnerAccount.ID,
                PlanTypeId = planType,
                IsActive = true,
                BillingPeriodTypeId = BillingPeriodType.Month
            };
            msgs.AddRange(obj.SaveWithStripe(dc: dc));
            return obj;
        }
    }
}
