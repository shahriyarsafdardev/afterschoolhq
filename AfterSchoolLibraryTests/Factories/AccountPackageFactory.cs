using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountPackageFactory
    {
        public static AccountPackage Create(Account account, Package package, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountPackage()
            {
                AccountId = account.ID,
                PackageId = package.ID,
                BillingPeriodTypeId = BillingPeriodType.Month,
                IsActive = true,
                Price = 100,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            };
            msgs.AddRange(obj.Save(dc));

            package.CopyFeaturesToAccountPackage(obj, 0, msgs, dc);

            return obj;
        }
    }
}
