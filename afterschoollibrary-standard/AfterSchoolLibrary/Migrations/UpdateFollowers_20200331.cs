using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateFollowers_20200331
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = Account.get_DatabaseQuery(Account.ObjectMap, typeof(Account).Name);
                    dq.Connection = adc;

                    var sql = $@"
	                    select 
	                        distinct 
		                        a1.ID as FamilyAccountId, a1.Name as FamilyAccountName, 
                                a2.ID as EducatorAccountId, a2.Name as EducatorAccountName, 
                                wu.ID as ParentUserId, wu.Email as ParentEmail
                        from 
	                        account a1 inner join
	                        registrant r1 on a1.ID = r1.AccountId inner join
	                        registrant r2 on r1.ID = r2.ParentRegistrantId inner join
	                        account a2 on r2.AccountId = a2.ID inner join
	                        registrant r3 on a1.ID = r3.AccountId and r3.IsParentOrGuardian = 1 inner join
	                        WebUser wu on r3.WebUserId = wu.ID
                        where 
	                        a1.AccountTypeId = 5 and
	                        a1.IsTest = 0 and
	                        a1.IsDeleted = 0 and
	                        a2.IsTest = 0 and
	                        a2.IsDeleted = 0
                        order by a1.Name";

                    var dt = dq.ExecuteSelect(sql, amsgs);

                    foreach(DataRow row in dt.Rows)
                    {
                        var educatorAccountId = row["EducatorAccountId"].ToString().ToInteger();
                        var educatorAccount = new Account() { ID = educatorAccountId };
                        var parentUserId = row["ParentUserId"].ToString().ToInteger();
                        var parentUserAccount = new WebUser() { ID = parentUserId };

                        educatorAccount.AddOrUpdateSubscriber(parentUserAccount, SubscriberStatus.Subscribed, new WebUser { ID = 0 }, amsgs, adc);
                    }
                },
                uniqueIdenfier: "UpdateFollowers_20200331",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
