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

                    freePackageFeatures.ForEach(packageFeature =>
                    {
                        var resetIfAcctValueIsGreaterThan = false;
                        var accountTypes = AccountService.ProviderAccountTypes;

                        switch (packageFeature.FeatureTypeId)
                        {
                            case FeatureType.ManageLeagues:
                                resetIfAcctValueIsGreaterThan = true;
                                break;

                            case FeatureType.UseStripeConnectedAccount:
                                resetIfAcctValueIsGreaterThan = true;
                                accountTypes = AccountService.IndividualProviderAccountTypes;
                                break;
                        }

                        packageFeature.ResetAllAccountFeaturesToPackageFeature(
                            accountTypes.ToList(),
                            resetIfAcctValueIsGreaterThan,
                            new WebUser() { ID = 0 }, 
                            amsgs, 
                            adc);

                    });

                },
                uniqueIdenfier: "ResetFreeAcctsToFreePackage_20210805",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
