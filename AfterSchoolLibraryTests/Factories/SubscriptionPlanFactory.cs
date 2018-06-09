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
        public static SubscriptionPlan Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new SubscriptionPlan() { };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
