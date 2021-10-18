using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Models
{
    public static class ActivityScheduleUI
    {
        public static List<DayOfWeekASL> PrepDaysOfWeek(NameValueCollection daylist)
        {
            var days = new List<DayOfWeekASL>();
            foreach (DayOfWeekASL day in System.Enum.GetValues(typeof(DayOfWeekASL)))
            {
                if (daylist["schedulerepeat-" + ((int)day)].IsNotEmpty())
                {
                    days.Add(day);
                }
            }
            return days;
        }

        public static List<DateTime> PrepScheduleDatesClosed(NameValueCollection closedMeetingDates)
        {
            var scheduleDates = new List<DateTime>();

            for (var i = 1; i <= 30; i++)
            {
                var meetingStartDate = closedMeetingDates["meeting-closed-date-" + i];
                var meetingInUse = closedMeetingDates["meeting-closed-inuse-" + i];

                if (meetingInUse.IsBoolean() && meetingInUse.ToBoolean() && meetingStartDate.IsDateTime())
                {
                    var dateAlreadyExists = scheduleDates.Where(date => date == meetingStartDate.ToDateTime()).Any();

                    if (!dateAlreadyExists)
                    {
                        scheduleDates.Add(meetingStartDate.ToDateTime());
                    }
                }
            }

            return scheduleDates;
        }

        public static List<ActivityInstance> PrepScheduleDates(string repeatsweekly, List<DayOfWeekASL> days, string startdate, string enddate, string dailystarttime, string dailyendtime, NameValueCollection meetingDates)
        {
            var scheduleDates = new List<ActivityInstance>();

            if (repeatsweekly.IsNotEmpty())
            {
                foreach (var day in days)
                {
                    var currentDate = startdate.ToDateTime().AddWeeks(0).NextDayOfWeek(day.ToDayOfWeek());
                    while (currentDate <= enddate.ToDateTime())
                    {
                        var meetingStartDate = String.Concat(currentDate.ToShortDateString(), " ", dailystarttime).ToDateTime();
                        var meetingEndDate = String.Concat(currentDate.ToShortDateString(), " ", dailyendtime).ToDateTime();

                        scheduleDates.Add(new ActivityInstance(meetingStartDate, meetingEndDate));

                        currentDate = currentDate.AddWeeks(1);
                    }
                }
            }
            else
            {
                for (var i = 1; i <= 30; i++)
                {
                    var meetingStartDate = meetingDates["meeting-open-date-" + i];
                    var meetingEndDate = meetingDates["meeting-open-date-" + i];
                    var meetingStartTime = meetingDates["meeting-open-starttime-" + i];
                    var meetingEndTime = meetingDates["meeting-open-endtime-" + i];
                    var meetingInUse = meetingDates["meeting-open-inuse-" + i];
                    var meetingSeatsAvailable = meetingDates["meeting-open-seatsavailable-" + i].IsNumber() ?
                                                            meetingDates["meeting-open-seatsavailable-" + i].ToInteger() :
                                                            new int?();

                    if (meetingInUse.IsBoolean() && meetingInUse.ToBoolean() && meetingStartDate.IsDateTime())
                    {
                        var meetingStart = $"{meetingStartDate} {meetingStartTime}".ToDateTime();
                        var meetingEnd = $"{meetingEndDate} {meetingEndTime}".ToDateTime();

                        var dateAlreadyExists = scheduleDates.Any(instance =>
                                                    instance.StartDateTime == meetingStart &&
                                                    instance.EndDateTime == meetingEnd
                                                );

                        if (!dateAlreadyExists)
                        {
                            scheduleDates.Add(new ActivityInstance()
                            {
                                StartDateTime = meetingStart,
                                EndDateTime = meetingEnd,
                                SeatsAvailable = meetingSeatsAvailable,
                            });
                        }
                    }
                }
            }

            return scheduleDates;
        }
    }
}