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
    public class AccountPropertyFactory
    {
        public static AccountProperty Create(Account account, AccountPropertyType accountPropertyType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new AccountProperty() { AccountId = account.ID, AccountPropertyTypeId = accountPropertyType };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static AccountProperty Create(ActivityGroup activityGroup, AccountPropertyType accountPropertyType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountProperty() { ActivityGroupId = activityGroup.ID, AccountPropertyTypeId = accountPropertyType };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static AccountProperty Create(Activity activity, AccountPropertyType accountPropertyType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new AccountProperty() { ActivityId = activity.ID, AccountPropertyTypeId = accountPropertyType };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
