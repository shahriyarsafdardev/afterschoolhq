using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class RegistrantActivityTest : IDisposable
    {
        DatabaseConnection dc = null;
        SystemMessages msgs = null;

        public RegistrantActivityTest()
        {
            AfterSchoolLibrary.Utilities.InitializeApp();
            dc = new DatabaseConnection(Database.Credentials.ConnectionString);
            msgs = new SystemMessages();
            dc.Open();
            dc.BeginTransaction();
        }

        public void Dispose()
        {
            dc.RollbackTransaction();
            dc.Close();
            dc.Dispose();
        }

        [Fact]
        public void ToRegistrantTest()
        {
            var a = new RegistrantActivity { RegistrantId=1 };
            a.Save(dc);
            Assert.NotNull(a.ToRegistrant());
        }

        [Fact]
        public void ToActivitydbTest()
        {
            var account = new Account();
            msgs.AddRange(account.Save(dc));

            var a = new Activity() { AccountId = account.ID };
            msgs.AddRange(a.Save(dc));

            var reg = new Registrant() { };
            msgs.AddRange(reg.Save(dc));

            var ra = new RegistrantActivity { RegistrantId = reg.ID, ActivityId = a.ID };
            msgs.AddRange(ra.Save(dc));

            ra = ra.Reload(dc);

            Assert.NotNull(ra.ToActivity(false));
        }

        [Fact]
        public void ToActivityTest()
        {
            var a = new RegistrantActivity { RegistrantId = 1 };
            a.Save(dc);
            Assert.NotNull(a.ToActivity(false,dc));
        }

        [Fact]
        public void LoadByActivityIdTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity { RegistrantId = 1 };
            a.Save(dc);

            Assert.NotNull(RegistrantActivity.LoadByActivityId(actid,dc));
        }

        [Fact]
        public void GetSeatCountByActivityIdTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity { };
            a.Save(dc);

            Assert.NotNull(RegistrantActivity.GetSeatCountByActivityId(actid, dc));
        }

        [Fact]
        public void GetSeatCountByAccountIdTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity { RegistrantId = 1 };
            a.Save(dc);
            var aid = a.ID.Value;

            Assert.NotNull(RegistrantActivity.GetSeatCountByAccountId(aid,dc));
        }

        [Fact]
        public void GetSeatCountByRegistrationPeriodIdTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity {RegistrationPeriodId =12 };
            a.Save(dc);
            var aid = a.ID.Value;

            Assert.NotNull(RegistrantActivity.GetSeatCountByRegistrationPeriodId(12, dc));
        }

        [Fact]
        public void GetSeatCountByActivityGroupIdTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity { ActivityGroupId = 12 };
            a.Save(dc);
            var aid = a.ID.Value;

            Assert.NotNull(RegistrantActivity.GetSeatCountByActivityGroupId(12, dc));
        }

        [Fact]
        public void LoadByRegistrationIdsTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity {RegistrationId =12};
            a.Save(dc);
            var aid = a.RegistrationId.Value;

            List<int> iList = new List<int>();
            iList.Add(2);
            iList.Add(aid);

            Assert.NotNull(RegistrantActivity.LoadByRegistrationIds(iList, dc));
        }

        [Fact]
        public void LoadByRegistrantIdTest()
        {
            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity { RegistrationId = 12 };
            a.Save(dc);
            var aid = a.RegistrationId.Value;

            Assert.NotNull(RegistrantActivity.LoadByRegistrantId(aid,dc));
        }

        [Fact]
        public void GetByActivityIdsTest()
        {
            var start = new DateTime(1,1,1,1,1,1) { };

            var act = new Activity() {StartDateTime = start };
            act.Save(dc);
            var actid = act.ID.Value;

            var a = new RegistrantActivity { RegistrationId = 12, ActivityId = actid };
            a.Save(dc);
            var aid = a.RegistrationId.Value;

            List<int> aList = new List<int>();
            aList.Add(2);
            aList.Add(aid);

            List<int> rList = new List<int>();
            rList.Add(2);
            rList.Add(aid);

            Assert.NotNull(RegistrantActivity.GetByActivityIds(aList,rList,start,dc));
        }

        [Fact]
        public void GetRegistrationActivityTest()
        {

            var act = new Activity() {};
            act.Save(dc);


            var r = new Registration() {  };
            r.Save(dc);

            var ra = new RegistrationActivity() {ActivityId = act.ID, RegistrationId = r.ID};
            ra.Save(dc);

            var a = new RegistrantActivity { RegistrationId = r.ID, ActivityId = act.ID };
            a.Save(dc);

            Assert.NotNull(a.GetRegistrationActivity(dc));
        }

        [Fact]
        public void DeleteRegistrationActivityTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var r = new Registration() { };
            r.Save(dc);
            var rid = r.ID.Value;

            var a = new RegistrantActivity { RegistrationId = rid, ActivityId = actid };
            a.Save(dc);
            var aid = a.RegistrationId.Value;


            Assert.NotNull(a.DeleteRegistrationActivity(dc));
        }

        [Fact]
        public void CancelRegistrationTest()
        {

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var r = new Registration() { };
            r.Save(dc);
            var rid = r.ID.Value;

            var a = new RegistrantActivity { RegistrationId = rid, ActivityId = actid };
            a.Save(dc);
            var aid = a.RegistrationId.Value;


            Assert.NotNull(a.CancelRegistration(dc));
        }


        //dependant on ToActivity
        [Fact]
        public void SendNewRegistrationNoticeTest()
        {

            var accnt = new Account() { };
            accnt.Save(dc);

            var act = new Activity() { };
            act.Save(dc);
            var actid = act.ID.Value;

            var r = new Registration() { };
            r.Save(dc);
            var rid = r.ID.Value;

            var a = new RegistrantActivity { RegistrationId = rid, ActivityId = actid };
            a.Save(dc);
            var aid = a.RegistrationId.Value;


            Assert.NotNull(a.SendNewRegistrationNotice(null,null,dc));
        }

    }
}
