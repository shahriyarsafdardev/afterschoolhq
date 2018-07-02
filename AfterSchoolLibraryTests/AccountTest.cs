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

        [Fact]
        public void LoadLongTest()
        {
            Assert.InRange(Account.Load(null, 0, 10, msgs, dc).Count, 0, 10);
        }

        //Gets the Number of accounts in the DB
        [Fact]
        public void GetCountTest()
        {
            var startCount = Account.GetCount(msgs: msgs, dc: dc);
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);

            Assert.Equal(startCount + 1, Account.GetCount(msgs: msgs, dc: dc));
        }

        //saves the account to the DB
        [Fact]
        public void SaveTest()
        {
            var a = new Account() { };
            Assert.NotNull(a.Save(dc: dc));
        }
        
        //removes the account from the DB
        [Fact]
        public void DeleteTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            Assert.NotNull(account.Delete(dc));
        }

        //Creates and returns a new Account
        [Fact]
        public void CreateTest()
        {
            Assert.NotNull(Account.Create(dc: dc));
        }

        [Fact]
        public void AlterTest()
        {
            Assert.NotNull(Account.Alter(dc: dc));
        }

        //loads the account using its own ID
        [Fact]
        public void ReloadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            Assert.NotNull(account.Reload(msgs, dc));
        }

        [Fact]
        public void LoadByOwnerIdTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            Assert.NotNull(Account.LoadByOwnerId(accountOwner.ID.Value, msgs, dc));
        }

        [Fact]
        public void LoadByUniqueNameTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            account.UniqueName = AccountService.GenerateUniqueName(account.Name, account.ID, dc);
            msgs.AddRange(account.Save(dc));
            Assert.NotNull(Account.LoadByUniqueName(account.UniqueName, msgs, dc));
        }

        /// <summary>
        /// should already have one in the database
        /// </summary>
        [Fact]
        public void LoadByIsAdminTest()
        {
            Assert.NotNull(Account.LoadByIsAdminAccount(true, msgs, dc));
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
