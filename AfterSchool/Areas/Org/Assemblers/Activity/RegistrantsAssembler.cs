using System.Collections.Generic;
using System.Linq;
using AfterSchool.Areas.Org.Controllers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Activity
{
    public class RegistrantsResult
    {
        public AfterSchoolLibrary.Models.Activity Activity { get; set; }
        public List<RegistrantActivity> RegistrantActivities { get; set; }
        public Account CurrentAccount { get; set; }
        public WebUser CurrentUser { get; set; }
        public RegistrationAssignmentType AssignmentType { get; set; }
        public AccountPackage AccountPackage { get; set; }
        public bool IsReadOnlySeatStatus { get; set; }
        public bool IsActivityLeader { get; set; }
        public ActivitySchedule ActivitySchedule { get; set; }
        public SystemMessages Messages { get; set; }
    }

    public class RegistrantsAssembler
    {
        public RegistrantsResult AssembleRegistrantsResult(AfterSchoolLibrary.Models.Activity activity, List<RegistrantActivity> registrantActivities,
            Account currentAccount, WebUser currentUser, RegistrationAssignmentType assignmentType, AccountPackage accountPackage, bool isReadOnlySeatStatus,
            ActivitySchedule activitySchedule, UserType userType, bool userIsActivityLeader, SystemMessages messages)
        {
            if (assignmentType != RegistrationAssignmentType.NotSet)
                registrantActivities = registrantActivities.Where(x => x.AssignmentTypeId == assignmentType).OrderBy(x => x.SeatIndex).ToList();

            return new RegistrantsResult
            {
                Activity = activity,
                RegistrantActivities = registrantActivities,
                CurrentAccount = currentAccount,
                CurrentUser = currentUser,
                AssignmentType = assignmentType,
                AccountPackage = accountPackage,
                IsReadOnlySeatStatus = isReadOnlySeatStatus,
                IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, userType, userIsActivityLeader),
                ActivitySchedule = activitySchedule,
                Messages = messages,
            };
        }
    }
}