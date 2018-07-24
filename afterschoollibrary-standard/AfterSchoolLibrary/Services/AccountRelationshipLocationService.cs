using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AccountRelationshipLocationService
    {
        public static AccountRelationshipLocation CreateNew(AccountRelationship accountRelationship, Account hostAccount, Account visitorAccount, Location location, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var arl = new AccountRelationshipLocation()
            {
                AccountRelationshipId = accountRelationship.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                HostAccountId = hostAccount.ID,
                VisitorAccountId = visitorAccount.ID,
                LocationId = location.ID
            };
            msgs.AddRange(arl.Save(dc));

            return arl;
        }

        public static Location GetLocation(this AccountRelationshipLocation alr, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Location.Load(alr.LocationId.Value, msgs, dc);
        }
        public static AccountRelationship GetAccountRelationship(this AccountRelationshipLocation alr, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountRelationship.Load(alr.AccountRelationshipId.Value, msgs, dc);
        }
        public static Account GetHostAccount(this AccountRelationshipLocation alr, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(alr.HostAccountId.Value, msgs, dc);
        }
        public static Account GetVisitorAccount(this AccountRelationshipLocation alr, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(alr.VisitorAccountId.Value, msgs, dc);
        }
    }
}
