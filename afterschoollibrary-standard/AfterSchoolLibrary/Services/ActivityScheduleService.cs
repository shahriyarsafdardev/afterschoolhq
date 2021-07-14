using System;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityScheduleService
    {
        public static ActivitySchedule SetupSchedule(int accountId, int scheduleId, string scheduleName, string details, string repeatsweekly, string isongoing, string startdate, string enddate, string dailystarttime, string dailyendtime, List<DateRange> scheduledates, List<DateTime> scheduledatesclosed, Account account, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
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
                    isongoing.ToBoolean(),
                    startdate.ToDateTime(),
                    enddate.ToDateTime(),
                    dailystarttime.ToDateTime(),
                    dailyendtime.ToDateTime(),
                    scheduledates,
                    scheduledatesclosed,
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
                //if (account.HasFeature(FeatureType.ManageActivityGroups, dc: dc) == false)
                //{
                    msgs.AddRange(schedule.UpdateScheduleAndInstances(
                        schedule.Name,
                        schedule.ScheduleDetails,
                        repeatsweekly.ToBoolean(),
                        isongoing.ToBoolean(),
                        startdate.ToDateTime(),
                        enddate.ToDateTime(),
                        dailystarttime.ToDateTime(),
                        dailyendtime.ToDateTime(),
                        scheduledates,
                        scheduledatesclosed,
                        loggedInUserId,
                        dc));
                //}
            }

            return schedule;
        }

        public static ActivitySchedule CreateNew(string scheduleName, string details, bool repeatsWeekly, bool isongoing, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, List<DateRange> scheduleDates, List<DateTime> scheduleDatesClosed, int accountId, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            ActivitySchedule schedule = null;            

            Database.ExecuteTransaction(
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   schedule = new ActivitySchedule
                   {
                       Name = scheduleName,
                       ScheduleDetails = details.IfBlank("").MaxChars(4000),
                       AccountId = accountId,
                       RepeatsWeekly = repeatsWeekly,
                       IsOngoing = isongoing,
                       StartDate = startDate,
                       EndDate = endDate,
                       DailyStartTime = dailyStartTime,
                       DailyEndTime = dailyEndTime,
                       CreateDate = DateTime.UtcNow,
                       LastModifiedDate = DateTime.UtcNow,
                       CreatedBy = loggedInUserId,
                       LastModifiedBy = loggedInUserId,
                       ActivityGroupId = activityGroupId
                   };
                   amsgs.AddRange(schedule.Save(adc));

                   amsgs.AddRange(schedule.UpdateInstances(scheduleDates, scheduleDatesClosed, loggedInUserId, adc));                   
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

        public static bool IsEndingInTwoDays(this ActivitySchedule schedule, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (schedule.IsNotEmpty())
            {
                var now = (ownerAccount ?? schedule.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
                return now.AddDays(2).IsOnOrAfter(schedule.EndDate.Value);
            }

            return false;
        }
    }
}
