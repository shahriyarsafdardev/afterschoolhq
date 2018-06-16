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
    public class AccountFactory
    {
        public static Account Create(WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
