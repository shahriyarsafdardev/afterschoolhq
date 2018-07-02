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
        public static Registrant Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Registrant() { Email = "test@test.com" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Registrant Create(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Registrant() { Email = "test@test.com", AccountId = account.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
