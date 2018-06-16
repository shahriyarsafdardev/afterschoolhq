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
    public class ActivityScheduleFactory
    {
        public static ActivitySchedule Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivitySchedule() { };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static ActivitySchedule Create(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivitySchedule() { AccountId = account.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
