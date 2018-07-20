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
    public class ActivityRegistrationPeriodFactory
    {
        public static ActivityRegistrationPeriod Create(Activity activity, RegistrationPeriod registrationPeriod, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityRegistrationPeriod()
            {
                ActivityId = activity.ID,
                RegistrationPeriodId = registrationPeriod.ID,
                IsActive = true
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
