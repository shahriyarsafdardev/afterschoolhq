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
    public class RegistrationFactory
    {
        public static Registration Create(Account account, Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Registration() { AccountId = account.ID, RegistrantId = registrant.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
