using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using System;
using BasicLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountPackageServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        private Package Package { get; set; }

        public AccountPackageServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            Account = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.CreateFreePackage(msgs, dc);
        }

        [Fact]
        public void GetAccountTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);
            var a = accountPackage.GetAccount(msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(a);
            Assert.Equal(Account.ID, a.ID);
        }

        [Fact]
        public void RemoveFeatureTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);
            var featureToCheck = FeatureType.AcceptACH;
            var hasFeature1 = accountPackage.HasFeature(featureToCheck, msgs, dc);

            accountPackage.RemoveFeature(featureToCheck, msgs, dc);

            var hasFeature2 = accountPackage.HasFeature(featureToCheck, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.True(hasFeature1);
            Assert.False(hasFeature2);
        }
    }
}
