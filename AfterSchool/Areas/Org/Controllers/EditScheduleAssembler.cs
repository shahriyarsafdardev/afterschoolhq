using System;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class EditScheduleResult
    {
        public Account Account { get; set; }
        public SystemMessages Messages { get; set; }
        public Activity Activity { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFullScreen { get; set; }
        public bool IsActivityLeader { get; set; }
        public string FullScreenExitUrl { get; set; }
        public List<ActivitySchedule> Schedules { get; set; }
        public string ActiveMenu { get; set; }
        public ActivitySchedule Schedule { get; set; }
    }

    public class EditScheduleAssembler
    {
        public EditScheduleResult AssembleEditScheduleResult(ActivitySchedule schedule, Activity activity,
            Account currentAccount, UserType accountUserType, RegistrationPeriod activeRegistrationPeriod, bool currentUserIsActivityLeader,
            List<ActivitySchedule> schedules, bool isNew, string fullScreenUrl, SystemMessages msgs)
        {
            if (schedule.IsEmpty())
                schedule = activeRegistrationPeriod.IsEmpty()
                    ? ActivitySchedule.GetNewDefaultSchedule()
                    : CreateDefaultScheduleForRegistrationPeriod(activity, activeRegistrationPeriod);

            var editScheduleResult = new EditScheduleResult
            {
                Account = currentAccount,
                IsActivityLeader = ActivityController.IsActivityLeader(activity, currentAccount, accountUserType, currentUserIsActivityLeader),
                ActiveMenu = "EditSchedule",
                Schedules = schedules,
                IsFullScreen = ActivityController.GetIsFullScreen(activity, isNew),
                FullScreenExitUrl = fullScreenUrl,
                Schedule = schedule,
                Messages = msgs,
                Activity = activity,
            };

            return editScheduleResult;
        }

        internal ActivitySchedule CreateDefaultScheduleForRegistrationPeriod(Activity activity, RegistrationPeriod period)
        {
            var startDate = period.EndDateTime.Value.Date.FirstDateOfWeek().AddWeeks(1);
            var endDate = period.EndDateTime.Value.Date.FirstDateOfWeek().AddWeeks(9);

            if (activity.IsContinuousEnrollment)
                startDate = period.StartDateTime.Value.FirstDateOfWeek().AddWeeks(1);

            if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerClass)
            {
                startDate = DateTime.UtcNow.Date.AddDays(1);
                endDate = startDate.AddWeeks(9);
            }

            return new ActivitySchedule
            {
                StartDate = startDate,
                EndDate = endDate,
                DailyStartTime = DateTime.UtcNow.Date.AddHours(15),
                DailyEndTime = DateTime.UtcNow.Date.AddHours(17),
                RepeatsWeekly = true,
            };
        }
    }
}