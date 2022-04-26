using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class PackageServiceTest : BaseTest
    {
        private readonly WebUser accountOwner;

        public PackageServiceTest(ITestOutputHelper output) : base(output)
        {
            accountOwner = WebUserFactory.Create(msgs, dc);
            Package = PackageFactory.CreatePackage("Test Package", "Testing Package Description", 10m, accountOwner,
                msgs, dc);
        }

        private Package Package { get; }

        [Fact]
        public void AddPackage()
        {
            var package = Package.AddFeature(FeatureType.ManagePermissionSlips, 100, accountOwner, msgs, dc);

            Assert.NotNull(package);
        }

        [Fact]
        public void CreateDefaultPackage()
        {
            var defaultPackage = PackageService.CreateDefaultPackageFeatures(Package, accountOwner, dc);

            Assert.NotNull(defaultPackage);
        }

        [Fact]
        public void CreateDefaultPackageFeatures()
        {
            var messages = PackageService.CreateDefaultPackageFeatures(Package, accountOwner, dc);

            Assert.NotNull(messages);
        }

        [Fact]
        public void GetPackageWithFeatureLowestPrice()
        {
            var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, Package, msgs, dc);
            var packageFeaturePrices = new List<PackageFeaturePrice>
            {
                new PackageFeaturePrice
                {
                    Package = PackageFactory.CreatePackage("Test Package 1", "Test Package Description 1", 100m,
                        accountOwner, msgs, dc),
                    FeatureTypeIds = new List<int> {(int) FeatureType.ManagePermissionSlips},
                },
                new PackageFeaturePrice
                {
                    Package = PackageFactory.CreatePackage("Test Package 2", "Test Package Description 2", 50m,
                        accountOwner, msgs, dc),
                    FeatureTypeIds = new List<int> {(int) FeatureType.ManagePermissionSlips},
                },
            };
            var packageWithLowestPrice =
                PackageService.GetPackageWithFeatureNextPriceTier(FeatureType.ManagePermissionSlips, accountPackage,
                    packageFeaturePrices, msgs);

            Assert.NotNull(packageWithLowestPrice);
            Assert.Equal(10m, accountPackage.Price);
            Assert.Equal(50m, packageWithLowestPrice.Price);
        }

        [Fact]
        public void EnsurePackageWithFeatureMaximumAvailabilityTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc)
                .First(x => x.FeatureTypeId == FeatureType.ManagePermissionSlips)
                .FeatureTypeId;
            var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
            AccountPackageFactory.Create(account, Package, msgs, dc);

            var requiredPackage =
                PackageService.GetPackageWithFeatureMaximumAvailability(FeatureType.ManagePermissionSlips, 2, msgs, dc);

            Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
            Assert.NotNull(requiredPackage);
        }
    }
}