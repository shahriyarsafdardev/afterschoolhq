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
    public class AccountWebUserFactory
    {
        public static AccountWebUser Create(Account account, WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new AccountWebUser() { AccountId = account.ID, WebUserId = user.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
