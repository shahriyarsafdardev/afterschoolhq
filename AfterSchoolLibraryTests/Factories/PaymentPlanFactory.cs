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
        public static PaymentPlan Create(Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new PaymentPlan() { ActivityId = activity.ID, TotalPeriods = 5, CostPerPeriod = 30 };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static PaymentPlan Create(Activity activity, int totalPeriods, decimal costPerPeriod, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new PaymentPlan() { ActivityId = activity.ID, TotalPeriods = totalPeriods, CostPerPeriod = costPerPeriod };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
