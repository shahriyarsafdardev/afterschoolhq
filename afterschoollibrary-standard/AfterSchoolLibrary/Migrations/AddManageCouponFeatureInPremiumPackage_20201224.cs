using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class AddManageCouponFeatureInPremiumPackage_20201224
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var packages = Package.Load(msgs: amsgs, dc: adc);
                    var loggedInUser = WebUser.Load(1, amsgs, adc);
                    var premiumPackage = packages.Where(m => m.Name.ToLower().Contains("premium")).FirstOrDefault();

                    if (premiumPackage.IsNotEmpty())
                    {
                        //all packages where cost is greater than or equal to premium
                        var packagesToUpdate = packages.Where(m => m.Price >= premiumPackage.Price).ToList();

                        foreach (var package in packagesToUpdate)
                        {
                            PackageFeatureService.AddPackageFeature(PackageId: package.ID, featureTypeId: FeatureType.ManageCoupons, maxValue: 1, loggedInUser: loggedInUser.ID.Value, msgs: amsgs, dc: adc);
                        }
                    }
                },
                uniqueIdenfier: "AddManageCouponFeatureInPremiumPackage_20201224",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
