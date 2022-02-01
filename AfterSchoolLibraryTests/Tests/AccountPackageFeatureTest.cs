using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using BasicLibrary;
using System;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountPackageFeatureTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        private Package Package { get; set; }
        private AccountPackage AccountPackage { get; set; }

        public AccountPackageFeatureTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            Account = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.Create(msgs, dc);
            AccountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            Assert.True(AccountPackageFeature.Load(apf.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            apf.LastModifiedBy = 1;
            msgs.AddRange(apf.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            msgs.AddRange(apf.Delete(dc));

            Assert.Null(AccountPackageFeature.Load(apf.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            Assert.Equal(apf.ID, apf.Reload(msgs, dc).ID);
        }
    }
}
