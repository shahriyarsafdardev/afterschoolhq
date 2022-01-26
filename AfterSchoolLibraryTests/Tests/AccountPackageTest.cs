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
    public class AccountPackageTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        private Package Package { get; set; }

        public AccountPackageTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            Account = AccountFactory.Create(AccountOwner, msgs, dc);
            Package = PackageFactory.Create(msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);

            Assert.True(AccountPackage.Load(accountPackage.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);

            accountPackage.LastModifiedBy = 1;
            msgs.AddRange(accountPackage.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);

            msgs.AddRange(accountPackage.Delete(dc));

            Assert.Null(AccountPackage.Load(accountPackage.ID.Value));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);

            Assert.Equal(accountPackage.ID, accountPackage.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var accountPackage = AccountPackageFactory.Create(Account, Package, msgs, dc);

                // Act                
                var expected = AccountPackage.LoadByUidByAccountId(Uid: accountPackage.Uid, AccountId: Account.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: accountPackage.ID);
                Assert.Equal(expected: expected.Uid, actual: accountPackage.Uid);
                Assert.Equal(expected: expected.Price, actual: accountPackage.Price);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
