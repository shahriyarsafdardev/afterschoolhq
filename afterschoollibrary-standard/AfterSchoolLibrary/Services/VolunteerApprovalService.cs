using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public class VolunteerApprovalService
    {
        public static VolunteerApproval AddApproval(int? webUserId, int? accountId, ApprovalStatus approvalStatus, string reason,DateTime expirationDate ,WebUser loggedInUser ,SystemMessages msgs,
            DatabaseConnection dc = null)
        {
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

            msgs.AddRange(volunteerApproval.Save(dc: dc));
            return volunteerApproval;
        }
    }
}