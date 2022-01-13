using System.Linq;
using System.Collections;
using BasicLibrary;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Models
{
    public class UILabel
    {
        public static string ActivityLabel(ActivityGroupType activityGroupType, bool plural = false)
        {
            if (activityGroupType == ActivityGroupType.League)
                return ActivityLabel(ActivityType.Team, plural);

            return ActivityLabel(ActivityType.Activity, plural);
        }
        public static string ActivityLabel(ActivityType activityType = ActivityType.Activity, bool plural = false)
        {
            switch (activityType)
            {
                case ActivityType.Team:
                    return GetLabelConfigParam("Team", plural);
                case ActivityType.FieldTrip:
                    return GetLabelConfigParam("FieldTrip", plural);
                case ActivityType.PermissionSlip:
                    return GetLabelConfigParam("PermissionSlip", plural);
                case ActivityType.LearningPod:
                    return GetLabelConfigParam("LearningPod", plural);
                default:
                    return GetLabelConfigParam("Activity", plural);
            }
        }
        public static string RegistrantLabel(bool plural = false)
        {
            return GetLabelConfigParam("Registrant", plural);
        }
        public static string ActivityGroupLabel(ActivityGroupType groupType, bool plural = false)
        {
            if (groupType == ActivityGroupType.League)
                return GetLabelConfigParam("League", plural);

            return GetLabelConfigParam("ActivityGroup", plural);
        }
        public static string RegistrationPeriodLabel(bool plural = false)
        {
            return GetLabelConfigParam("RegistrationPeriod", plural);
        }
        public static string RegistrationPeriodAbbrLabel(bool plural = false)
        {
            return GetLabelConfigParam("RegistrationPeriodAbbr", plural);
        }
        public static string ProductName()
        {
            if (Utilities.IsAfterSchool())
                return ProductNameAfterSchoolHQ();

            return ProductNameEnrollHQ();
        }
        public static string ProductNameEnrollHQ()
        {
            return GetLabelConfigParam("ProductNameEnrollHQ");
        }
        public static string ProductNameAfterSchoolHQ()
        {
            return GetLabelConfigParam("ProductNameAfterSchoolHQ");
        }
        public static string ProductLogo()
        {
            if (Utilities.IsAfterSchool())
                return ProductLogoAfterSchoolHQ();

            return ProductLogoEnrollHQ();
        }
        public static string ProductLogoEnrollHQ()
        {
            return GetLabelConfigParam("ProductLogoEnrollHQ");
        }
        public static string ProductLogoAfterSchoolHQ()
        {
            return GetLabelConfigParam("ProductLogoAfterSchoolHQ");
        }
        public static string ProductLogoInverse()
        {
            if (Utilities.IsAfterSchool())
                return ProductLogoInverseAfterSchoolHQ();

            return ProductLogoInverseEnrollHQ();
        }
        public static string ProductLogoInverseEnrollHQ()
        {
            return GetLabelConfigParam("ProductLogoInverseEnrollHQ");
        }
        public static string ProductLogoInverseAfterSchoolHQ()
        {
            return GetLabelConfigParam("ProductLogoInverseAfterSchoolHQ");
        }

        private static string GetLabelConfigParam(string name, bool plural = false)
        {
            string key = name;
            if (plural) key = name + "_plural";
            return ((Hashtable)System.Configuration.ConfigurationManager.GetSection("applabels"))[key].ToString();
        }

        public static string GetGradeDescription(string gradeText)
        {
            if (gradeText.IsEmpty()) return gradeText;
            var gradeValue = AfterSchoolLibrary.Helpers.Utilities.IsAlreadyJsonString(gradeText)
                ? AfterSchoolLibrary.Helpers.Utilities.GetKeyValuePairs(gradeText).FirstOrDefault()?.value
                : gradeText;
            if (gradeValue == "K")
                return "Kindergarten";
            if (gradeValue == "Pre-K")
                return "Pre-K";
            return gradeValue + " Grade";

        }

        public static string GetFormattedAnswer(string text)
        {
            if (text.IsNotEmpty() && AfterSchoolLibrary.Helpers.Utilities.IsAlreadyJsonString(text))
                return AfterSchoolLibrary.Helpers.Utilities.GetKeyValuePairs(text).FirstOrDefault()?.value;

            return text;
        }

        public static string GetFormattedAnswerForArray(string text)
        {
            if (text.IsNotEmpty() && AfterSchoolLibrary.Helpers.Utilities.IsAlreadyJsonString(text))
              return AfterSchoolLibrary.Helpers.Utilities.GetKeyValuePairs(text)?.Select(m => m.value)?.ToStringList(separator: ", ");

            return text;
        }

        public static bool CheckAllowTextMessage(WebUser currentUser)
        {
            return !currentUser.IsNotEmpty() || currentUser.AllowTextMessages && System.Web.HttpContext.Current.Request.HttpMethod.ToString() == "GET";
        public static bool IsCurrentUserCleverIntegrated()
        {
            if (SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentUser.CleverUserId.IsNotEmpty())
                return true;

            return false;
        }

        public static string GetCleverUserNameForCurrentUser()
        {
            if (SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentUser.CleverUserName.IsNotEmpty())
                return SessionVariables.CurrentUser.CleverUserName;

            return null;
        }
    }
}