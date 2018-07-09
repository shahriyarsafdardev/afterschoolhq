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
    public class RegistrationPeriodFactory
    {
        public static RegistrationPeriod Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new RegistrationPeriod()
            {
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(7),
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
