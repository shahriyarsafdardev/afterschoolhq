using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
