using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BasicLibrary;

namespace AfterSchoolLibrary
{
    public static class Extensions
    {
        public static string EncodeBase64(this string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        public static string DecodeBase64(this string value)
        {
            var valueBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public static Lob.Models.AccountType ToLobAccountType(this LegalEntityType legalEntityType)
        {
            if (legalEntityType == LegalEntityType.Company)
                return Lob.Models.AccountType.Company;

            return Lob.Models.AccountType.Individual;
        }

        public static string GetDescription(this StripeVerificationFailedCode stripeVerificationFailedCode)
        {
            switch (stripeVerificationFailedCode)
            {
                case StripeVerificationFailedCode.FailedKeyedIdentity:
                    return "the supplied identity information could not be verified";
                case StripeVerificationFailedCode.FailedOther:
                    return "verification failed";
                case StripeVerificationFailedCode.ScanCorrupt:
                    return "the supplied ID image is corrupt";
                case StripeVerificationFailedCode.ScanFailedGrayscale:
                    return "the supplied ID image is not in color";
                case StripeVerificationFailedCode.ScanFailedOther:
                    return "the scan failed for an unknown reason, please retry";
                case StripeVerificationFailedCode.ScanIDCountryNotSupported:
                    return "the supplied ID type is not supported (e.g., is not government-issued";
                case StripeVerificationFailedCode.ScanNameMismatch:
                    return "the name on the ID does not match the name on the account";
                case StripeVerificationFailedCode.ScanNotReadable:
                    return "the supplied ID image is not readable (e.g., too blurry or dark)";
                case StripeVerificationFailedCode.ScanNotUploaded:
                    return "no ID scan was uploaded";
                case StripeVerificationFailedCode.ScanFailedTestMode:
                    return "verification failed - test mode";
            }

            return "unknown";
        }

        public static DayOfWeek ToDayOfWeek(this DayOfWeekASL day)
        {
            return (DayOfWeek) ((int) day - 1);
        }

        public static DayOfWeekASL ToDayOfWeekASL(this DayOfWeek day)
        {
            return (DayOfWeekASL) ((int) day + 1);
        }

        public static IEnumerable<DataRow> AsEnumerable(this DataTable dt)
        {
            var rows = new List<DataRow>();
            if (dt != null)
                foreach (DataRow dr in dt.Rows)
                    rows.Add(dr);
            return rows.AsEnumerable();
        }

        public static string GetMaxWords(this string aString, int maxChars)
        {
            var words = aString.Split(' ');
            var finalSlug = "";

            foreach (var word in words)
                if (word.Length + finalSlug.Length <= maxChars)
                    finalSlug += word;
                else
                    break;

            if (finalSlug.IsEmpty())
                finalSlug = aString.MaxChars(maxChars);

            return finalSlug;
        }

        public static AHQObjectType ToAHQObjectType(this ParticipationObjectType participationObjectType)
        {
            var objectType = AHQObjectType.NotSet;

            switch (participationObjectType)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    objectType = AHQObjectType.AccountActivityGroup;
                    break;
                case ParticipationObjectType.Activity:
                    objectType = AHQObjectType.Activity;
                    break;
                case ParticipationObjectType.ActivityGroup:
                    objectType = AHQObjectType.ActivityGroup;
                    break;
            }

            return objectType;
        }

        public static ObjectType ToObjectType(this ParticipationObjectType participationObjectType)
        {
            var objectType = ObjectType.NotSet;

            switch (participationObjectType)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    objectType = ObjectType.AccountActivityGroup;
                    break;
                case ParticipationObjectType.Activity:
                    objectType = ObjectType.Activity;
                    break;
                case ParticipationObjectType.ActivityGroup:
                    objectType = ObjectType.ActivityGroup;
                    break;
            }

            return objectType;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var ret) ? ret : default;
        }

        public static void ValidateStartAndEndDate(string startdate, string enddate, SystemMessages msgs)
        {
            if (startdate.IsNotEmpty() && enddate.IsNotEmpty())
            {
                var startDate = startdate.ToDateTime();
                var endDate = enddate.ToDateTime();
                if (endDate < startDate) msgs.Add("Please ensure start date is earlier than the corresponding end date", MessageType.UserError);
            }
        }

        public static void ValidateStartAndEndDateWithTime(string startdate, string starttime, string enddate, string endtime, SystemMessages msgs)
        {
            if (startdate.IsNotEmpty() && enddate.IsNotEmpty())
            {
                var startDate = startdate.ToDateTime();
                var endDate = enddate.ToDateTime();

                var exactStartTime = starttime.ToDateTime().TimeOfDay;
                var exactEndTime = endtime.ToDateTime().TimeOfDay;

                if (endDate.Add(exactEndTime) < startDate.Add(exactStartTime))
                    msgs.Add("Please ensure start date, start time is earlier than the corresponding end date, end time", MessageType.UserError);
            }
        }
    }

    public struct DateTimeSpan
    {
        public int Years { get; }
        public int Months { get; }
        public int Days { get; }
        public int Hours { get; }
        public int Minutes { get; }
        public int Seconds { get; }
        public int Milliseconds { get; }
        public int TotalDays { get; }

        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds, int totalDays)
        {
            Years = years;
            Months = months;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
            TotalDays = totalDays;
        }

        private enum Phase
        {
            Years,
            Months,
            Days,
            Done,
        }

        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            var current = date1;
            var years = 0;
            var months = 0;
            var days = 0;
            var totalDays = (date2.Date - date1.Date).Days;

            var phase = Phase.Years;
            var span = new DateTimeSpan();
            var officialDay = current.Day;

            while (phase != Phase.Done)
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                        {
                            years++;
                        }

                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                            if (current.Day < officialDay && officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                                current = current.AddDays(officialDay - current.Day);
                        }
                        else
                        {
                            months++;
                        }

                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds, totalDays);
                            phase = Phase.Done;
                        }
                        else
                        {
                            days++;
                        }

                        break;
                }

            return span;
        }
    }

    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Use to get the DateTime for the start of the week.
        ///     DateTime dt = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
        ///     DateTime dt = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = (7 - (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(1 * diff).Date;
        }
    }

    public static class StringExtensions
    {
        public static bool IsMatch(this string s, string regex)
        {
            return Regex.IsMatch(s, regex);
        }

        public static string Truncate(this string s, int length)
        {
            return s.Length <= length ? s : s.Substring(0, length - 1) + "…";
        }
    }
}