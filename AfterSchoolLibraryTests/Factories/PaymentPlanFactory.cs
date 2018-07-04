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
    public class PaymentPlanFactory
    {
        public static PaymentPlan Create(Account planOwnerAccount, Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new PaymentPlan() { ParticipationObjectId = activity.ID, ParticipationObjectTypeId = ParticipationObjectType.Activity, TotalPeriods = 5, CostPerPeriod = 30, PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static PaymentPlan Create(Account planOwnerAccount, Activity activity, int totalPeriods, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new PaymentPlan() { ParticipationObjectId = activity.ID, ParticipationObjectTypeId = ParticipationObjectType.Activity, TotalPeriods = totalPeriods, CostPerPeriod = costPerPeriod, PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static PaymentPlan Create(Account planOwnerAccount, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new PaymentPlan() { ParticipationObjectId = activityGroup.ID, ParticipationObjectTypeId = ParticipationObjectType.ActivityGroup, TotalPeriods = 5, CostPerPeriod = 30, PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static PaymentPlan Create(Account planOwnerAccount, ActivityGroup activityGroup, int totalPeriods, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new PaymentPlan() { ParticipationObjectId = activityGroup.ID, ParticipationObjectTypeId = ParticipationObjectType.ActivityGroup, TotalPeriods = totalPeriods, CostPerPeriod = costPerPeriod, PlanOwnerAccountId = planOwnerAccount.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
