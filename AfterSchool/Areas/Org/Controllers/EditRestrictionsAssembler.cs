using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class EditRestrictionsResult
    {
        public Activity Activity { get; set; }
        public SystemMessages Messages { get; set; }
        public Account CurrentAccount { get; set; }
        public string ActiveMenu { get; set; }
        public bool IsFullScreen { get; set; }
        public string FullScreenExitUrl { get; set; }
    }

    public class EditRestrictionsAssembler
    {
        public EditRestrictionsResult AssembleEditRestrictionsResult(Activity activity, SystemMessages messages, Account currentAccount, bool isNew,
            string fullScreenExitUrl)
        {
            var isFullScreen = ActivityController.GetIsFullScreen(activity, isNew);

            return new EditRestrictionsResult
            {
                Activity = activity,
                Messages = messages,
                CurrentAccount = currentAccount,
                ActiveMenu = "EditRestrictions",
                IsFullScreen = isFullScreen,
                FullScreenExitUrl = fullScreenExitUrl,
            };
        }
    }
}