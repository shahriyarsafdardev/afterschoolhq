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


        /// <summary>
        /// test for loading a RegistrantActivity by n Activity Id
        /// </summary>
        [Fact]
        public void LoadByActivityIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Equal(registrantActivity.ToString(), RegistrantActivity.LoadByActivityId(activity.ID.Value, dc: dc).ElementAt(0).ToString());
        }

        /// <summary>
        /// Test for getting the total seat count of a RegistrantActivity by and Activity Id
        /// </summary>
        [Fact]
        public void GetSeatCountByActivityIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Equal(registrantActivity.TotalSeats.ToString(), RegistrantActivity.LoadByAccountId(account.ID.Value, null, null, dc: dc).ElementAt(0).TotalSeats.ToString());
        }

        /// <summary>
        /// Test for getting the total seat count of RegistrantActivity by the account id,
        /// first load the registrant activity by account id and test the seat count
        /// </summary>
        [Fact]
        public void GetSeatCountByAccountIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Equal(registrantActivity.TotalSeats.ToString(), RegistrantActivity.LoadByAccountId(account.ID.Value,null,null, dc: dc).ElementAt(0).TotalSeats.ToString());
        }

        /// <summary>
        /// Test for getting the seat count of a RegistrantActivity by the registration period id
        /// </summary>
        [Fact]
        public void GetSeatCountByRegistrationPeriodIdTest()
        {
            var user = Factories.WebUserFactory.CreateWithEmail("test", msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, msgs, dc);

            int registrationPeriodID = registrationPeriod.ID.Value;
            int numberOfSeats = 12;
            int numberOfWaitingListSeats = 1;
            int numberOfApplicationSeats = 1;

            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, registrationPeriodID, numberOfSeats, numberOfWaitingListSeats, numberOfApplicationSeats, msgs, dc);
            registrationPeriod.AddActivity(activity, true, user.ID.Value, dc);

            // expect 13 seats, 12 seats + 1 waiting list seat
            Assert.Equal(13,RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrationPeriod.ID.Value, dc: dc));
        }
        /// <summary>
        /// Test for retrieving the total number of seats for an  Registrant Activity
        /// </summary>
        [Fact]
        public void GetSeatCountByActivityGroupIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrationPeriod = Factories.RegistrationPeriodFactory.Create(account, msgs, dc);

            int registrationPeriodID = registrationPeriod.ID.Value;
            int numberOfSeats = 12;
            int numberOfWaitingListSeats = 1;
            int numberOfApplicationSeats = 1;
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, registrationPeriodID, numberOfSeats, numberOfWaitingListSeats, numberOfApplicationSeats, msgs, dc);
            
            //expect 13 because we added a waiting list seat
            Assert.Equal(13 ,RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrantActivity.RegistrationPeriodId.Value, dc: dc));
        }
        /// <summary>
        /// Test loading RegistrantActivities (list) by registration ids (list) from RegistrantActivity Table
        /// </summary>
        [Fact]
        public void LoadByRegistrationIdsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant1 = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrant2 = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity1 = Factories.RegistrantActivityFactory.Create(activity, registrant1, msgs, dc: dc);
            var registrantActivity2 = Factories.RegistrantActivityFactory.Create(activity, registrant2, msgs, dc: dc);
            var registration1 = Factories.RegistrationFactory.Create(account,registrant1, msgs, dc);
            var registration2 = Factories.RegistrationFactory.Create(account, registrant2, msgs, dc);
   
   
            List<int> registrationIds = new List<int>();
            registrationIds.Add(registration1.ID.Value);
            registrationIds.Add(registration2.ID.Value);

            List<RegistrantActivity> registrantActivities = new List<RegistrantActivity>();
            registrantActivities.Add(registrantActivity1);
            registrantActivities.Add(registrantActivity2);

            Assert.Equal(
                registrantActivities.Select(ra => ra.ID.Value).ToArray().ToStringArray().ToString(), 
                RegistrantActivity.LoadByRegistrationIds(registrationIds, dc: dc).Select( ra => ra.ID.Value).ToArray().ToStringArray().ToString()

            );
        }
        /// <summary>
        /// Test loading RegistrantActivities (list) by a registrant id from RegistrantActivity Table
        /// </summary>
        [Fact]
        public void LoadByRegistrantIdTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity1 = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            var registrantActivity2 = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            List<RegistrantActivity> registrantActivities = new List<RegistrantActivity>();
            registrantActivities.Add(registrantActivity1);
            registrantActivities.Add(registrantActivity2);

            Assert.Equal(
                registrantActivities.Select(ra => ra.ID.Value).ToArray().ToStringArray().ToString(),
                RegistrantActivity.LoadByRegistrantId(registrant.ID.Value, dc: dc).Select(ra => ra.ID.Value).ToArray().ToStringArray().ToString()
            );
        }

        /// <summary>
        /// Test getting Registrant Activities (list) by ActivityIds (list) from RegistrantActivity Table
        /// </summary>
        [Fact]
        public void GetByActivityIdsTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            activity.StartDateTime = new DateTime(1990, 0, 0);
            var s = activity.StartDateTime.Value;

            List<int> list = new List<int>();
            list.Add(registrant.ID.Value);

            List<int> list2 = new List<int>();
            list.Add(activity.ID.Value);

            Assert.Single(RegistrantActivity.GetByActivityIds(list2,list,s, dc: dc));
        }

        /// <summary>
        /// Test for retrieving registrantActivity from DB
        /// Needs to be fixed
        /// </summary>
        [Fact]
        public void GetRegistrationActivityTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registration = Factories.RegistrationFactory.Create(account, registrant, msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, registration, msgs, dc: dc);

            Assert.Equal(registrantActivity.ToString(), registrantActivity.GetRegistrationActivity(dc: dc).ToString());
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
