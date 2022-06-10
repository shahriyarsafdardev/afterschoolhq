using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Controllers
{
    public class EditSetupResult
    {
        public ActivityGroup Group { get; set; }
        public Account Account { get; set; }
        public AccountPackage AccountPackage { get; set; }
        public List<Form> Forms { get; set; }
        public List<RegistrationPeriod> Periods { get; set; }
        public bool IsActivityLeader { get; set; }
        public RegistrationPeriod ActivePeriod { get; set; }
        public ActivityForm ActiveForm { get; set; }
        public bool IsFullScreen { get; set; }
        public SystemMessages Messages { get; set; }
        public string ActiveMenu { get; set; }
        public Activity Activity { get; set; }
        public string FullScreenExitUrl { get; set; }
    }

    public class EditSetupAssembler
    {
        public EditSetupResult AssembleEditSetupResult(Activity activity, ActivityGroup activityGroup, SystemMessages msgs, Account currentAccount,
            string activeMenu, List<RegistrationPeriod> periods, RegistrationPeriod activePeriod, ActivityForm activeForm, AccountPackage accountPackage,
            UserType accountUserType, bool currentUserIsActivityLeader, string isNew, IEnumerable<Form> forms, string fullScreenExitUrl)
        {
            periods.Add(activePeriod);
            periods = periods.OrderBy(period => period.StartDateTime.Value).ToList();
            var formsList = forms.OrderBy(x => x.Name).ToList();
            var isActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, accountUserType, currentUserIsActivityLeader);
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");

            return new EditSetupResult
            {
                Group = activityGroup,
                Account = currentAccount,
                AccountPackage = accountPackage,
                Forms = formsList,
                Periods = periods,
                IsActivityLeader = isActivityLeader,
                ActivePeriod = activePeriod,
                ActiveForm = activeForm,
                IsFullScreen = isFullScreen,
                Messages = msgs,
                ActiveMenu = activeMenu,
                Activity = activity,
                FullScreenExitUrl = fullScreenExitUrl,
            };
        }
    }
}