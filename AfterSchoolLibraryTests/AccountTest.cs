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
            var a = new Account() { };
            a.Save(dc);
            var id = a.ID.Value;
            Assert.NotNull(Account.Load(id, dc));
        }

        [Fact]
        public void LoadLongTest()
        {
            var a = new Account() { };
            a.Save(dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Account.Load(c, 0,c.Count ,dc));
        }

        [Fact]
        public void GetCountTest()
        {
            var a = new Account() { };
            a.Save(dc);
            var id = a.ID.Value;

            Assert.NotNull(Account.GetCount(null,dc));
        }

        [Fact]
        public void SaveTest()
        {
            var a = new Account() { };
            Assert.NotNull(a.Save(dc));
        }

        [Fact]
        public void DeleteTest()
        {
            var a = new Account() {OwnerId = 12 };
            a.Save(dc);
            var id = a.OwnerId.Value;

            Assert.NotNull(a.Delete(dc));
            
        }

        [Fact]
        public void CreateTest()
        {
            Assert.NotNull(Account.Create(dc));
        }

        [Fact]
        public void AlterTest()
        {
            Assert.NotNull(Account.Alter(dc));
        }


        [Fact]
        public void ReloadTest()
        {
            var a = new Account() { };
            a.Save(dc);
            Assert.NotNull(a.Reload(dc));
        }

        [Fact]
        public void LoadByOwnerIdTest()
        {
            var a = new Account() { OwnerId = 12 };
            a.Save(dc);
            Assert.NotNull(Account.LoadByOwnerId(12, dc));
        }

        [Fact]
        public void LoadByUniqueNameTest()
        {
            var a = new Account() { UniqueName = "test"};
            a.Save(dc);
            Assert.NotNull(Account.LoadByUniqueName("test",dc));
        }

        [Fact]
        public void LoadByIsAdminTest()
        {
            var a = new Account() { IsAdminAccount = true };
            a.Save(dc);
            Assert.NotNull(Account.LoadByIsAdminAccount(true ,dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var u = new Guid() {};
            var a = new Account() {Uid = u };
            a.Save(dc);
            Assert.NotNull(Account.LoadByUid(u ,dc));
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// need to ask about sending emails and name parameters!
        /// </summary>
        [Fact]
        public void RequestAccessToAccountTest()
        {
            var webuser = Factories.WebUserFactory.Create(msgs, dc);
            webuser.Email = "test@test";
            webuser.Save(dc);
            var webuser2 = Factories.WebUserFactory.Create(msgs, dc);
            webuser2.Email = "test2@test";
            webuser2.Save(dc);

            var hostaccount = Factories.AccountFactory.Create(msgs, dc);
            hostaccount.OwnerId = webuser.ID.Value;
            hostaccount.Name = "test";
            hostaccount.Save(dc);

            var visitoraccount = Factories.AccountFactory.Create(msgs, dc);
            visitoraccount.OwnerId = webuser2.ID.Value;
            visitoraccount.Name = "test2";
            visitoraccount.Save(dc);

            Assert.True(visitoraccount.RequestAccessToAccount(hostaccount,dc).HasNoErrors());
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact]
        public void ApproveAccessToAccountTest()
        {
            var webuser = Factories.WebUserFactory.Create(msgs, dc);
            webuser.Email = "test@test";
            webuser.Save(dc);
            var webuser2 = Factories.WebUserFactory.Create(msgs, dc);
            webuser2.Email = "test2@test";
            webuser2.Save(dc);

            var hostaccount = Factories.AccountFactory.Create(msgs, dc);
            hostaccount.OwnerId = webuser.ID.Value;
            hostaccount.Name = "test";
            hostaccount.Save(dc);

            var visitoraccount = Factories.AccountFactory.Create(msgs, dc);
            visitoraccount.OwnerId = webuser2.ID.Value;
            visitoraccount.Name = "test2";
            visitoraccount.Save(dc);
            visitoraccount.RequestAccessToAccount(hostaccount, dc);

            Assert.True(hostaccount.ApproveAccessToAccount(visitoraccount, dc).HasNoErrors());
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact]
        public void DenyAccessToAccountTest()
        {
            var webuser = Factories.WebUserFactory.Create(msgs, dc);
            webuser.Email = "test@test";
            webuser.Save(dc);
            var webuser2 = Factories.WebUserFactory.Create(msgs, dc);
            webuser2.Email = "test2@test";
            webuser2.Save(dc);

            var hostaccount = Factories.AccountFactory.Create(msgs, dc);
            hostaccount.OwnerId = webuser.ID.Value;
            hostaccount.Name = "test";
            hostaccount.Save(dc);

            var visitoraccount = Factories.AccountFactory.Create(msgs, dc);
            visitoraccount.OwnerId = webuser2.ID.Value;
            visitoraccount.Name = "test2";
            visitoraccount.Save(dc);
            visitoraccount.RequestAccessToAccount(hostaccount, dc);

            Assert.True(visitoraccount.DenyAccessToAccount(visitoraccount, dc).HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the approved visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact]
        public void GetApprovedVisitorAccountsTest()
        {
            var hostaccount = Factories.AccountFactory.Create(msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc);
            
            visitoraccount.RequestAccessToAccount(hostaccount, dc);
            hostaccount.ApproveAccessToAccount(visitoraccount,dc);

            Assert.Single(hostaccount.GetApprovedVisitorAccounts(dc));
        }

        /// <summary>
        /// Will request access to an account, deny it. Then list off the denied visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact]
        public void GetDeniedVisitorAccountsTest()
        {
            var hostaccount = Factories.AccountFactory.Create(msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc);

            visitoraccount.RequestAccessToAccount(hostaccount, dc);
            hostaccount.DenyAccessToAccount(visitoraccount, dc);

            Assert.Single(hostaccount.GetDeniedVisitorAccounts(dc));
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the requested visitors for the host
        /// </summary>

        [Fact]
        public void GetRequestedVisitorAccountsTest()
        {
            var hostaccount = Factories.AccountFactory.Create(msgs, dc);
            var visitoraccount = Factories.AccountFactory.Create(msgs, dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc);

            visitoraccount.RequestAccessToAccount(hostaccount, dc);

            Assert.Single(hostaccount.GetRequestedVisitorAccounts(dc));
        }

    }
}
