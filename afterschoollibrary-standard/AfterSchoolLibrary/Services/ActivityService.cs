using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary
{
    public static class ActivityService
    {
        public static Location GetLocation(this Activity activity, DatabaseConnection dc)
        {
            return Location.Load(activity.LocationId.Value, dc);
        }

        public static string GenerateUniqueName(string name, int? activityId, int? accountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            string origUniqueName = reg.Replace(name, "").MaxChars(25);
            string uniqueName = origUniqueName;
            var duplicate = Activity.LoadByUniqueNameByAccountId(uniqueName, accountId.Value, dc);
            int count = 1;
            while (duplicate?.ID != activityId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = Activity.LoadByUniqueNameByAccountId(uniqueName, accountId.Value, dc);
            }
            return uniqueName;
        }

        public static int GetSeatsTaken(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Reserved, dc);
        }

        public static int GetWaitingListSeatsTaken(this Activity activity, DatabaseConnection dc = null)
        {
            return activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.WaitingList, dc);
        }

        public static int GetSeatsRemaining(this Activity activity, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.SeatsAvailable.HasValue)
                rtn = activity.SeatsAvailable.Value - activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Reserved, dc);

            return Math.Max(0, rtn);
        }

        public static int GetWaitingListSeatsRemaining(this Activity activity, DatabaseConnection dc = null)
        {
            var rtn = 0;
            if (activity.WaitingListSeats.HasValue)
                rtn = activity.WaitingListSeats.Value - activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.WaitingList, dc);

            return Math.Max(0, rtn);
        }
    }
}
