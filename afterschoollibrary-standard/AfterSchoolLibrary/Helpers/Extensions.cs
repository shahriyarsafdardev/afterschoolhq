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
                rows.AddRange(dt.Rows.Cast<DataRow>());
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
            var objectType = participationObjectType switch
            {
                ParticipationObjectType.AccountActivityGroup => AHQObjectType.AccountActivityGroup,
                ParticipationObjectType.Activity => AHQObjectType.Activity,
                ParticipationObjectType.ActivityGroup => AHQObjectType.ActivityGroup,
                _ => AHQObjectType.NotSet,
            };

            return objectType;
        }

        public static ObjectType ToObjectType(this ParticipationObjectType participationObjectType)
        {
            var objectType = participationObjectType switch
            {
                ParticipationObjectType.AccountActivityGroup => ObjectType.AccountActivityGroup,
                ParticipationObjectType.Activity => ObjectType.Activity,
                ParticipationObjectType.ActivityGroup => ObjectType.ActivityGroup,
                _ => ObjectType.NotSet,
            };

            return objectType;
        }

        public static bool IsJson(this string text)
        {
            return Helpers.Utilities.IsAlreadyJsonString(text);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var ret) ? ret : default;
        }

        public static void ValidateStartAndEndDate(string startDateText, string endDateText, SystemMessages msgs)
        {
            if (startDateText.IsEmpty() || endDateText.IsEmpty())
                return;

            var startDate = startDateText.ToDateTime();
            var endDate = endDateText.ToDateTime();
            if (endDate < startDate)
                msgs.Add("Please ensure start date is earlier than the corresponding end date", MessageType.UserError);
        }

        public static void ValidateStartAndEndDateWithTime(string startDateText, string startTimeText, string endDateText, string endTimeText,
            SystemMessages msgs)
        {
            if (startDateText.IsEmpty() || endDateText.IsEmpty())
                return;

            var startDate = startDateText.ToDateTime();
            var startTime = startTimeText.ToDateTime().TimeOfDay;
            var startDateTime = startDate.Add(startTime);

            var endDate = endDateText.ToDateTime();
            var endTime = endTimeText.ToDateTime().TimeOfDay;
            var endDateTime = endDate.Add(endTime);

            if (endDateTime < startDateTime)
                msgs.Add("Please ensure start date, start time is earlier than the corresponding end date, end time", MessageType.UserError);
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