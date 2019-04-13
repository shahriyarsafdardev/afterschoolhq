using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountPackageFeatureFactory
    {
        public static AccountPackageFeature Create(AccountPackage accountPackage, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountPackageFeature()
            {
                AccountPackageId = accountPackage.ID,
                FeatureTypeId = FeatureType.AcceptPayments,
                Max = 1
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static AccountPackageFeature Create(AccountPackage accountPackage, FeatureType featureType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountPackageFeature()
            {
                AccountPackageId = accountPackage.ID,
                FeatureTypeId = featureType,
                Max = 1
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static AccountPackageFeature Create(AccountPackage accountPackage, FeatureType featureType, decimal value, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountPackageFeature()
            {
                AccountPackageId = accountPackage.ID,
                FeatureTypeId = featureType,
                Max = value               
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
