using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public static class EditHelpers
    {
        public static bool IsActivityLeader(int? currentAccountId, int? activityAccountId, UserType accountUserType, bool currentUserIsActivityLeader)
        {
            return currentAccountId == activityAccountId && (accountUserType.In(FormConstants.AdminUserTypes) || currentUserIsActivityLeader);
        }

        public static bool GetIsFullScreen(Activity activity, bool isNew)
        {
            return isNew && !activity.IsPublished;
        }
    }
}