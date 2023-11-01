using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20231101_HideTimezoneOnActivityPage
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var webUser = WebUser.Load(1, amsgs, adc);
                    // update packages, set HideTimezoneOnActivityPage feature to zero
                    Package.LoadByIsActive(true, amsgs, adc).ForEach(package => package.AddFeature(FeatureType.HideTimezoneOnActivityPage, 0, webUser, amsgs, adc));
                    // get all accounts with packages
                    var accounts = Account.LoadByAccountTypeIds(FormConstants.ProviderAccountTypes.ToList(), false, null, null, amsgs, adc);
                    accounts.ForEach(account =>
                    {
                        var accountPackage = account.GetActiveAccountPackage(amsgs, adc);
                        if (accountPackage.IsNotEmpty())
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.HideTimezoneOnActivityPage, 0, webUser, amsgs, adc);
                        else
                            amsgs.Add($"{account.Name} missing package??", MessageType.Notice);
                    });
                },
                uniqueIdenfier: "Migration_20231101_HideTimezoneOnActivityPage",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
