using System;
using System.Linq;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class VolunteerApprovalService
    {
        public static VolunteerApproval CreateNewApproval(int? webUserId, int? accountId, ApprovalStatus approvalStatus, string reason,DateTime expirationDate 
            ,WebUser loggedInUser, DateTime CreateDate, SystemMessages msgs, DatabaseConnection dc = null)
        {

            var previousApproval = VolunteerApproval.LoadByWebUserId((int) webUserId, msgs, dc);

            if (previousApproval.IsNotEmpty())
            {
                foreach (var approval in previousApproval)
                {
                    approval.IsActive = false;
                    msgs.AddRange(approval.Save(dc:dc));
                }
            }
            
            var volunteerApproval = new VolunteerApproval();

            volunteerApproval.WebUserId = webUserId;
            volunteerApproval.AccountId = accountId;
            volunteerApproval.ApprovalStatusId = approvalStatus;
            volunteerApproval.Reason = reason;
            volunteerApproval.ExpirationDate = expirationDate;
            volunteerApproval.CreateDate = CreateDate;
            volunteerApproval.CreatedBy = loggedInUser?.ID;
            volunteerApproval.LastModifiedBy = loggedInUser?.ID;
            volunteerApproval.LastModifiedDate = DateTime.UtcNow;
            volunteerApproval.IsActive = true;

            msgs.AddRange(volunteerApproval.Save(dc: dc));
            
            if (msgs.HasNoErrors() && approvalStatus == ApprovalStatus.Approved)
                UpdateVolunteerRegistrationStatus(webUserId, msgs, dc: dc);
            
            // if(msgs.HasNoErrors())
            //     EmailHelper.SendVolunteerBackgroundApprovalDenialEmail(webUserId, approvalStatus);
            
            return volunteerApproval;
        }

        private static void UpdateVolunteerRegistrationStatus(int? webUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrants = Registrant.LoadByWebUserId((int) webUserId, RegistrantPersonType.Volunteer, false,
                false, msgs, dc);
            registrants.ForEach(registrant =>
            {
                var registration = Registration.LoadByRegistrantId(registrant.ID.Value, msgs, dc).FirstOrDefault();
                if (registration.IsNotEmpty())
                {
                    var participationObject = registration.GetFirstParticipationObject(msgs, dc);
                    var registrantActivity = registration.GetRegistrantActivity(participationObject, msgs, dc);
                    if (registrantActivity.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingBackgroundCheck)
                        registrantActivity.SetAssignmentType(RegistrationAssignmentType.Reserved, null, msgs, dc);
                }
            });
        }

        public static bool IsCurrentlyDenied(this VolunteerApproval volunteerApproval)
        {
            return
                volunteerApproval.ApprovalStatusId == ApprovalStatus.Denied
                && volunteerApproval.ExpirationDate.IsNotEmpty()
                && volunteerApproval.ExpirationDate >= DateTime.UtcNow;
        }
        private static void UpdateVolunteerRegistrationStatus(int? webUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrants = Registrant.LoadByWebUserId((int) webUserId, RegistrantPersonType.Volunteer, false,
                false, msgs, dc);
            registrants.ForEach(registrant =>
            {
                var registration = Registration.LoadByRegistrantId(registrant.ID.Value, msgs, dc).FirstOrDefault();
                if (registration.IsNotEmpty())
                {
                    var participationObject = registration.GetFirstParticipationObject(msgs, dc);
                    var registrantActivity = registration.GetRegistrantActivity(participationObject, msgs, dc);
                    if (registrantActivity.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingBackgroundCheck)
                        registrantActivity.SetAssignmentType(RegistrationAssignmentType.Reserved, null, msgs, dc);
                }
            });
        }
    }
}