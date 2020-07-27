using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Migrations
{
    public class AddProfilesToAllAccounts_20200727
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var accounts = Account.LoadByAccountTypeIds(
                                        new List<AccountType>() 
                                        {
                                            AccountType.ActivityProvider,
                                            AccountType.School,
                                        }, 
                                        false, 
                                        null, 
                                        null, 
                                        amsgs, 
                                        adc);

                    accounts.ForEach(account => {
                        if (!account.HasAccountProfile(amsgs, adc))
                            account.GetOrCreateAccountProfile(new WebUser { ID = 0 }, amsgs, adc);
                    });
                },
                uniqueIdenfier: "AddProfilesToAllAccounts_20200727",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
