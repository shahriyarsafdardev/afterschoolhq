using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests
{
    public class AccountTest : BaseTest
    {
        public AccountTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(DisplayName = "Load 1 account")]
        public void LoadTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            Assert.NotNull(Account.Load(account.ID.Value, msgs, dc));
        }

        [Theory(DisplayName = "Load accounts with paging")]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            var originalCount = Account.GetCount(msgs: msgs, dc: dc);
            for( var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate + originalCount), 
                Account.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        //Gets the Number of accounts in the DB
        [Theory(DisplayName = "Get # of accounts in db")]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            var originalCount = Account.GetCount(msgs: msgs, dc: dc);
            for (var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);
            }

            Assert.Equal(originalCount + numberToCreate, Account.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the account to the DB
        /// </summary>
        [Fact(DisplayName = "Save account")]
        public void SaveTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            account.AccountTypeId = AccountType.ActivityProvider;
            msgs.AddRange(account.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the account from the DB
        /// </summary>
        [Fact(DisplayName = "Delete an account")]
        public void DeleteTest()
        {
            var originalCount = Account.GetCount(msgs: msgs, dc: dc);
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            msgs.AddRange(account.Delete(dc));

            Assert.Equal(originalCount, Account.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the account using its own ID
        /// </summary>
        [Fact(DisplayName = "Reload account")]
        public void ReloadTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            Assert.Equal(account.ID, account.Reload(msgs, dc).ID);
        }

        [Fact(DisplayName = "Load account by Owner")]
        public void LoadByOwnerIdTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            Assert.Single(Account.LoadByOwnerId(accountOwner.ID.Value, msgs, dc));
        }

        [Fact(DisplayName = "Load account by unique name")]
        public void LoadByUniqueNameTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            account.UniqueName = AccountService.GenerateUniqueName(account.Name, account.ID, msgs, dc);
            msgs.AddRange(account.Save(dc));

            Assert.Equal(account.ID, Account.LoadByUniqueName(account.UniqueName, msgs, dc).ID);
        }

        /// <summary>
        /// testing for admin account which is changing to use AccountType in the future
        /// </summary>
        [Theory(DisplayName = "Load accounts by account type")]
        [InlineData(AccountType.ActivityProvider)]
        [InlineData(AccountType.School)]
        public void LoadByAccountTypeId(AccountType accountType)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, accountType, msgs, dc);

            Assert.NotNull(Account.LoadByAccountTypeId(accountType, msgs, dc));
        }

        [Fact(DisplayName = "Load account by UID")]
        public void LoadByUidTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            Assert.NotNull(Account.LoadByUid(account.Uid, msgs, dc));
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// need to ask about sending emails and name parameters!
        /// </summary>
        [Fact(DisplayName = "Request access to a host account")]
        public void RequestAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var webuser = WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var webuser2 = WebUserFactory.CreateWithEmail("tes2t@test.com", msgs, dc);
            var hostaccount = AccountFactory.Create(webuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(webuser2, msgs, dc);
            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }


        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact(DisplayName = "Approve Provider Access to a Host Account")]
        public void ApproveAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostUser = WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var visitorUser = WebUserFactory.CreateWithEmail("test2@test.com", msgs, dc);
            var hostaccount = AccountFactory.Create(hostUser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);
            var relationship1 = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
            var relationship2 = hostaccount.ApproveAccessToAccount(visitoraccount, loggedInUser, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the approved visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact(Skip = "Rewrite")]
        public void GetApprovedVisitorAccountsTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.Create(true, msgs, dc);
            var visitorUser = WebUserFactory.Create(true, msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);
            
            visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc: dc);
            hostaccount.ApproveAccessToAccount(visitoraccount, loggedInUser, dc: dc);

            // Assert.Single(hostaccount.GetApprovedVisitorAccounts(dc: dc));
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the requested visitors for the host
        /// </summary>

        [Fact(Skip = "Rewrite")]
        public void GetRequestedVisitorAccountsTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.Create(true, msgs, dc);
            var visitorUser = WebUserFactory.Create(true, msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, dc: dc);

            //Assert.Single(hostaccount.GetRequestedVisitorAccounts(dc: dc));
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact(DisplayName = "Host account denies request to visitor account")]
        public void DenyAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.CreateWithEmail("test@test", msgs, dc);
            var visitorUser = WebUserFactory.CreateWithEmail("test2@test", msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, deny it. Then list off the denied visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact(DisplayName = "Host account gets list of denied visitor accounts")]
        public void GetDeniedVisitorAccountsTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.Create(true, msgs, dc);
            var visitorUser = WebUserFactory.Create(true, msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.Single(hostaccount.GetDeniedVisitorAccounts(visitoraccount.AccountTypeId, msgs, dc));
        }

        [Fact(DisplayName = "Get or create default form for account")]
        public void GetOrCreateDefaultForm()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var form = account.GetOrCreateDefaultForm(accountOwner, msgs, dc);

            Assert.NotNull(form);
            
        }

        [Theory(DisplayName = "Get leagues that a school has been invited to")]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void GetExternalLeaguesTest(int numberToCreate)
        {
            var leagueAccountOwner = WebUserFactory.Create(msgs, dc);
            var leagueAccount = AccountFactory.Create(leagueAccountOwner, msgs, dc);

            var invitedAccountOwner = WebUserFactory.Create(msgs, dc);
            var invitedAccount = AccountFactory.Create(invitedAccountOwner, msgs, dc);

            for (var i = 0; i < numberToCreate; i++)
            {
                var activityGroup = ActivityGroupFactory.Create(leagueAccount, ActivityGroupType.League, msgs, dc);
                msgs.AddRange(activityGroup.InviteAccount(invitedAccount, leagueAccountOwner, dc));
            }

            Assert.Equal(numberToCreate, invitedAccount.GetExternalLeagueCount(msgs, dc));

        }

        [Theory(DisplayName = "Load accounts by list of account type ids")]
        [InlineData(AccountType.School)]
        public void LoadByAccountTypeIdsTest(AccountType accountType)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, accountType, msgs, dc);
            var accountTypes = new List<AccountType>()
            {
                AccountType.ActivityProvider,
                AccountType.Registrant,
                AccountType.School,
                AccountType.SchoolDistrict
            };

            Assert.Single(Account.LoadByAccountTypeIds(accountTypes, null, null, msgs, dc));

        }

    }
}
