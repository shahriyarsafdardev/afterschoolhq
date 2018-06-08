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
    public class RegistrantFactory
    {
        public static Registrant CreateRegistrant(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrant = new Registrant();
            msgs.AddRange(registrant.Save(dc));
            return registrant;
        }
    }
}
