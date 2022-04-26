using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AccountPackageService
    {
        public static Account GetAccount(this AccountPackage accountPackage, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (accountPackage.IsNotEmpty())
                return Account.Load(accountPackage.AccountId.Value, msgs, dc);

            return null;
        }

        public static void RemoveFeature(this AccountPackage accountPackage, FeatureType featureType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (accountPackage.IsNotEmpty())
            {
                var apf = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value,
                    featureType, msgs, dc);

                if (apf.IsNotEmpty())
                    msgs.AddRange(apf.Delete(dc));
            }
        }
    }
}