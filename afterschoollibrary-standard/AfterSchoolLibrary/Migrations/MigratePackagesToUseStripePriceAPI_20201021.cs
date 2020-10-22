using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Migrations
{
    public class MigratePackagesToUseStripePricesAPI_20201021
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    List<Package> packages = Package.LoadByIsActive(true, amsgs, adc);

                    packages.ForEach( package =>
                    {
                       // update each package by saving so it uses the stripe price api
                        package.SaveWithStripe(adc);

                        List<AccountPackage> accountPackages = AccountPackage.LoadByPackageId(package.ID.Value, amsgs, adc);

                        accountPackages.ForEach(accountPackage =>
                        {
                            var stripeCustomerId = Account.Load(accountPackage.AccountId.Value, amsgs, adc).StripeCustomerId;
                            var priceId = accountPackage.BillingPeriodTypeId == BillingPeriodType.Month ? package.StripePricePerMonthId : package.StripePricePerYearId;

                            // update each accountpackage in stripe by saving so it uses the stripe price api
                            // Not sure how this works with changing from plans to prices
                            // How are proration and billing anchor dates effected?
                            accountPackage.SaveWithStripe(stripeCustomerId, false, priceId);
                        });
                    });

                },
                uniqueIdenfier: "UpdateActivityMessageRegistrants-20201012",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
