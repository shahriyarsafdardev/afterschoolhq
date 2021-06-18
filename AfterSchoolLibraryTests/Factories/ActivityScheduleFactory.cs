using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityScheduleFactory
    {
        public static ActivitySchedule Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivitySchedule()
            {
                Name = "Session 4 Schedule",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(35),
                DailyStartTime = DateTime.UtcNow,
                DailyEndTime = DateTime.UtcNow.AddHours(2),
                AccountId =account.ID 
                
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        
        public static ActivitySchedule CreateIsClosing(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivitySchedule()
            {
                Name = "Session 4 Schedule",
                EndDate = DateTime.UtcNow,
                DailyStartTime = DateTime.UtcNow,
                DailyEndTime = DateTime.UtcNow.AddHours(2),
                AccountId = account.ID               
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static ActivitySchedule Create(Account account, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc)
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

        public static ActivitySchedule Create(Account account, ActivityGroup activityGroup, DateTime startDate, SystemMessages msgs, DatabaseConnection dc)
        {
            var activitySchedule = new ActivitySchedule()
            {
                Name = "Test Activity Schedule",
                AccountId = account.ID.Value,
                ActivityGroupId = activityGroup?.ID.Value,
                ScheduleDetails = "Test Schedule Details",
                RepeatsWeekly = false,
                IsOngoing = true,
                StartDate = startDate,
                EndDate = DateTime.UtcNow.AddMonths(2),
                DailyStartTime = DateTime.UtcNow,
                DailyEndTime = DateTime.UtcNow.AddHours(2)
            };

            msgs.AddRange(activitySchedule.Save(dc));
            return activitySchedule;

        }

        public static void Create(Account account, Activity activity, ActivitySchedule schedule, List<DateRange> startDates, List<DateTime> endDates, WebUser user, SystemMessages msgs, DatabaseConnection dc)
        {
            ActivityScheduleService.SetupSchedule(
                activity.AccountId.Value,
                schedule.ID.Value,
                $"{activity.Name} - Schedule",
                schedule.ScheduleDetails,
                schedule.RepeatsWeekly.ToString(),
                schedule.IsOngoing.ToString(),
                schedule.StartDate.ToString(),
                schedule.EndDate.ToString(),
                schedule.DailyStartTime.ToString(),
                schedule.DailyEndTime.ToString(),
                startDates,
                endDates,
                account,
                activity.ActivityGroupId,
                user.ID.Value,
                msgs,
                dc);
        }
    }
}
