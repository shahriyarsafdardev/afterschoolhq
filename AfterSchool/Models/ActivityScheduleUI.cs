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

        public static List<string> PrepScheduleDates(string repeatsweekly, List<DayOfWeekASL> days, string startdate, string enddate, string dailystarttime, string dailyendtime, NameValueCollection meetingDates)
        {
            var scheduleDates = new List<string>();

            if (repeatsweekly.IsNotEmpty())
            {
                foreach (var day in days)
                {
                    var currentDate = startdate.ToDateTime().AddWeeks(0).NextDayOfWeek(day.ToDayOfWeek());
                    while (currentDate <= enddate.ToDateTime())
                    {
                        var temp = String.Concat(currentDate.ToShortDateString(), " ", dailystarttime);
                        temp += "-";
                        temp += String.Concat(currentDate.ToShortDateString(), " ", dailyendtime);

                        scheduleDates.Add(temp);

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
                        var meetingStartDateTime = String.Concat(meetingStartDate, " ", meetingStartTime);
                        var meetingEndDateTime = String.Concat(meetingEndDate, " ", meetingEndTime);

                        var temp = meetingStartDateTime;
                        temp += "-";

                        temp += meetingEndDateTime;

                        if (!scheduleDates.Contains(temp))
                        {
                            scheduleDates.Add(temp);
                        }
                    }
                }
            }

            return scheduleDates;
        }
    }
}