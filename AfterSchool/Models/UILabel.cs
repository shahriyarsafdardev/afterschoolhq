using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    public class UILabel
    {
        public static string ActivityLabel(ActivityGroupType activityGroupType, bool plural = false)
        {
            if (activityGroupType == ActivityGroupType.League)
                return ActivityLabel(ActivityType.Team, plural);

            return ActivityLabel(ActivityType.Activity);
        }
        public static string ActivityLabel(ActivityType activityType, bool plural = false)
        {
            if (activityType == ActivityType.Team)
                return GetLabelConfigParam("Team", plural);

            return GetLabelConfigParam("Activity", plural);
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
    }
}