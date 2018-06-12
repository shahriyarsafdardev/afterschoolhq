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
        public static SubscriptionPlan Create(Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { ActivityId = activity.ID, CostPerPeriod = 49.95.ToDecimal() };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static SubscriptionPlan Create(Activity activity, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { ActivityId = activity.ID, CostPerPeriod = costPerPeriod };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
