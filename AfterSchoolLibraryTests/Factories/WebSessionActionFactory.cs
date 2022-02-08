using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibraryTests.Factories
{
    public class WebSessionActionFactory
    {
        public static WebSessionAction Create(Account account, Activity activity, WebSession webSession, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new WebSessionAction() { AccountId = account.ID.Value, ObjectId = activity.ID.Value, ObjectTypeId = ObjectType.Activity, SessionId = webSession.ID.Value, ActionString = "Test Create" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static WebSessionAction CreateWithActivityGroup(Account account, ActivityGroup activityGroup, WebSession webSession, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new WebSessionAction() { AccountId = account.ID.Value, ObjectId = activityGroup.ID.Value, ObjectTypeId = ObjectType.ActivityGroup, SessionId = webSession.ID.Value, ActionString = "Test Create" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
