using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class AddFeatureTypeSurvey_20211004
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var webUser = WebUser.Load(1, amsgs, adc);

                    // update packages, set survey feature to zero
                    Package.LoadByIsActive(true, amsgs, adc).ForEach(package => package.AddFeature(FeatureType.Survey, 0, webUser, amsgs, adc));


                    // get all accounts with packages
                    var accounts = Account.LoadByAccountTypeIds(AccountService.ProviderAccountTypes.ToList(), false, null, null, amsgs, adc);
                    accounts.ForEach(account =>
                    {
                        var accountPackage = account.GetActiveAccountPackage(amsgs, adc);

                        if (accountPackage.IsNotEmpty())
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.Survey, 0, webUser, amsgs, adc);
                        else
                            amsgs.Add($"{account.Name} missing package??", MessageType.Notice);
                    });
                },
                uniqueIdenfier: "AddFeatureTypeSurvey_20211004",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
