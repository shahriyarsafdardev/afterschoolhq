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
using AfterSchoolLibraryTests.Factories;

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
            a.Save(dc: dc);
            var id = a.ID.Value;
            Assert.NotNull(Registrant.Load(id, dc: dc));
        }

        [Fact]
        public void LoadLongTest()
        {
            var a = new Registrant() { };
            a.Save(dc: dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Account.Load(c, 0, c.Count, dc: dc));
        }


        [Fact]
        public void GetCountTest()
        {
            var a = new Registrant() { };
            a.Save(dc: dc);
            var id = a.ID.Value;
            var c = new System.Collections.Hashtable();
            Assert.NotNull(Activity.GetCount(c, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var a = new Registrant() { };
            Assert.NotNull(a.Save(dc: dc));
        }

        [Fact]
        public void DeleteTest()
        {
            var a = new Registrant() { };
            a.Save(dc: dc);
            var id = a.ID.Value;
            a.Delete(dc: dc);
            Assert.Null(Activity.Load(id, dc: dc));
        }

        [Fact]
        public void CreateTest()
        {
            Assert.NotNull(Registrant.Create(dc: dc));
        }

        [Fact]
        public void AlterTest()
        {
            Assert.NotNull(Registrant.Alter(dc: dc));
        }


        [Fact]
        public void ReloadTest()
        {
            var a = new Registrant() { };
            a.Save(dc: dc);
            Assert.NotNull(a.Reload(dc: dc));
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var a = new Registrant() { };
            a.Save(dc: dc);
            var id = a.ID.Value;
            Assert.NotNull(Activity.LoadByAccountId(id, dc: dc));
        }

        [Fact]
        public void LoadByParentRegistrantIdTest()
        {
            var a = new Registrant() { ParentRegistrantId =12};
            a.Save(dc: dc);
            Assert.NotNull(Registrant.LoadByParentRegistrantId(12, dc: dc));
        }

        [Fact]
        public void LoadByAccountIdByRegistrantTypeIdTest()
        {
            var a = new Registrant() { ParentRegistrantId = 12 };
            a.Save(dc: dc);
            var id = a.ID.Value;


            Assert.NotNull(Registrant.LoadByAccountIdByRegistrantTypeId(id,RegistrantType.Individual, dc: dc));
        }

        [Fact]
        public void LoadByAccountIdByIsContactOnlyTest()
        {
            var a = new Registrant() { IsContactOnly = true };
            a.Save(dc: dc);
            var id = a.ID.Value;

            Assert.NotNull(Registrant.LoadByAccountIdByIsContactOnly(id,true, dc: dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var g = new Guid() { };

            var a = new Registrant() { Uid = g };
            a.Save(dc: dc);
            var id = a.ID.Value;

            Assert.NotNull(Registrant.LoadByUid(g, dc: dc));
        }

        [Fact(DisplayName = "Loads registrant by web user id")]
        public void LoadByWebUserIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var registrant = RegistrantFactory.Create(user, true, msgs, dc);

            Assert.NotNull(Registrant.LoadByWebUserId(user.ID.Value, msgs, dc));
        }

        [Theory(DisplayName = "Loads registrant by user id by is master")]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadByWebUserIdByIsMasterTest(bool isMaster)
        {
            var user = WebUserFactory.Create(msgs, dc);
            var registrant = RegistrantFactory.Create(user, isMaster, msgs, dc);

            Assert.NotNull(Registrant.LoadByWebUserIdByIsMaster(user.ID.Value, isMaster, msgs, dc));
        }

        [Fact(Skip = "Need to finish")]
        public void LoadByFirstRegistrantIdTest()
        {
            var a = new Registrant() {  FirstRegistrantId =12 };
            a.Save(dc: dc);
            var id = a.FirstRegistrantId.Value;

            Assert.NotNull(Registrant.LoadByFirstRegistrantId(id, dc: dc));
        }


    }

}
