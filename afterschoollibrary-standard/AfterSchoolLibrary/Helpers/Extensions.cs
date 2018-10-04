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

        public static TValue GetValueOrDefault<TKey,TValue> (this IDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.TryGetValue(key, out var ret) ? ret : default;
    }
}
