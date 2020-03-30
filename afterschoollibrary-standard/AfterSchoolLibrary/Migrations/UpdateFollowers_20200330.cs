using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateFollowers_20200330
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var parentAccounts = Account.LoadByAccountTypeId(AccountType.Registrant, amsgs, adc);
                    var providerAccounts = Account.LoadByAccountTypeIds(new List<AccountType>() { AccountType.ActivityProvider, AccountType.School }, null, null, amsgs, adc);

                    parentAccounts.ForEach(parentAccount => {

                        var user = parentAccount.GetFirstParentOrGuardianRegistrant(amsgs, adc)?.GetWebUser(amsgs, adc);

                        if (user.IsNotEmpty())
                            providerAccounts.ForEach(providerAccount => {
                                if (!user.IsSubscribedTo(providerAccount, amsgs, adc) && parentAccount.HasRegistrationsWith(providerAccount, amsgs, adc))
                                    providerAccount.AddOrUpdateSubscriber(user, SubscriberStatus.Subscribed, new WebUser() { ID = 0 }, amsgs, adc);
                            });
                    });
                },
                uniqueIdenfier: "UpdateFollowers_20200330",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
