using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class CleanUpExtraParentAccounts_20200215
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var users = WebUser.Load(msgs: amsgs, dc: adc);

                    users.ForEach(user => {

                        // get all parent awus for the user
                        var awus = AccountWebUser
                                    .LoadByWebUserId(user.ID.Value, amsgs, adc)
                                    .Where(awu => awu.AccountAccountTypeId == AccountType.Registrant)
                                    .OrderBy(awu => awu.ID)
                                    .ToList();

                        // if there is more than one parent account for the user then there's a duplicate parent account
                        if (awus.Count > 1)
                        {
                            // delete all except for the first record
                            awus.Except(awus.First().AsList()).ToList().ForEach(awu => {

                                var registrants = Registrant.LoadByAccountId(awu.AccountId.Value, amsgs, adc);

                                // delete all registrants
                                if (registrants.Any())
                                    registrants.ForEach(registrant => {

                                        // delete registrant web user connections
                                        var rwu = RegistrantWebUser.LoadByRegistrantIdByWebUserId(registrant.ID.Value, user.ID.Value, amsgs, adc);

                                        if (rwu.IsNotEmpty())
                                            amsgs.AddRange(rwu.Delete(adc));

                                        amsgs.AddRange(registrant.Delete(adc)); 
                                    });

                                // delete the awu
                                amsgs.AddRange(awu.Delete(adc));

                            });
                        }

                    });

                },
                uniqueIdenfier: "CleanUpExtraParentAccounts_20200215",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
