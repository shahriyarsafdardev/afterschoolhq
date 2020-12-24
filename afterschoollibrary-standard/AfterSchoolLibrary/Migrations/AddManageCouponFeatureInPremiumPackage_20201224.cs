using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
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

                    foreach (var package in packages.Where(m => m.Name.ToLower().Contains("premium")))
                    {
                        PackageFeature.AddPackageFeature(PackageId: package.ID, featureTypeId: FeatureType.ManageCoupons, maxValue: 1, loggedInUser: loggedInUser, msgs: amsgs, dc: adc);
                    }
                },
                uniqueIdenfier: "AddManageCouponFeatureInPremiumPackage_20201224",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
