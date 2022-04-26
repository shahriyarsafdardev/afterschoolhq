using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountPackageFactory
    {
        public static AccountPackage Create(Account account, Package package, SystemMessages msgs,
            DatabaseConnection dc)
        {
            var obj = new AccountPackage
            {
                AccountId = account.ID,
                PackageId = package.ID,
                BillingPeriodTypeId = BillingPeriodType.Month,
                IsActive = true,
                Price = package.Price,
                StartDate = DateTime.UtcNow.AddMonths(-1),
            };
            msgs.AddRange(obj.Save(dc));

            package.CopyFeaturesToAccountPackage(obj, 0, msgs, dc);

            return obj;
        }
    }
}