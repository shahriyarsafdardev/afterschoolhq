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
    public class ActivityMessageFactory
    {
        public static ActivityMessage Create(Account account, Activity activity, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityMessage() { AccountId = account.ID, ActivityId = activity.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
