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
    public class ActivityScheduleFactory
    {
        public static ActivitySchedule Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivitySchedule() { Name = "Session 4 Schedule", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(35), DailyStartTime = DateTime.UtcNow, DailyEndTime = DateTime.UtcNow.AddHours(2), AccountId = account.ID };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static ActivitySchedule Create(Account account, ParticipationObject participationObject, ParticipationObjectType participationObjectType, SystemMessages msgs, DatabaseConnection dc, ActivityGroup activityGroup = null)
        {
            var activitySchedule = new ActivitySchedule()
            {
                Name = "Test Activity Schedule",
                AccountId = account.ID.Value,
                ActivityGroupId = activityGroup?.ID.Value,
                ScheduleDetails = "Test Schedule Details",
                RepeatsWeekly = false,
                IsOngoing = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                DailyStartTime = null,
                DailyEndTime = null
            };

            msgs.AddRange(activitySchedule.Save(dc));
            return activitySchedule;

        }
    }
}
