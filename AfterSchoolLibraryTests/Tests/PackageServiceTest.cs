using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class PackageServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        private Package Package { get; set; }
        public PackageServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            Package = PackageFactory.CreatePackage("Test Package","Testing Package Description",10,AccountOwner,msgs, dc);
        }

        [Fact]
        public void EnsurePackageWithFeatureMaximumAvailabilityTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc).Where(x => x.FeatureTypeId == FeatureType.ManagePermissionSlips).First().FeatureTypeId;
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, Package, msgs, dc);
            var requiredPackage = PackageService.GetPackageWithFeatureMaximumAvailability(FeatureType.ManagePermissionSlips, 3, msgs, dc);
            

            Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
            Assert.True(requiredPackage != null);

            requiredPackage = PackageService.GetPackageWithFeatureMaximumAvailability(FeatureType.ManagePermissionSlips, 3, msgs, dc);

            Assert.True(requiredPackage == null);           
        }
    }
}
