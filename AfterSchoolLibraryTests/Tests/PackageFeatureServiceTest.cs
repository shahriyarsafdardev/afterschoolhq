using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using BasicLibrary;
using System;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class PackageFeatureServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        private Package Package { get; set; }

        public PackageFeatureServiceTest(ITestOutputHelper output) : base(output)
        {
            Package = PackageFactory.CreateFreePackage(msgs, dc);
        }

        [Fact]
        public void EnsureAllAccountsHavePackageFeatureTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc).First().FeatureTypeId;
            var accounts = new List<Account>();

            for (var i = 0; i < 5; i++)
            {
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
                var accountPackage = AccountPackageFactory.Create(account, Package, msgs, dc);

                accounts.Add(account);
                accountPackage.RemoveFeature(featureToHave, msgs, dc);
            }

            var packageFeature = PackageFeature.LoadByPackageIdByFeatureTypeId(Package.ID.Value, featureToHave, msgs, dc);
            packageFeature.EnsureAllAccountsHavePackageFeature(AccountService.ProviderAccountTypes.ToList(), new WebUser() { ID = 0 }, msgs, dc);

            foreach (var account in accounts)
            {
                Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
                Assert.Equal(packageFeature.Max, account.GetAccountPackageFeature(featureToHave, msgs, dc).Max);
            }

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void EnsureAccountHasPackageFeatureTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc).First().FeatureTypeId;
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, Package, msgs, dc);

            accountPackage.RemoveFeature(featureToHave, msgs, dc);

            Assert.False(account.HasAccountPackageFeature(featureToHave, msgs, dc));            

            var packageFeature = PackageFeature.LoadByPackageIdByFeatureTypeId(Package.ID.Value, featureToHave, msgs, dc);
            packageFeature.EnsureAccountHasPackageFeature(account, AccountService.ProviderAccountTypes.ToList(), accountOwner, msgs, dc);

            Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
            Assert.Equal(packageFeature.Max, account.GetAccountPackageFeature(featureToHave, msgs, dc).Max);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void ResetAllAccountFeaturesToPackageFeatureTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc).First().FeatureTypeId;
            var featureToHave2 = Package.GetFeatures(msgs, dc).Last().FeatureTypeId;
            var featureToHave2Value = 4893;
            var accounts = new List<Account>();

            for (var i = 0; i < 5; i++)
            {
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
                var accountPackage = AccountPackageFactory.Create(account, Package, msgs, dc);

                accounts.Add(account);
                accountPackage.RemoveFeature(featureToHave, msgs, dc);

                Assert.False(account.HasAccountPackageFeature(featureToHave, msgs, dc));
                Assert.True(account.HasAccountPackageFeature(featureToHave2, msgs, dc));
            }

            var packageFeature = PackageFeature.LoadByPackageIdByFeatureTypeId(Package.ID.Value, featureToHave, msgs, dc);
            packageFeature.ResetAllAccountFeaturesToPackageFeature(AccountService.ProviderAccountTypes.ToList(), true, new WebUser() { ID = 0 }, msgs, dc);

            foreach (var account in accounts)
            {
                Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
                Assert.Equal(packageFeature.Max, account.GetAccountPackageFeature(featureToHave, msgs, dc).Max);

                var accountPackage = account.GetActiveAccountPackage(msgs, dc);
                accountPackage.AddOrUpdatePackageFeature(featureToHave2, featureToHave2Value, new WebUser() { ID = 0 }, msgs, dc);
            }

            packageFeature.ResetAllAccountFeaturesToPackageFeature(AccountService.ProviderAccountTypes.ToList(), false, new WebUser() { ID = 0 }, msgs, dc);

            foreach (var account in accounts)
            {
                Assert.True(account.HasAccountPackageFeature(featureToHave2, msgs, dc));
                Assert.Equal(featureToHave2Value, account.GetAccountPackageFeature(featureToHave2, msgs, dc).Max);
            }

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void ResetAccountFeatureToPackageFeatureTest()
        {
            var featureToHave = Package.GetFeatures(msgs, dc).First().FeatureTypeId;
            var featureToHave2 = Package.GetFeatures(msgs, dc).Last().FeatureTypeId;
            var featureToHave2Value = 4893;
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, AccountType.ActivityProvider, msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, Package, msgs, dc);

            accountPackage.RemoveFeature(featureToHave, msgs, dc);

            Assert.False(account.HasAccountPackageFeature(featureToHave, msgs, dc));

            var packageFeature = PackageFeature.LoadByPackageIdByFeatureTypeId(Package.ID.Value, featureToHave, msgs, dc);
            packageFeature.ResetAccountFeatureToPackageFeature(account, AccountService.ProviderAccountTypes.ToList(), true, accountOwner, msgs, dc);

            Assert.True(account.HasAccountPackageFeature(featureToHave, msgs, dc));
            Assert.Equal(packageFeature.Max, account.GetAccountPackageFeature(featureToHave, msgs, dc).Max);

            accountPackage.AddOrUpdatePackageFeature(featureToHave2, featureToHave2Value, accountOwner, msgs, dc);
            packageFeature.ResetAccountFeatureToPackageFeature(account, AccountService.ProviderAccountTypes.ToList(), false, accountOwner, msgs, dc);

            Assert.True(account.HasAccountPackageFeature(featureToHave2, msgs, dc));
            Assert.Equal(featureToHave2Value, account.GetAccountPackageFeature(featureToHave2, msgs, dc).Max);

            Assert.True(msgs.HasNoErrors());
        }

    }
}
