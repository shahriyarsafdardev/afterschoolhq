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
    public class RegistrantTest: BaseTest
    {
        public RegistrantTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var a = new Registrant { };
            a.Save(dc);
            var id = a.ID.Value;
            Assert.NotNull(Registrant.Load(id, dc: dc));
        }

        [Fact]
        public void LoadLongTest()
        {
            var a = new Registrant() { };
            a.Save(dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Account.Load(c, 0, c.Count, dc: dc));
        }


        [Fact]
        public void GetCountTest()
        {
            var a = new Registrant() { };
            a.Save(dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Activity.GetCount(c, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var a = new Registrant() { };
            Assert.NotNull(a.Save(dc));
        }

        [Fact]
        public void DeleteTest()
        {
            var a = new Registrant() { };
            a.Save(dc);
            var id = a.ID.Value;
            a.Delete(dc);
            Assert.Null(Activity.Load(id, dc: dc));
        }

        [Fact]
        public void CreateTest()
        {
            Assert.NotNull(Registrant.Create(dc));
        }

        [Fact]
        public void AlterTest()
        {
            Assert.NotNull(Registrant.Alter(dc));
        }


        [Fact]
        public void ReloadTest()
        {
            var a = new Registrant() { };
            a.Save(dc);
            Assert.NotNull(a.Reload(dc));
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var a = new Registrant() { };
            a.Save(dc);
            var id = a.ID.Value;
            Assert.NotNull(Activity.LoadByAccountId(id, dc: dc));
        }

        [Fact]
        public void LoadByParentRegistrantIdTest()
        {
            var a = new Registrant() { ParentRegistrantId =12};
            a.Save(dc);
            Assert.NotNull(Registrant.LoadByParentRegistrantId(12,dc));
        }

        [Fact]
        public void LoadByAccountIdByRegistrantTypeIdTest()
        {
            var a = new Registrant() { ParentRegistrantId = 12 };
            a.Save(dc);
            var id = a.ID.Value;


            Assert.NotNull(Registrant.LoadByAccountIdByRegistrantTypeId(id,RegistrantType.Individual, dc: dc));
        }

        [Fact]
        public void LoadByAccountIdByIsContactOnlyTest()
        {
            var a = new Registrant() { IsContactOnly = true };
            a.Save(dc);
            var id = a.ID.Value;

            Assert.NotNull(Registrant.LoadByAccountIdByIsContactOnly(id,true,dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var g = new Guid() { };

            var a = new Registrant() { Uid = g };
            a.Save(dc);
            var id = a.ID.Value;

            Assert.NotNull(Registrant.LoadByUid(g,dc));
        }

        [Fact]
        public void LoadByWebUserIdTest()
        {
            var u = new WebUser() { };
            u.Save(dc);
            var wid = u.ID.Value;

            var a = new Registrant() { WebUserId = wid };
            a.Save(dc);
            var id = a.ID.Value;

            Assert.NotNull(Registrant.LoadByWebUserId(wid, dc: dc));
        }

        [Fact]
        public void LoadByWebUserIdByIsMasterTest()
        {
            var u = new WebUser() { };
            u.Save(dc);
            var wid = u.ID.Value;

            var a = new Registrant() {WebUserId = wid ,IsMaster= true};
            a.Save(dc);
            var id = a.ID.Value;

            Assert.NotNull(Registrant.LoadByWebUserIdByIsMaster(wid,true,dc));
        }

        [Fact]
        public void LoadByFirstRegistrantIdTest()
        {
            var a = new Registrant() {  FirstRegistrantId =12 };
            a.Save(dc);
            var id = a.FirstRegistrantId.Value;

            Assert.NotNull(Registrant.LoadByFirstRegistrantId(id, dc: dc));
        }


    }

}
