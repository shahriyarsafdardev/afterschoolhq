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
            var obj = new RegistrationPeriod() { };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
