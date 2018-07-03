using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class AccountTest : BaseTest
    {
        public AccountTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            Assert.NotNull(Account.Load(account.ID.Value, msgs, dc));
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberOfAccountsToCreate, int numberOfAccountsPerPage)
        {
            for( var i = 0; i < numberOfAccountsToCreate; i++)
            {
                var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberOfAccountsPerPage, numberOfAccountsToCreate), 
                Account.Load(null, 0, numberOfAccountsPerPage, msgs, dc).Count
            );
        }

        //Gets the Number of accounts in the DB
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberOfAccountsToCreate)
        {
            for (var i = 0; i < numberOfAccountsToCreate; i++)
            {
                var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            }

            Assert.Equal(numberOfAccountsToCreate, Account.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the account to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            account.AccountTypeId = AccountType.ActivityProvider;
            msgs.AddRange(account.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the account from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            msgs.AddRange(account.Delete(dc));

            Assert.Equal(0, Account.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the account using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            Assert.Equal(account.ID, account.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByOwnerIdTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            Assert.Single(Account.LoadByOwnerId(accountOwner.ID.Value, msgs, dc));
        }

        [Fact]
        public void LoadByUniqueNameTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            account.UniqueName = AccountService.GenerateUniqueName(account.Name, account.ID, dc);
            msgs.AddRange(account.Save(dc));

            Assert.Equal(account.ID, Account.LoadByUniqueName(account.UniqueName, msgs, dc).ID);
        }

        /// <summary>
        /// testing for admin account which is changing to use AccountType in the future
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadByIsAdminTest(bool isAdmin)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, isAdmin, msgs, dc);

            Assert.NotNull(Account.LoadByIsAdminAccount(isAdmin, msgs, dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            Assert.NotNull(Account.LoadByUid(account.Uid, msgs, dc));
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// need to ask about sending emails and name parameters!
        /// </summary>
        [Fact]
        public void RequestAccessToAccountTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var webuser = Factories.WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var webuser2 = Factories.WebUserFactory.CreateWithEmail("tes2t@test.com", msgs, dc);
            var hostaccount = Factories.AccountFactory.Create(webuser, msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(webuser2, msgs, dc);

            Assert.True(visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc: dc).HasNoErrors());
        }


        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact]
        public void ApproveAccessToAccountTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var hostUser = Factories.WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var visitorUser = Factories.WebUserFactory.CreateWithEmail("test2@test.com", msgs, dc);
            var hostaccount = Factories.AccountFactory.Create(hostUser, msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(visitorUser, msgs, dc);
            msgs.AddRange(visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc));
            msgs.AddRange(hostaccount.ApproveAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the approved visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact]
        public void GetApprovedVisitorAccountsTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc);
            var hostaccount = Factories.AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(visitorUser, msgs, dc);
            
            visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc: dc);
            hostaccount.ApproveAccessToAccount(visitoraccount, loggedInUser, dc: dc);

            Assert.Single(hostaccount.GetApprovedVisitorAccounts(dc: dc));
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the requested visitors for the host
        /// </summary>

        [Fact]
        public void GetRequestedVisitorAccountsTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc);
            var hostaccount = Factories.AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(visitorUser, msgs, dc);

            visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc: dc);

            Assert.Single(hostaccount.GetRequestedVisitorAccounts(dc: dc));
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact]
        public void DenyAccessToAccountTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.CreateWithEmail("test@test", msgs, dc);
            var visitorUser = Factories.WebUserFactory.CreateWithEmail("test2@test", msgs, dc);
            var hostaccount = Factories.AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(visitorUser, msgs, dc);

            msgs.AddRange(visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc));
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, deny it. Then list off the denied visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact]
        public void GetDeniedVisitorAccountsTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc);
            var hostaccount = Factories.AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(visitorUser, msgs, dc);

            visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc);
            hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc);

            Assert.Single(hostaccount.GetDeniedVisitorAccounts(dc));
        }

    }
}
