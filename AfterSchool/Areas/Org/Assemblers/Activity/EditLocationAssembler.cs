using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Activity
{
    public class EditLocationResult
    {
        public Account Account { get; set; }
        public SystemMessages Messages { get; set; }
        public bool IsFullScreen { get; set; }
        public AfterSchoolLibrary.Models.Activity Activity { get; set; }
        public bool IsActivityLeader { get; set; }
        public string FullScreenExitUrl { get; set; }
        public Location Location { get; set; }
    }

    public class EditLocationAssembler
    {
        public EditLocationResult AssembleEditLocationResult(Account account, AfterSchoolLibrary.Models.Activity activity, SystemMessages messages,
            string isNew, UserType accountUserType, bool currentUserIsActivityLeader, string fullScreenExitUrl, Location location)
        {
            var isActivityLeader = EditHelpers.IsActivityLeader(account.ID, activity.AccountId, accountUserType, currentUserIsActivityLeader);
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");
            return new EditLocationResult
            {
                Account = account,
                Activity = activity,
                Messages = messages,
                IsActivityLeader = isActivityLeader,
                IsFullScreen = isFullScreen,
                FullScreenExitUrl = fullScreenExitUrl,
                Location = location ?? new Location(),
            };
        }
    }
}