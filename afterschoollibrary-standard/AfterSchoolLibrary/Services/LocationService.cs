using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary
{
    public static class LocationService
    {
        public static List<Account> GetVisitorAccounts(this Location location, DatabaseConnection dc = null)
        {
            return AccountRelationshipLocation.LoadByLocationId(location.ID.Value, dc: dc)
                .Select(x => x.GetVisitorAccount(dc: dc))
                .ToList();
        }
        
        public static Account GetHostAcount(this Location location, DatabaseConnection dc = null)
        {
            return Account.Load(location.HostAccountId.Value, dc: dc);
        }
    }
}
