using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class AccountRelationshipService
    {
        public static Account GetHostAccount(this AccountRelationship ar, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(ar.HostAccountId.Value, msgs, dc);
        }

        public static Account GetVisitorAccount(this AccountRelationship ar, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(ar.VisitorAccountId.Value, msgs, dc);
        }

        public static AccountRelationship GetOrCreateNew(Account hostAccount, Account visitorAccount, ApprovalStatus approvalStatus, HostPermissionType hostPermissionType, bool requiresPublishPermission, bool showAllPrograms, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountRelationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value, msgs, dc);

            return accountRelationship ?? CreateNew(hostAccount, visitorAccount, approvalStatus, hostPermissionType, requiresPublishPermission, showAllPrograms, loggedInUser, msgs, dc);
        }

        public static AccountRelationship CreateNew(Account hostAccount, Account visitorAccount, ApprovalStatus approvalStatus, HostPermissionType hostPermissionType, bool requiresPublishPermission, bool showAllPrograms, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountRelationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value, msgs, dc);

            if (accountRelationship.IsEmpty())
            {
                accountRelationship = new AccountRelationship
                {
                    VisitorAccountId = visitorAccount.ID,
                    HostAccountId = hostAccount.ID,
                    ApprovalStatusId = approvalStatus,
                    HostPermissionTypeId = hostPermissionType,
                    RequiresPublishPermission = requiresPublishPermission,
                    ShowAllPrograms = showAllPrograms,
                    CreatedBy = loggedInUser.ID,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedBy = loggedInUser.ID,
                    LastModifiedDate = DateTime.UtcNow
                };

                msgs.AddRange(accountRelationship.Save(dc: dc));
            }

            return accountRelationship;
        }

        public static SystemMessages UpdateApprovalStatus(this AccountRelationship accountRelationship, ApprovalStatus approvalStatus, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            accountRelationship.ApprovalStatusId = approvalStatus;
            accountRelationship.LastModifiedBy = loggedInUser.ID;
            accountRelationship.LastModifiedDate = DateTime.UtcNow;
            return accountRelationship.Save(dc);
        }

        public static SystemMessages UpdateApprovalStatus(Account hostAccount, Account visitorAccount, ApprovalStatus approvalStatus, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.LoadByHostAccountIdByVisitorAccountId(hostAccount.ID.Value, visitorAccount.ID.Value, dc: dc);

            if (accountRelationship.IsNotEmpty())
                msgs.AddRange(accountRelationship.UpdateApprovalStatus(approvalStatus, loggedInUser, dc));
            else
                msgs.Add("No access request found", MessageType.UserError);

            return msgs;
        }
        
        public static void UpdateRequiresPublishPermission(this AccountRelationship accountRelationship, bool requiresPublishPermission, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountRelationship.RequiresPublishPermission = requiresPublishPermission;

            if (requiresPublishPermission)
                accountRelationship.ShowAllPrograms = false;

            accountRelationship.LastModifiedDate = DateTime.UtcNow;
            accountRelationship.LastModifiedBy = loggedInUser.ID.Value;
            msgs.AddRange(accountRelationship.Save(dc));
        }

        public static void UpdateShowAllPrograms(this AccountRelationship accountRelationship, bool showAllPrograms, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountRelationship.ShowAllPrograms = showAllPrograms;
            accountRelationship.LastModifiedDate = DateTime.UtcNow;
            accountRelationship.LastModifiedBy = loggedInUser.ID.Value;
            msgs.AddRange(accountRelationship.Save(dc));
        }
    }
}
