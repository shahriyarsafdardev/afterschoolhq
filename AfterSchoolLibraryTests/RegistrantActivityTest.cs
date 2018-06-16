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
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.NotNull(registrantActivity.ToRegistrant());
        }

        [Fact]
        public void ToActivitydbTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.NotNull(registrantActivity.ToActivity(false));
        }

        [Fact]
        public void ToActivityTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.NotNull(registrantActivity.ToActivity(true, dc: dc));
        }

        [Fact]
        public void LoadByActivityIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Single(RegistrantActivity.LoadByActivityId(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void GetSeatCountByActivityIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.NotNull(RegistrantActivity.GetSeatCountByActivityId(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void GetSeatCountByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Single(RegistrantActivity.LoadByAccountId(account.ID.Value,null,null, dc: dc));
        }

        [Fact]
        public void GetSeatCountByRegistrationPeriodIdTest()
        {
            var user = Factories.WebUserFactory.CreateWithEmail("test", msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            registrantActivity.NumberOfSeats = 12;
            registrantActivity.NumberOfWaitingListSeats = 1;
            registrantActivity.NumberOfApplicationSeats = 1;
            registrantActivity.Save(dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(msgs,dc);
            registrationPeriod.AddActivity(activity, true, user.ID.Value, dc);

            Assert.NotEqual(0,RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrationPeriod.ID.Value, dc: dc));
        }

        [Fact]
        public void GetSeatCountByActivityGroupIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            registrantActivity.NumberOfSeats = 12;
            registrantActivity.NumberOfWaitingListSeats = 1;
            registrantActivity.NumberOfApplicationSeats = 1;
            registrantActivity.Save(dc);
            Assert.NotEqual(0 ,RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrantActivity.RegistrationPeriodId.Value, dc: dc));
        }

        [Fact]
        public void LoadByRegistrationIdsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            var r = registrantActivity.RegistrationId.Value;

            List<int> list = new List<int>();
            list.Add(r);

            Assert.Single(RegistrantActivity.LoadByRegistrationIds(list, dc: dc));
        }

        [Fact]
        public void LoadByRegistrantIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.Single(RegistrantActivity.LoadByRegistrantId(registrant.ID.Value, dc: dc));
        }

        [Fact]
        public void GetByActivityIdsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            activity.StartDateTime = new DateTime(0, 0, 0, 0, 0, 0);
            var s = activity.StartDateTime.Value;

            List<int> list = new List<int>();
            list.Add(registrant.ID.Value);

            List<int> list2 = new List<int>();
            list.Add(activity.ID.Value);

            Assert.Single(RegistrantActivity.GetByActivityIds(list2,list,s, dc: dc));
        }

        [Fact]
        public void GetRegistrationActivityTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.NotNull(registrantActivity.GetRegistrationActivity(dc: dc));
        }

        [Fact]
        public void DeleteRegistrationActivityTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity.DeleteRegistrationActivity(dc: dc);

            Assert.True(RegistrantActivity.LoadByActivityId(registrantActivity.ID.Value, dc: dc).IsEmpty());
        }

        [Fact]
        public void CancelRegistrationTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            var registration = Factories.RegistrationFactory.Create(account, registrant, msgs, dc: dc);
            var registrationActivity = Factories.RegistrationActivityFactory.Create(activity, registration, msgs, dc: dc);

            Assert.True(registrantActivity.CancelRegistration(dc: dc).HasNoErrors());
        }


        //dependant on ToActivity
        [Fact]
        public void SendNewRegistrationNoticeTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity.DeleteRegistrationActivity(dc: dc);

            Assert.True(registrantActivity.SendNewRegistrationNotice(account,activity, dc: dc).HasNoErrors());
        }

    }
}
