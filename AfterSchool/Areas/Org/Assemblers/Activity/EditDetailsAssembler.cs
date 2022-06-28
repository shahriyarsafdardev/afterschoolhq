using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Activity
{
    public class EditDetailsResult
    {
        public Account Account { get; set; }
        public SystemMessages Messages { get; set; }
        public bool IsFullScreen { get; set; }
        public AfterSchoolLibrary.Models.Activity Activity { get; set; }
        public string FullScreenExitUrl { get; set; }
        public bool IsActivityLeader { get; set; }
        public AccountPackage AccountPackage { get; set; }
        public string ActiveMenu { get; set; }
    }

    public class EditDetailsAssembler
    {
        public EditDetailsResult AssembleEditDetailsResult(Account account, SystemMessages messages, AfterSchoolLibrary.Models.Activity activity,
            string fullScreenExitUrl, bool isActivityLeader, AccountPackage accountPackage, bool isNew)
        {
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew);

            return new EditDetailsResult
            {
                Account = account,
                Messages = messages,
                IsFullScreen = isFullScreen,
                Activity = activity,
                FullScreenExitUrl = fullScreenExitUrl,
                IsActivityLeader = isActivityLeader,
                AccountPackage = accountPackage,
                ActiveMenu = "EditDetails",
            };
        }
    }
}