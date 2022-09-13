using System;
using System.Collections;
using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220912_AlterAccountAddReferralCodeAndReferrerId {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (amsgs, adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_ReferralCode ON [Account] (ReferralCode)"));
                    amsgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_ReferrerAccountId ON [Account] (ReferrerAccountId)"));

                    var accounts = Account.Load(msgs: amsgs, dc: adc);
                    accounts.ForEach(account =>
                    {
                        account.ReferralCode = Guid.NewGuid();
                        amsgs.AddRange(account.Save(adc));
                    });

                },
                uniqueIdenfier: "Migration_20220912_AlterAccountAddReferralCodeAndReferrerId",
                msgs: msgs,
                dc: dc
            );
        }
    }
}