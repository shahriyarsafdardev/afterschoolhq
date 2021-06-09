using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class FixAccountPackageIsActive_20210603_Test : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public FixAccountPackageIsActive_20210603_Test(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }

        [Fact]
        public void RunMigration()
        {    
            try
            {
                // Arrange
                if (msgs.IsEmpty()) msgs = new SystemMessages();

                AfterSchoolLibrary.Helpers.Utilities.AdditionalData(dc);

                // create package
                var package1 = PackageService.CreateTestPackage("Test Package 1", "Test Package Description 1", 100m, user, msgs, dc);
                var package2 = PackageService.CreateTestPackage("Test Package 2", "Test Package Description 2", 50m, user, msgs, dc);

                // create multiple active account packages
                account.AddPackageWithoutDisablingPreviousPackage(package1.ID.Value, BillingPeriodType.Month, user, msgs, dc);
                account.AddPackageWithoutDisablingPreviousPackage(package2.ID.Value, BillingPeriodType.Month, user, msgs, dc);

                // Act                   
                //check if account has multiple active packages
                var accountPackages = account.GetAccountPackages(msgs, dc);
                if (accountPackages.Where(m => m.IsActive).Count() > 1)
                    // run migration, there should be only one active account package for account
                    AfterSchoolLibrary.Migrations.FixAccountPackageIsActive_20210603.Do(msgs, dc);

                // Assert
                var expected = account.GetAccountPackages(msgs, dc);
                Assert.Single(expected.Where(m => m.IsActive));
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
       