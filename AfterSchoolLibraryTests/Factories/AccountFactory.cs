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
        public static Account CreateAccount(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var account = new Account() { };
            msgs.AddRange(account.Save(dc));
            return account;
        }
    }
}
