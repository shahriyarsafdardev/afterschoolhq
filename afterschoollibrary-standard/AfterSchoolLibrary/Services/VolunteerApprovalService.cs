using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public class VolunteerApprovalService
    {
        public static VolunteerApproval CreateNewApproval(int? webUserId, int? accountId, ApprovalStatus approvalStatus, string reason,DateTime expirationDate ,WebUser loggedInUser ,SystemMessages msgs,
            DatabaseConnection dc = null)
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
            volunteerApproval.CreateDate = DateTime.UtcNow;
            volunteerApproval.CreatedBy = loggedInUser?.ID;
            volunteerApproval.LastModifiedBy = loggedInUser?.ID;
            volunteerApproval.LastModifiedDate = DateTime.UtcNow;
            volunteerApproval.IsActive = true;

            msgs.AddRange(volunteerApproval.Save(dc: dc));
            return volunteerApproval;
        }
    }
}