using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class EditContactResult
    {
        public Account Account { get; set; }
        public SystemMessages Messages { get; set; }
        public Activity Activity { get; set; }
        public bool IsFullScreen { get; set; }
        public string FullScreenExitUrl { get; set; }
        public bool IsActivityLeader { get; set; }
        public List<WebUser> Instructors { get; set; }
        public string ActiveMenu { get; set; }
    }

    public class EditContactAssembler
    {
        public EditContactResult AssembleEditContactResult(Account account, SystemMessages messages, Activity activity, string isNew, string fullScreenExitUrl,
            List<WebUser> instructors, UserType accountUserType, bool currentUserIsActivityLeader)
        {
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");
            var isActivityLeader = EditHelpers.IsActivityLeader(account.ID, activity.AccountId, accountUserType, currentUserIsActivityLeader);
            return new EditContactResult
            {
                Account = account,
                Messages = messages,
                Activity = activity,
                IsFullScreen = isFullScreen,
                FullScreenExitUrl = fullScreenExitUrl,
                IsActivityLeader = isActivityLeader,
                Instructors = instructors,
                ActiveMenu = "EditContact",
            };
        }
    }
}