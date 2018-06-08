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
    public class ActivityInstanceFactory
    {
        public static ActivityInstance Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivityInstance() { };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static ActivityInstance Create(Activity activity, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivityInstance() { ActivityId = activity.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
