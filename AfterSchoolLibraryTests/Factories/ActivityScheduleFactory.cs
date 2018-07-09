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
        public static ActivitySchedule Create(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new ActivitySchedule() { Name = "Session 4 Schedule", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(35), DailyStartTime = DateTime.UtcNow, DailyEndTime = DateTime.UtcNow.AddHours(2), AccountId = account.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

    }
}
