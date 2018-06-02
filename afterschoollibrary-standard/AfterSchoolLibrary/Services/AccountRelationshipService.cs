using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary
{
    public static class AccountRelationshipService
    {
        public static Account GetHostAccount(this AccountRelationship ar, DatabaseConnection dc = null)
        {
            return Account.Load(ar.HostAccountId.Value, dc);
        }
        public static Account GetVisitorAccount(this AccountRelationship ar, DatabaseConnection dc = null)
        {
            return Account.Load(ar.VisitorAccountId.Value, dc);
        }
    }
}
