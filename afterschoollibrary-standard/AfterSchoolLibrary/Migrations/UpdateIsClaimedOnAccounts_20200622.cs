using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateIsClaimedOnAccounts_20200622
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Account.Update(
                        new Hashtable() { { "IsClaimed", true } }, 
                        new Hashtable() { { "IsClaimed", DBNull.Value } }, 
                        Account.ObjectMap, 
                        adc));
                },
                uniqueIdenfier: "UpdateIsClaimedOnAccounts_20200622",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
