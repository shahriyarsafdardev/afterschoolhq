using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;

namespace AfterSchoolLibrary
{
    public static class Extensions
    {
        public static Lob.Models.AccountType ToLobAccountType(this LegalEntityType legalEntityType)
        {
            if (legalEntityType == LegalEntityType.Company)
                return Lob.Models.AccountType.Company;

            return Lob.Models.AccountType.Individual;
        }

        public static string GetDescription(this StripeVerificationFailedCode stripeVerificationFailedCode)
        {
            switch(stripeVerificationFailedCode)
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
            return (DayOfWeek)((int)day - 1);
        }
        public static DayOfWeekASL ToDayOfWeekASL(this DayOfWeek day)
        {
            return (DayOfWeekASL)((int)day + 1);
        }
        public static IEnumerable<System.Data.DataRow> AsEnumerable(this System.Data.DataTable dt)
        {
            var rows = new List<System.Data.DataRow>();
            if (dt != null)
            {
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    rows.Add(dr);
                }
            }
            return rows.AsEnumerable();
        }

        public static string GetMaxWords(this string aString, int maxChars)
        {
            var words = aString.Split(' ');
            var finalSlug = "";

            foreach (var word in words)
            {
                if (word.Length + finalSlug.Length <= maxChars)
                    finalSlug += word;
                else
                    break;
            }

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

        public static TValue GetValueOrDefault<TKey,TValue> (this IDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.TryGetValue(key, out var ret) ? ret : default;
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

        enum Phase { Years, Months, Days, Done }

        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            DateTime current = date1;
            int years = 0;
            int months = 0;
            int days = 0;
            int totalDays = (date2.Date - date1.Date).Days;

            Phase phase = Phase.Years;
            DateTimeSpan span = new DateTimeSpan();
            int officialDay = current.Day;

            while (phase != Phase.Done)
            {
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
            }

            return span;
        }
    }

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Use to get the DateTime for the start of the week.
        /// DateTime dt = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
        /// DateTime dt = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 - (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(1 * diff).Date;
        }
    }
}
