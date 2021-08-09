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
    public class AccountPackageFeatureServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        private Package Package { get; set; }

        public AccountPackageFeatureServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            Account = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.CreateFreePackage(msgs, dc);
        }

        [Fact]
        public void GetAccountPackageTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);
            var apf = accountPackage.GetFeatures(msgs, dc).First();
            var ap = apf.GetAccountPackage(msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(apf);
            Assert.NotNull(ap);
            Assert.Equal(accountPackage.ID, ap.ID);
        }

        [Fact]
        public void GetAccountTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);
            var apf = accountPackage.GetFeatures(msgs, dc).First();
            var a = apf.GetAccount(msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(apf);
            Assert.NotNull(a);
            Assert.Equal(Account.ID, a.ID);
        }

    }
}
