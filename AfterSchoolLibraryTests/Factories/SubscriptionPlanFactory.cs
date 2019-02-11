using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

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
                PlanTypeId = planType
            };
            msgs.AddRange(obj.Save(dc: dc));
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
                PlanTypeId = planType
            };
            msgs.AddRange(obj.Save(dc: dc));
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
                PlanTypeId = planType
            };
            msgs.AddRange(obj.Save(dc: dc));
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
                PlanTypeId = planType
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
