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
    public class RegistrationActivityFactory
    {
        public static RegistrationActivity Create(Activity activity, Registration registration, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new RegistrationActivity() { ActivityId = activity.ID, RegistrationId = registration.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
