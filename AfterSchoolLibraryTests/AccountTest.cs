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

    }
}
