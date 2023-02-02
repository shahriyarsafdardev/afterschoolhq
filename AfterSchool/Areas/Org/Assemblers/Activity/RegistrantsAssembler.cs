using System;
using System.Collections.Generic;
using System.Globalization;
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

        public UserType AccountUserType { get; set; }
        public RegistrationAssignmentType AssignmentType { get; set; }
        public AccountPackage AccountPackage { get; set; }
        public bool IsReadOnlySeatStatus { get; set; }
        public bool IsActivityLeader { get; set; }
        public ActivitySchedule ActivitySchedule { get; set; }
        public int RegistrantCount { get; set; }
        public int WaitingListCount { get; set; }
        public int ApplicationCount { get; set; }
        public int InvitationCount { get; set; }
        public int DeclinedCount { get; set; }
        public int PendingBackgroundCheckCount { get; set; }
        public SystemMessages Messages { get; set; }
    }

    public class RegistrantsAssembler
    {
        public RegistrantsResult AssembleRegistrantsResult(AfterSchoolLibrary.Models.Activity activity, List<RegistrantActivity> registrantActivities,
            Account currentAccount, WebUser currentUser, RegistrationAssignmentType assignmentType, AccountPackage accountPackage, bool isReadOnlySeatStatus,
            ActivitySchedule activitySchedule, UserType userType, bool userIsActivityLeader, SystemMessages messages, string date = null)
        {
            var registrantCount = registrantActivities.Count(x => x.AssignmentTypeId.In(AfterSchoolLibrary.Models.Registration.ReservedStatuses));
            var waitingListCount = registrantActivities.Count(x => x.AssignmentTypeId == RegistrationAssignmentType.WaitingList);
            var applicationCount = registrantActivities.Count(x => x.AssignmentTypeId == RegistrationAssignmentType.Applied);
            var invitationCount = registrantActivities.Count(x => x.AssignmentTypeId == RegistrationAssignmentType.Invited);
            var declinedCount = registrantActivities.Count(x => x.AssignmentTypeId == RegistrationAssignmentType.Declined);
            var pendingBackgroundCheckCount = registrantActivities.Count(x => x.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingBackgroundCheck);
            
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
                RegistrantCount = registrantCount,
                WaitingListCount = waitingListCount,
                ApplicationCount = applicationCount,
                InvitationCount = invitationCount,
                DeclinedCount = declinedCount,
                PendingBackgroundCheckCount = pendingBackgroundCheckCount,
                Messages = messages,
                AccountUserType = userType,
            };
        }
    }
}