using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class PackageServiceTest : BaseTest
    {
        private readonly WebUser accountOwner;
        private Package Package { get; }

        public PackageServiceTest(ITestOutputHelper output) : base(output)
        {
            accountOwner = WebUserFactory.Create(msgs, dc);
            Package = PackageFactory.CreatePackage("Test Package", "Testing Package Description", 10, accountOwner, msgs, dc);
        }
        
        [Fact]
        public void AddPackage()
        {
            var package = PackageService.AddFeature(Package, FeatureType.ManagePermissionSlips, 100, accountOwner, msgs, dc);
            
            Assert.True(package != null);
        }
        
        [Fact]
        public void CreateDefaultPackage()
        {
            var defaultPackage = PackageService.CreateDefaultPackageFeatures(Package, accountOwner, dc);
            
            Assert.True(defaultPackage != null);
        }

        [Fact]
        public void CreateDefaultPackageFeatures()
        {
            var messages = PackageService.CreateDefaultPackageFeatures(Package, accountOwner, dc);

            Assert.True(messages != null);
        }

        [Fact]
        public void GetPackageWithFeatureLowestPrice()
        {
            var packageWithLowestPrice = PackageService.GetPackageWithFeatureLowestPrice(FeatureType.ManagePermissionSlips, msgs,dc);

            Assert.True(packageWithLowestPrice != null);
        }

        [Fact]
        public void EnsurePackageWithFeatureMaximumAvailabilityTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc).First(x => x.FeatureTypeId == FeatureType.ManagePermissionSlips)
                .FeatureTypeId;
            var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
            AccountPackageFactory.Create(account, Package, msgs, dc);

            var requiredPackage = PackageService.GetPackageWithFeatureMaximumAvailability(FeatureType.ManagePermissionSlips, 2, msgs, dc);

            Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
            Assert.True(requiredPackage != null);
        }
    }
}
