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
        public void ToActivityNoDBTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Equal(registrantActivity.ParticipationObjectId, registrantActivity.GetParticipationObject(msgs, dc).ObjectID);
        }

        [Fact]
        public void ToActivityDBTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            registrantActivity = registrantActivity.Reload(dc: dc);

            Assert.Equal(registrantActivity.ParticipationObjectId, registrantActivity.GetParticipationObject(msgs, dc).ObjectID);
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

            registrantActivity = registrantActivity.Reload(msgs, dc);

            var list = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, dc: dc);

            Assert.Single(list);
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

            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, registrationPeriodID, msgs, dc);
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
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, registrationPeriodID, msgs, dc);
            
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
        //[Fact(Skip = "Need to fix")]
        //public void GetByActivityIdsTest()
        //{
        //    var user = Factories.WebUserFactory.Create(msgs, dc);
        //    var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
        //    var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
        //    var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc: dc);
        //    var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
        //    var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
        //    var instance = ActivityInstanceFactory.Create(activity, schedule, DateTime.UtcNow.AddWeeks(-1), DateTime.UtcNow.AddWeeks(1), msgs, dc);

        //    var activityIds = new List<int>
        //    {
        //        activity.ID.Value
        //    };

        //    var registrantIds = new List<int>
        //    {
        //        registrant.ID.Value
        //    };

        //    Assert.Single(RegistrantActivity.GetByActivityIds(activityIds, registrantIds, DateTime.UtcNow, dc: dc));
        //}             

        [Fact]
        public void CancelRegistrationTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc);
            var registration = Factories.RegistrationFactory.Create(account, registrant, msgs, dc);

            Assert.True(registrantActivity.CancelRegistration(msgs, dc).HasNoErrors());
        }


        //dependant on ToActivity
        [Fact(Skip = "Need to fix")]
        public void SendNewRegistrationNoticeTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);
            var registration = Factories.RegistrationFactory.Create(account, registrant, msgs, dc);

            Assert.True(registrantActivity.SendNewRegistrationNotice(activity.ToParticipationObject(msgs, dc), dc).HasNoErrors());
        }

    }
}
