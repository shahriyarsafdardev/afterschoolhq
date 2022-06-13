using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public static class EditHelpers
    {
        public const string EditSetup = "EditSetup";
        public const string EditRestrictions = "EditRestrictions";
        public const string EditSchedule = "EditSchedule";
        public const string EditDetails = "EditDetails";
        public const string EditContact = "EditContact";
        public const string EditLocation = "EditLocation";
        public const string EditCost = "EditCost";

        public static bool IsActivityLeader(int? currentAccountId, int? activityAccountId, UserType accountUserType, bool currentUserIsActivityLeader)
        {
            return currentAccountId == activityAccountId && (accountUserType.In(FormConstants.AdminUserTypes) || currentUserIsActivityLeader);
        }

        public static bool GetIsFullScreen(Activity activity, bool isNew)
        {
            return isNew && !activity.IsPublished;
        }

        public static IEnumerable<string> EditSetupPagesForActivityType(ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.Activity:
                    return new[] {EditSetup, EditRestrictions, EditSchedule, EditDetails, EditContact, EditLocation, EditCost};
                case ActivityType.Team:
                    return new[] {EditSetup, EditSchedule, EditContact};
                case ActivityType.FieldTrip:
                    return new[] {EditSetup, EditSchedule, EditDetails, EditContact, EditCost};
                case ActivityType.PermissionSlip:
                    return new[] {EditSetup, EditDetails, EditContact, EditCost};
                case ActivityType.LearningPod:
                    return new[] {EditSetup, EditRestrictions, EditSchedule, EditDetails, EditContact, EditLocation, EditCost};
                case ActivityType.VolunteerOpportunity:
                    return new[] {EditSetup, EditRestrictions, EditSchedule, EditDetails, EditContact, EditLocation};
                case ActivityType.NotSet:
                default:
                    throw new ArgumentOutOfRangeException(nameof(activityType));
            }
        }

        public static string GetNextScreenForActivityType(ActivityType activityType, string currentScreen)
        {
            var allPages = EditSetupPagesForActivityType(activityType).ToArray();
            var currentIndex = Array.IndexOf(allPages, currentScreen);
            var nextIndex = currentIndex + 1;
            return allPages.Length > nextIndex ? allPages[nextIndex] : null;
        }
    }
}