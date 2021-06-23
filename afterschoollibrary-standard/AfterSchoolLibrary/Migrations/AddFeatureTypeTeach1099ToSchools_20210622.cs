using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class AddFeatureTypeTeacher1099ToSchools_20210622
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // update packages, set teacher1099 feature to zero
                    var packages = Package.LoadByIsActive(true, amsgs, adc);
                    packages.ForEach(package =>
                   {
                       var packageFeature = new PackageFeature()
                       {
                           PackageId = package.ID.Value,
                           FeatureTypeId = FeatureType.Teacher1099,
                           Max = 0,
                       };

                       amsgs.AddRange(packageFeature.Save(adc));
                   });

                    // get all accounts with packages
                    var accounts = Account.LoadByAccountTypeIds(AccountService.ProviderAccountTypes.ToList(), false, null, null, amsgs, adc);
                    var webUser = WebUser.Load(1, amsgs, adc);
                    accounts.ForEach(account =>
                    {
                        var accountPackage = account.GetActiveAccountPackage(amsgs, adc);

                        if (accountPackage.IsNotEmpty())
                        {
                            // add teacher1099 value
                            var max = 0;
                            // if school account teacher1099 = 1
                            if (account.IsSchool)
                                max = 1;

                            accountPackage.AddOrUpdatePackageFeature(FeatureType.Teacher1099, max, webUser, amsgs, adc);

                        } else
                        {
                            amsgs.Add($"{account.Name} missing package??", MessageType.Notice);
                        }
                    });
                },
                uniqueIdenfier: "AddFeatureTypeTeacher1099ToSchools_20210622-1",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
