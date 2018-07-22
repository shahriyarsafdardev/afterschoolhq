using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    public class ActivityScheduleUI
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

        public static List<DateRange> PrepScheduleDates(string repeatsweekly, List<DayOfWeekASL> days, string startdate, string enddate, string dailystarttime, string dailyendtime, NameValueCollection meetingDates)
        {
            var scheduleDates = new List<DateRange>();

            if (repeatsweekly.IsNotEmpty())
            {
                foreach (var day in days)
                {
                    var currentDate = startdate.ToDateTime().AddWeeks(0).NextDayOfWeek(day.ToDayOfWeek());
                    while (currentDate <= enddate.ToDateTime())
                    {
                        var meetingStartDate = String.Concat(currentDate.ToShortDateString(), " ", dailystarttime).ToDateTime();
                        var meetingEndDate = String.Concat(currentDate.ToShortDateString(), " ", dailyendtime).ToDateTime();

                        scheduleDates.Add(new DateRange(meetingStartDate, meetingEndDate));

                        currentDate = currentDate.AddWeeks(1);
                    }
                }
            }
            else
            {
                for (var i = 1; i <= 50; i++)
                {
                    var meetingStartDate = meetingDates["meeting-date-" + i];
                    var meetingEndDate = meetingDates["meeting-date-" + i];
                    var meetingStartTime = meetingDates["meeting-starttime-" + i];
                    var meetingEndTime = meetingDates["meeting-endtime-" + i];
                    var meetingInUse = meetingDates["meeting-inuse-" + i];

                    if (meetingInUse.IsBoolean() && meetingInUse.ToBoolean() && meetingStartDate.IsDateTime())
                    {
                        var meetingStart = $"{meetingStartDate} {meetingStartTime}".ToDateTime();
                        var meetingEnd = $"{meetingEndDate} {meetingEndTime}".ToDateTime();

                        var dateAlreadyExists = scheduleDates.Where(dateRange => 
                                dateRange.StartDate == meetingStart && 
                                dateRange.EndDate == meetingEnd
                            )
                            .Any();

                        if (!dateAlreadyExists)
                        {
                            scheduleDates.Add(new DateRange()
                            {
                                StartDate = $"{meetingStart.ToShortDateString()} {dailystarttime}".ToDateTime(),
                                EndDate = $"{meetingEnd.ToShortDateString()} {dailyendtime}".ToDateTime(),
                            });
                        }
                    }
                }
            }

            return scheduleDates;
        }
    }
}