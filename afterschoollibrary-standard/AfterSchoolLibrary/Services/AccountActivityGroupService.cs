using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AccountActivityGroupService
    {
        public static AccountActivityGroup CreateNew(Account account, ActivityGroup activityGroup, InviteStatus inviteStatus, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var aag = AccountActivityGroup.LoadByAccountIdByActivityGroupId(account.ID.Value, activityGroup.ID.Value, msgs, dc);

            if (aag.IsEmpty())
            {
                aag = new AccountActivityGroup()
                {
                    AccountId = account.ID,
                    ActivityGroupId = activityGroup.ID,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    InviteStatusId = inviteStatus,
                    IsPublished = false,
                    LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                    LastModifiedDate = DateTime.UtcNow
                };
                msgs.AddRange(aag.Save(dc));
            }
            else
            {
                msgs.Add("Invite already exists", MessageType.Notice);
            }

            return aag;
        }
    }
}
