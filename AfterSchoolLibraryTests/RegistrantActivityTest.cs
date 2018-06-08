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
    public class RegistrantActivityTest : BaseTest
    {
        public RegistrantActivityTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ToRegistrantTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(registrantActivity.ToRegistrant());
        }

        [Fact]
        public void ToActivitydbTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(registrantActivity.ToActivity(false));
        }

        [Fact]
        public void ToActivityTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(registrantActivity.ToActivity(true,dc));
        }

        [Fact]
        public void LoadByActivityIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(RegistrantActivity.LoadByActivityId(activity.ID.Value,dc));
        }

        [Fact]
        public void GetSeatCountByActivityIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(RegistrantActivity.GetSeatCountByActivityId(activity.ID.Value,dc));
        }

        [Fact]
        public void GetSeatCountByAccountIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(RegistrantActivity.LoadByAccountId(account.ID.Value,null,null,dc));
        }

        [Fact]
        public void GetSeatCountByRegistrationPeriodIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            registrantActivity.NumberOfSeats = 12;
            registrantActivity.NumberOfWaitingListSeats = 1;
            registrantActivity.NumberOfApplicationSeats = 1;
            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotEqual(0,RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrantActivity.RegistrationPeriodId.Value,dc));
        }

        [Fact]
        public void GetSeatCountByActivityGroupIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            //var activityGroup = 
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity = registrantActivity.Reload(dc);

            Assert.NotNull(RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrantActivity.RegistrationPeriodId.Value, dc));
        }

        [Fact]
        public void LoadByRegistrationIdsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            List<int> list = new List<int>();
            list.Add(registrantActivity.RegistrationId.Value);

            Assert.NotEqual(0,RegistrantActivity.LoadByRegistrationIds(list,dc).Count);
        }

        [Fact]
        public void LoadByRegistrantIdTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            Assert.NotEqual(0, RegistrantActivity.LoadByRegistrantId(registrant.ID.Value,dc).Count);
        }

        [Fact]
        public void GetByActivityIdsTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            activity.StartDateTime = new DateTime(0, 0, 0, 0, 0, 0);
            var s = activity.StartDateTime.Value;

            List<int> list = new List<int>();
            list.Add(registrant.ID.Value);

            List<int> list2 = new List<int>();
            list.Add(activity.ID.Value);

            Assert.NotNull(RegistrantActivity.GetByActivityIds(list2,list,s,dc));
        }

        [Fact]
        public void GetRegistrationActivityTest()
        {
            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            Assert.NotNull( registrantActivity.GetRegistrationActivity(dc));
        }

        [Fact]
        public void DeleteRegistrationActivityTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity.DeleteRegistrationActivity(dc);

            Assert.True(RegistrantActivity.LoadByActivityId(registrantActivity.ID.Value,dc).IsEmpty());
        }

        [Fact]
        public void CancelRegistrationTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity.DeleteRegistrationActivity(dc);

            Assert.NotNull(registrantActivity.CancelRegistration(dc));
        }


        //dependant on ToActivity
        [Fact]
        public void SendNewRegistrationNoticeTest()
        {

            var account = Factories.AccountFactory.Create(msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            registrantActivity.DeleteRegistrationActivity(dc);

            Assert.NotNull(registrantActivity.SendNewRegistrationNotice(account,activity,dc));
        }

    }
}
