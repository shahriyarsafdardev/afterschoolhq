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
    public class RegistrantSubscriptionPlanFactory
    {
        public static RegistrantSubscriptionPlan Create(Registrant registrant, SubscriptionPlan subscriptionPlan, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantSubscriptionPlan()
            {
                RegistrantId = registrant.ID,
                SubscriptionPlanId = subscriptionPlan.ID,
                StartDate = DateTime.UtcNow
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
