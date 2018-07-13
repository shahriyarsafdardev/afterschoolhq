using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityScheduleService
    {
        public static ActivitySchedule SetupSchedule(int accountId, int scheduleId, string scheduleName, string details, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string scheduledates, Account account, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivitySchedule schedule = null;

            // create new schedule
            if (scheduleId == 0)
            {
                // string.Format("{0} - Schedule", activity.Name)

                scheduleName = ActivitySchedule.GenerateUniqueName(accountId, scheduleName, dc: dc);

                schedule = CreateNew(
                    scheduleName,
                    details,
                    repeatsweekly.ToBoolean(),
                    startdate.ToDateTime(),
                    enddate.ToDateTime(),
                    dailystarttime.ToDateTime(),
                    dailyendtime.ToDateTime(),
                    scheduledates,
                    account.ID.Value,
                    activityGroupId,
                    loggedInUserId,
                    msgs,
                    dc);
            }
            else
            {
                schedule = ActivitySchedule.Load(scheduleId, msgs, dc);

                // if user doesn't have feature then we're updaing in the background
                // otherwise user may have multiple activities with the same schedule so we cannot update directly on the activity
                if (account.HasFeature(FeatureType.ManageActivityGroups, dc: dc) == false)
                {
                    msgs.AddRange(schedule.UpdateScheduleAndInstances(
                        repeatsweekly.ToBoolean(),
                        startdate.ToDateTime(),
                        enddate.ToDateTime(),
                        dailystarttime.ToDateTime(),
                        dailyendtime.ToDateTime(),
                        scheduledates,
                        loggedInUserId,
                        dc));
                }
            }

            return schedule;
        }


        public static ActivitySchedule CreateNew(string scheduleName, string details, bool repeatsWeekly, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, string scheduleDates, int accountId, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var schedule = ActivitySchedule.CreateNew(
                scheduleName, 
                details, 
                repeatsWeekly, 
                startDate, 
                endDate, 
                dailyStartTime, 
                dailyEndTime, 
                scheduleDates, 
                accountId, 
                loggedInUserId, 
                activityGroupId, 
                msgs, 
                dc);            

            return schedule;
        }
    }
}
