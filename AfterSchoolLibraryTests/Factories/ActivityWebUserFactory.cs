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
    public class ActivityWebUserFactory
    {
        public static ActivityWebUser Create(Activity activity, WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivityWebUser() { ActivityId = activity.ID, WebUserId = user.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static ActivityWebUser Create(Activity activity, WebUser user, bool isLeader, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivityWebUser() { ActivityId = activity.ID, WebUserId = user.ID, IsLeader = isLeader };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
