using System;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityScheduleService
    {
        public static ActivitySchedule SetupSchedule(int accountId, int scheduleId, string scheduleName, string details, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, List<DateRange> scheduledates, Account account, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivitySchedule schedule = null;

            // create new schedule
            if (scheduleId == 0)
            {
                scheduleName = ActivitySchedule.GenerateUniqueName(accountId, scheduleName, msgs, dc);

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

        public static ActivitySchedule CreateNew(string scheduleName, string details, bool repeatsWeekly, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, List<DateRange> scheduleDates, int accountId, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            ActivitySchedule schedule = null;            

            Database.ExecuteTransaction(
               action: (SystemMessages actionMsgs, DatabaseConnection adc) =>
               {
                   schedule = new ActivitySchedule();
                   schedule.Name = scheduleName;
                   schedule.ScheduleDetails = details.IfBlank("").MaxChars(4000);
                   schedule.AccountId = accountId;
                   schedule.RepeatsWeekly = repeatsWeekly;
                   schedule.StartDate = startDate;
                   schedule.EndDate = endDate;
                   schedule.DailyStartTime = dailyStartTime;
                   schedule.DailyEndTime = dailyEndTime;
                   schedule.CreateDate = schedule.LastModifiedDate = DateTime.UtcNow;
                   schedule.CreatedBy = schedule.LastModifiedBy = loggedInUserId;
                   schedule.ActivityGroupId = activityGroupId;
                   actionMsgs.AddRange(schedule.Save(adc));

                   if ((scheduleDates.IsEmpty() || !scheduleDates.Any()) && schedule.RepeatsWeekly)
                   {
                       scheduleDates = ActivityInstance.CreateDateRangesFromSchedule(schedule, msgs, dc);
                   }

                   actionMsgs.AddRange(schedule.UpdateInstances(scheduleDates, loggedInUserId, adc));                   
               },
               onSuccess: (SystemMessages successMsgs) =>
               {
                   msgs.AddRange(successMsgs);
               },
               onFailure: (SystemMessages failureMsgs) =>
               {
                   msgs.AddRange(failureMsgs);
               },
               dc: dc
           );

            return schedule;
        }
    }
}
