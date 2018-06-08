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
    public class ActivityFactory
    {
        public static Activity CreateActivity(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var af = new AccountFactory();
            var activity = new Activity() { AccountId = account.ID };
            msgs.AddRange(activity.Save(dc));
            return activity;
        }
    }
}
