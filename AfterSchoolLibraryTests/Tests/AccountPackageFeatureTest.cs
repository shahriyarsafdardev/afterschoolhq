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

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                AccountPackageFeature.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of transactions in the database
        /// </summary>
        /// <param name="numberToCreate"></param>
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);
            }

            Assert.Equal(numberToCreate, AccountPackageFeature.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the plan to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            apf.LastModifiedBy = 1;
            msgs.AddRange(apf.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            msgs.AddRange(apf.Delete(dc));

            Assert.Equal(0, AccountPackageFeature.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var apf = AccountPackageFeatureFactory.Create(AccountPackage, msgs, dc);

            Assert.Equal(apf.ID, apf.Reload(msgs, dc).ID);
        }
    }
}
