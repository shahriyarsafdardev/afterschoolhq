using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class ResetFreeAcctsToFreePackage_20210805
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var freePackage = Package.LoadByIsActive(true, amsgs, adc).Last(package => package.IsFree(BillingPeriodType.Month));
                    var freePackageFeatures = freePackage.GetFeatures(amsgs, adc);
                    var accounts = Account.LoadByAccountTypeIds(AccountService.ProviderAccountTypesAndNotSet.ToList(), false, null, null, amsgs, adc);

                    accounts.ForEach(account => {
                        var accountPackage = account.GetActiveAccountPackage(amsgs, adc);

                        if (accountPackage.PackageId == freePackage.ID)
                        {
                            freePackageFeatures.ForEach(packageFeature =>
                            {
                                var accountPackageFeature = accountPackage.GetFeature(packageFeature.FeatureTypeId, amsgs, adc);

                                if (accountPackageFeature.IsEmpty())
                                {
                                    accountPackage.AddPackageFeature(packageFeature, new WebUser() { ID = 0 }, amsgs, adc);
                                }
                                else
                                {
                                    switch(accountPackageFeature.FeatureTypeId)
                                    {
                                        case FeatureType.PaymentsPercentageFee:
                                        case FeatureType.PaymentsPercentageFlatFee:
                                            break;
                                        case FeatureType.ManageLeagues:
                                            accountPackage.AddOrUpdatePackageFeature(
                                                packageFeature.FeatureTypeId,
                                                0,
                                                new WebUser() { ID = 0 },
                                                amsgs,
                                                adc);
                                            break;
                                        default:
                                            if (accountPackageFeature.Max < packageFeature.Max)
                                                accountPackage.AddOrUpdatePackageFeature(
                                                    packageFeature.FeatureTypeId,
                                                    packageFeature.Max.Value,
                                                    new WebUser() { ID = 0 },
                                                    amsgs,
                                                    adc);
                                            
                                            break;
                                    }
                                }

                            });
                        }
                    });

                },
                uniqueIdenfier: "ResetFreeAcctsToFreePackage_20210805",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
