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

            return finalSlug;
        }

        public static TValue GetValueOrDefault<TKey,TValue> (this IDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.TryGetValue(key, out var ret) ? ret : default;
    }
}
