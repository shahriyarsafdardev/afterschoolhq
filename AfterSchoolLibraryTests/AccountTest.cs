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
            a.Save(dc: dc);
            var id = a.ID.Value;
            Assert.NotNull(Account.Load(id, dc: dc));
        }

        [Fact]
        public void LoadLongTest()
        {
            var a = new Account() { };
            a.Save(dc: dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Account.Load(c, 0, c.Count, dc: dc));
        }

        [Fact]
        public void GetCountTest()
        {
            var a = new Account() { };
            a.Save(dc: dc);
            var id = a.ID.Value;

            Assert.Equal(1,Account.GetCount(null, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var a = new Account() { };
            Assert.NotNull(a.Save(dc: dc));
        }

        [Fact]
        public void DeleteTest()
        {
            var a = new Account() {OwnerId = 12 };
            a.Save(dc: dc);
            var id = a.OwnerId.Value;

            Assert.NotNull(a.Delete(dc: dc));
            
        }

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


        [Fact]
        public void ReloadTest()
        {
            var a = new Account() { };
            a.Save(dc: dc);
            Assert.NotNull(a.Reload(dc: dc));
        }

        [Fact]
        public void LoadByOwnerIdTest()
        {
            var a = new Account() { OwnerId = 12 };
            a.Save(dc: dc);
            Assert.NotNull(Account.LoadByOwnerId(12, dc: dc));
        }

        [Fact]
        public void LoadByUniqueNameTest()
        {
            var a = new Account() { UniqueName = "test"};
            a.Save(dc: dc);
            Assert.NotNull(Account.LoadByUniqueName("test", dc: dc));
        }

        [Fact]
        public void LoadByIsAdminTest()
        {
            var a = new Account() { IsAdminAccount = true };
            a.Save(dc: dc);
            Assert.NotNull(Account.LoadByIsAdminAccount(true , dc: dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var u = new Guid() {};
            var a = new Account() {Uid = u };
            a.Save(dc: dc);
            Assert.NotNull(Account.LoadByUid(u , dc: dc));
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// need to ask about sending emails and name parameters!
        /// </summary>
        [Fact]
        public void RequestAccessToAccountTest()
        {
            var webuser = Factories.WebUserFactory.Create(msgs, dc: dc);
            webuser.Email = "test@test";
            webuser.Save(dc: dc);
            var webuser2 = Factories.WebUserFactory.Create(msgs, dc: dc);
            webuser2.Email = "test2@test";
            webuser2.Save(dc: dc);

            var hostaccount = Factories.AccountFactory.Create(msgs, dc: dc);
            hostaccount.OwnerId = webuser.ID.Value;
            hostaccount.Name = "test";
            hostaccount.Save(dc: dc);

            var visitoraccount = Factories.AccountFactory.Create(msgs, dc: dc);
            visitoraccount.OwnerId = webuser2.ID.Value;
            visitoraccount.Name = "test2";
            visitoraccount.Save(dc: dc);

            Assert.True(visitoraccount.RequestAccessToAccount(hostaccount, dc: dc).HasNoErrors());
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact]
        public void ApproveAccessToAccountTest()
        {
            var webuser = Factories.WebUserFactory.CreateWithEmail("test@test", msgs, dc: dc);
            var webuser2 = Factories.WebUserFactory.CreateWithEmail("test2@test", msgs, dc: dc);

            var hostaccount = Factories.AccountFactory.Create(msgs, dc: dc);
            hostaccount.OwnerId = webuser.ID.Value;
            hostaccount.Name = "test";
            msgs.AddRange(hostaccount.Save(dc: dc));

            var visitoraccount = Factories.AccountFactory.Create(msgs, dc: dc);
            visitoraccount.OwnerId = webuser2.ID.Value;
            visitoraccount.Name = "test2";
            msgs.AddRange(visitoraccount.Save(dc: dc));
            msgs.AddRange(visitoraccount.RequestAccessToAccount(hostaccount, dc: dc));

            msgs.AddRange(hostaccount.ApproveAccessToAccount(visitoraccount, dc: dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact]
        public void DenyAccessToAccountTest()
        {
            var webuser = Factories.WebUserFactory.Create(msgs, dc: dc);
            webuser.Email = "test@test";
            webuser.Save(dc: dc);
            var webuser2 = Factories.WebUserFactory.Create(msgs, dc: dc);
            webuser2.Email = "test2@test";
            webuser2.Save(dc: dc);

            var hostaccount = Factories.AccountFactory.Create(msgs, dc: dc);
            hostaccount.OwnerId = webuser.ID.Value;
            hostaccount.Name = "test";
            hostaccount.Save(dc: dc);

            var visitoraccount = Factories.AccountFactory.Create(msgs, dc: dc);
            visitoraccount.OwnerId = webuser2.ID.Value;
            visitoraccount.Name = "test2";
            visitoraccount.Save(dc: dc);
            visitoraccount.RequestAccessToAccount(hostaccount, dc: dc);

            Assert.True(visitoraccount.DenyAccessToAccount(visitoraccount, dc: dc).HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the approved visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact]
        public void GetApprovedVisitorAccountsTest()
        {
            var hostaccount = Factories.AccountFactory.Create(msgs, dc: dc);
            var visitoraccount = Factories.AccountFactory.Create(msgs, dc: dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            
            visitoraccount.RequestAccessToAccount(hostaccount, dc: dc);
            hostaccount.ApproveAccessToAccount(visitoraccount, dc: dc);

            Assert.Single(hostaccount.GetApprovedVisitorAccounts(dc: dc));
        }

        /// <summary>
        /// Will request access to an account, deny it. Then list off the denied visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact]
        public void GetDeniedVisitorAccountsTest()
        {
            var hostaccount = Factories.AccountFactory.Create(msgs, dc: dc);
            var visitoraccount = Factories.AccountFactory.Create(msgs, dc: dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc: dc);

            visitoraccount.RequestAccessToAccount(hostaccount, dc: dc);
            hostaccount.DenyAccessToAccount(visitoraccount, dc: dc);

            Assert.Single(hostaccount.GetDeniedVisitorAccounts(dc: dc));
        }

        /// <summary>
        /// Will request access to an account, approve it. Then list off the requested visitors for the host
        /// </summary>

        [Fact]
        public void GetRequestedVisitorAccountsTest()
        {
            var hostaccount = Factories.AccountFactory.Create(msgs, dc: dc);
            var visitoraccount = Factories.AccountFactory.Create(msgs, dc: dc);
            var hostuser = Factories.WebUserFactory.Create(true, msgs, dc: dc);
            var visitorUser = Factories.WebUserFactory.Create(true, msgs, dc: dc);

            visitoraccount.RequestAccessToAccount(hostaccount, dc: dc);

            Assert.Single(hostaccount.GetRequestedVisitorAccounts(dc: dc));
        }

    }
}
