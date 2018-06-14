using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary
{
    public static class AccountRelationshipLocationService
    {
        public static Location GetLocation(this AccountRelationshipLocation alr, DatabaseConnection dc = null)
        {
            return Location.Load(alr.LocationId.Value, dc: dc);
        }
        public static AccountRelationship GetAccountRelationship(this AccountRelationshipLocation alr, DatabaseConnection dc = null)
        {
            return AccountRelationship.Load(alr.AccountRelationshipId.Value, dc: dc);
        }
        public static Account GetHostAccount(this AccountRelationshipLocation alr, DatabaseConnection dc = null)
        {
            return Account.Load(alr.HostAccountId.Value, dc: dc);
        }
        public static Account GetVisitorAccount(this AccountRelationshipLocation alr, DatabaseConnection dc = null)
        {
            return Account.Load(alr.VisitorAccountId.Value, dc: dc);
        }
    }
}
