using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountWebUserFactory
    {
        public static AccountWebUser Create(Account account, WebUser user, UserType ut, bool isActive, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountWebUser() { AccountId = account.ID, WebUserId = user.ID, UserTypeId = ut , IsActive = isActive};
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
