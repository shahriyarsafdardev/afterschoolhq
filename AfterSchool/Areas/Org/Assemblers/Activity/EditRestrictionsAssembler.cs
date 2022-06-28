using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Activity
{
    public class EditRestrictionsResult
    {
        public AfterSchoolLibrary.Models.Activity Activity { get; set; }
        public SystemMessages Messages { get; set; }
        public Account CurrentAccount { get; set; }
        public string ActiveMenu { get; set; }
        public bool IsFullScreen { get; set; }
        public string FullScreenExitUrl { get; set; }
    }

    public class EditRestrictionsAssembler
    {
        public EditRestrictionsResult AssembleEditRestrictionsResult(AfterSchoolLibrary.Models.Activity activity, SystemMessages messages, Account currentAccount, bool isNew,
            string fullScreenExitUrl)
        {
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew);

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