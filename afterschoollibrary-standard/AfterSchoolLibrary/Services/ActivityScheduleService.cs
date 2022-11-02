using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityScheduleService
    {
        public static ActivitySchedule SetupSchedule(int accountId, int scheduleId, string scheduleName, string details,
            string repeatsweekly, string isongoing, string startdate, string enddate, string dailystarttime,
            string dailyendtime, List<ActivityInstance> scheduledates, List<DateTime> scheduledatesclosed,
            string daysOfTheWeek, Account account, int? activityGroupId, int loggedInUserId, SystemMessages msgs,
            DatabaseConnection dc = null)
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
                    daysOfTheWeek,
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
                    daysOfTheWeek,
                    loggedInUserId,
                    dc));
                //}
            }

            return schedule;
        }

        public static ActivitySchedule CreateNew(string scheduleName, string details, bool repeatsWeekly,
            bool isongoing, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime,
            List<ActivityInstance> scheduleDates, List<DateTime> scheduleDatesClosed, string daysOfTheWeek,
            int accountId, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Extensions.ValidateStartAndEndDateWithTime(startDate.ToShortDateString(),
                dailyStartTime.ToShortTimeString(), endDate.ToShortDateString(), dailyEndTime.ToShortTimeString(),
                msgs);

            if (msgs.HasErrors()) return null;

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
                        DaysOfTheWeek = daysOfTheWeek,
                        CreateDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow,
                        CreatedBy = loggedInUserId,
                        LastModifiedBy = loggedInUserId,
                        ActivityGroupId = activityGroupId,
                    };
                    amsgs.AddRange(schedule.Save(adc));

                    amsgs.AddRange(
                        schedule.UpdateInstances(scheduleDates, scheduleDatesClosed, loggedInUserId, adc));
                },
                onSuccess: (SystemMessages successMsgs) => { msgs.AddRange(successMsgs); },
                onFailure: (SystemMessages failureMsgs) => { msgs.AddRange(failureMsgs); },
                dc: dc
            );
            return schedule;
        }

        public static bool IsEndingInTwoDays(this ActivitySchedule schedule, Account ownerAccount = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (schedule.IsNotEmpty())
            {
                var now = (ownerAccount ?? schedule.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
                return now.AddDays(2).IsOnOrAfter(schedule.EndDate.Value);
            }

            return false;
        }

        public static List<DateTime> GetNextInstances(this ActivitySchedule schedule, int numberOfInstances,
            ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (participationObject.IsActivity())
                return schedule.GetNextInstances(numberOfInstances, participationObject.GetActivity(), msgs, dc);

            return new List<DateTime>();
        }

        public static List<DateTime> GetNextInstances(this ActivitySchedule schedule, int numberOfInstances,
            Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dates = new List<DateTime>();
            var outSessionDates = new List<DateTime>();
            var account = schedule.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);

            if (schedule.IsOngoing && schedule.GetDaysOfTheWeek(msgs, dc).Any())
            {
                var referenceDate = now;
                var outOfSession = schedule.GetOutSessionInstances(msgs, dc)
                    .Where(instance => instance.StartDateTime.Value.IsAfter(referenceDate));
                var cutoffHours = 0;

                if (activity.RegistrationCutoffHours.HasValue)
                    cutoffHours = activity.RegistrationCutoffHours.Value;

                if (schedule.StartDate.HasValue && schedule.StartDate.Value.IsAfter(referenceDate))
                    referenceDate = schedule.StartDate.Value;

                var daysOfTheWeek = schedule.GetDaysOfTheWeek(msgs, dc).Select(x => x.ToDayOfWeek()).ToList();
                var currentDayOfTheWeek = referenceDate.DayOfWeek;
                var nextDay = DayOfWeek.Sunday;

                foreach (var day in daysOfTheWeek)
                {
                    if (day >= currentDayOfTheWeek)
                    {
                        nextDay = day;
                        break;
                    }
                }

                for (var i = 0; i < numberOfInstances; i++)
                {
                    var nextDayAsInt = (int) nextDay;

                    if (nextDayAsInt < (int) currentDayOfTheWeek)
                        nextDayAsInt += 7; // add a week

                    var daysToAdd = nextDayAsInt - (int) currentDayOfTheWeek;

                    var dateToAdd = referenceDate.AddDays(daysToAdd);

                    if (!dateToAdd.Date.In(outOfSession.Select(x => x.StartDateTime.Value.Date)) &&
                        dateToAdd.AddHours(cutoffHours * -1).IsAfter(now))
                        dates.Add($"{dateToAdd.ToShortDateString()} {schedule.DailyStartTime.ToShortTimeString()}"
                            .ToDateTime());
                    else
                        i--; // don't increment

                    if (daysOfTheWeek.IndexOf(nextDay) < daysOfTheWeek.Count - 1)
                        nextDay = daysOfTheWeek[daysOfTheWeek.IndexOf(nextDay) + 1];
                    else
                        nextDay = daysOfTheWeek[0];

                    referenceDate = dateToAdd.AddDays(1);
                    currentDayOfTheWeek = referenceDate.DayOfWeek;
                }
            }
            else
            {
                outSessionDates.AddRange(schedule.GetOutSessionInstances(msgs, dc).Select(x => x.StartDateTime.Value));
                dates.AddRange(schedule.GetInSessionInstances(msgs, dc)
                    .Where(x => x.StartDateTime.Value.Date.IsOnOrAfter(now.Date) &&  x.StartDateTime.Value.Date.NotIn(outSessionDates))
                    .Take(numberOfInstances)
                    .Select(x => x.StartDateTime.Value));
            }
            return dates;
        }

        public static ActivityInstance AddInstance(this ActivitySchedule schedule, DateTime startDate,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var instance = new ActivityInstance()
            {
                ActivityScheduleId = schedule.ID,
                StartDateTime = $"{startDate.Date.ToShortDateString()} {schedule.DailyStartTime.ToShortTimeString()}"
                    .ToDateTime(),
                EndDateTime = $"{startDate.Date.ToShortDateString()} {schedule.DailyEndTime.ToShortTimeString()}"
                    .ToDateTime(),
                InstanceTypeId = InstanceType.InSession,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser.ID.IfEmpty(0),
                Day = startDate.DayOfWeek.ToDayOfWeekASL(),
            };

            msgs.AddRange(instance.Save(dc));

            return instance;
        }

        public static List<ActivityInstance> RemoveOutOfSessionInstances(this List<ActivityInstance> instances,
            ActivitySchedule schedule, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var outOfSessionInstances = schedule?.GetOutSessionInstances(msgs, dc);

            if (outOfSessionInstances?.Any() == true)
            {
                var overlappingInstances = instances.Where(m =>
                    outOfSessionInstances.Any(t =>
                        t.Day == m.Day &&
                        t.StartDateTime.ToShortDateString() == m.StartDateTime.ToShortDateString() &&
                        t.EndDateTime.ToShortDateString() == m.EndDateTime.ToShortDateString()
                    )
                );

                if (overlappingInstances?.Any() == true)
                    instances.RemoveAll(m => overlappingInstances.Any(t => t.ID == m.ID));
            }

            return instances;
        }
    }
}