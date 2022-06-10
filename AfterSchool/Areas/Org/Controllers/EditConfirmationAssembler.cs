using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class EditConfirmationResult
    {
        public Activity Activity { get; set; }
        public Account Account { get; set; }
        public SystemMessages Messages { get; set; }
        public bool IsFullScreen { get; set; }
        public bool IsActivityLeader { get; set; }
        public string FullScreenExitUrl { get; set; }
    }
    public class EditConfirmationAssembler
    {
        public EditConfirmationResult AssembleEditConfirmationResult(Account currentAccount, Activity activity, UserType accountUserType, bool currentUserIsActivityLeader, string isNew,
            string fullScreenExitUrl, SystemMessages msgs)
        {
            return new EditConfirmationResult
            {
                Account = currentAccount,
                Activity = activity,
                IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, accountUserType,
                    currentUserIsActivityLeader),
                IsFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1"),
                FullScreenExitUrl = fullScreenExitUrl,
                Messages = msgs,
            };
        }
    }
}