using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class SubscriptionPlanFactory
    {
        public static SubscriptionPlan Create(Account planOwnerAccount, Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { ParticipationSourceId = activity.ID, ParticipationSourceTypeId = ParticipationSourceType.Activity, CostPerPeriod = 49.95.ToDecimal(), PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static SubscriptionPlan Create(Account planOwnerAccount, Activity activity, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { ParticipationSourceId = activity.ID, ParticipationSourceTypeId = ParticipationSourceType.Activity, CostPerPeriod = costPerPeriod, PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static SubscriptionPlan Create(Account planOwnerAccount, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { ParticipationSourceId = activityGroup.ID, ParticipationSourceTypeId = ParticipationSourceType.ActivityGroup, CostPerPeriod = 49.95.ToDecimal(), PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static SubscriptionPlan Create(Account planOwnerAccount, ActivityGroup activityGroup, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { ParticipationSourceId = activityGroup.ID, ParticipationSourceTypeId = ParticipationSourceType.ActivityGroup, CostPerPeriod = costPerPeriod, PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
