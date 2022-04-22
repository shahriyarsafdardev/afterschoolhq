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
using AfterSchoolLibrary.Services;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Tests
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
        public void LoadByParticipationObjectTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(msgs, dc: dc);
            RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            Assert.Single(RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc));
        }

        /// <summary>
        /// Test for getting the total seat count of a RegistrantActivity by and Activity Id
        /// </summary>
        [Fact]
        public void GetSeatCountByParticipationObjectTest()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc: dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc: dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc: dc);
            var registrantActivity = Factories.RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            var seatCount = RegistrantActivity.GetSeatCountByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc);

            Assert.Equal(1, seatCount);
        }

        /// <summary>
        /// Test for getting the seat count of a RegistrantActivity by the registration period id
        /// </summary>
        [Fact]
        public void GetSeatCountByRegistrationPeriodIdTest()
        {
            var user = WebUserFactory.CreateWithEmail("test", msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, registrationPeriod, msgs, dc);
            registrationPeriod.AddActivity(activity, true, user.ID.Value, dc);
            
            Assert.Equal(1, RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrationPeriod.ID.Value, msgs, dc));
        }
        /// <summary>
        /// Test for retrieving the total number of seats for an  Registrant Activity
        /// </summary>
        [Fact(DisplayName = "GetSeatCountByActivityGroupIdTest")]
        public void GetSeatCountByActivityGroupIdTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);            
            var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, registrationPeriod, msgs, dc);
            
            Assert.Equal(1, RegistrantActivity.GetSeatCountByRegistrationPeriodId(registrantActivity.RegistrationPeriodId.Value, msgs, dc));
        }

        /// <summary>
        /// Test loading RegistrantActivities (list) by registration ids (list) from RegistrantActivity Table
        /// </summary>
        [Fact(DisplayName = "LoadByRegistrationIdsTest")]
        public void LoadByRegistrationIdsTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant1 = RegistrantFactory.Create(msgs, dc);
            var registrant2 = RegistrantFactory.Create(msgs, dc);
            var registration1 = RegistrationFactory.Create(account, registrant1, msgs, dc);
            var registration2 = RegistrationFactory.Create(account, registrant2, msgs, dc);
            var registrantActivity1 = RegistrantActivityFactory.Create(activity, registrant1, registration1, msgs, dc);
            var registrantActivity2 = RegistrantActivityFactory.Create(activity, registrant2, registration2, msgs, dc);
      
            List<int> registrationIds = new List<int>();
            registrationIds.Add(registration1.ID.Value);
            registrationIds.Add(registration2.ID.Value);

            Assert.Equal(2, RegistrantActivity.LoadByRegistrationIds(registrationIds, msgs, dc).Count);
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
                registrantActivities.Select(ra => ra.ID.Value).ToStrings().ToString(),
                RegistrantActivity.LoadByRegistrantId(registrant.ID.Value, dc: dc).Select(ra => ra.ID.Value).ToStrings().ToString()
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

            registrantActivity.CancelRegistration(msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

    }
}
