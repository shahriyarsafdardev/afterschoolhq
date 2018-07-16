using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account ActivityOwnerAccount { get; set; }

        public ActivityServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact(DisplayName = "Payment plan is saved as an option on an activity")]
        public void AddPaymentPlanActivityObjectTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = activity.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

            Assert.Single(PaymentPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc));
        }

        [Fact(DisplayName = "Payment plan is saved as an option on an activity group")]
        public void AddPaymentPlanActivityGroupObjectTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var plan = activityGroup.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

            Assert.Single(PaymentPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, msgs, dc));
        }

        [Fact]
        public void ToParticipationObjectActivityTestFull()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var period = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var arp = ActivityRegistrationPeriodFactory.Create(activity, period, msgs, dc);

            Assert.IsType<ParticipationObject>(activity.ToParticipationObject(msgs, dc));
        }

        [Fact]
        public void ToParticipationObjectActivityTestSimple()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            Assert.IsType<ParticipationObject>(activity.ToParticipationObject(msgs, dc));
        }

        [Fact]
        public void GetPaymentPlansTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = PaymentPlanFactory.Create(ActivityOwnerAccount, activity, msgs, dc);

            Assert.Single(activity.GetPaymentPlans(msgs, dc));
        }

        [Fact]
        public void GetSubscriptionPlansTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = SubscriptionPlanFactory.Create(ActivityOwnerAccount, activity, msgs, dc);

            Assert.Single(activity.GetSubscriptionPlans(msgs, dc));
        }

        [Fact]
        public void CreateNewTest()
        {
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);
            var activity = ActivityService.CreateNew(
                activityName: "Test Activity",
                activityType: ActivityType.Activity,
                activityDetails: "<p>Here are some details</p>",
                seatsAvailable: 100,
                waitingListSeatsAvailable: 10,
                genderRestriction: GenderRestrictionType.None,
                contactName: "Test User",
                contactPhone: "123-123-1234",
                contactEmail: "dave@dave.com",
                costPerRegistrant: 12,
                costDescription: "Here is the main reason for the cost and what you get",
                acceptCreditCard: false,
                allowToPayLater: true,
                registrationMode: RegistrationMode.Register,
                accountId: ActivityOwnerAccount.ID,
                scheduleId: schedule.ID,
                locationId: location.ID,
                locationDetails: "<p>this is where instructions go</p>",
                activityGroupId: activityGroup.ID,
                minGradeLevelId: gradeLevels.First().ID,
                maxGradeLevelId: gradeLevels.Last().ID,
                loggedInUser: this.LoggedInUser,
                msgs: msgs,
                dc: dc
                );

            Assert.NotNull(activity.ID);
        }

        //[Fact]
        //public void UpdateTest()
        //{
        //    var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
        //    var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
        //    var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
        //    var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);
        //    var activity = ActivityService.CreateNew(
        //        activityName: "Test Activity",
        //        activityDetails: "<p>Here are some details</p>",
        //        seatsAvailable: 100,
        //        waitingListSeatsAvailable: 10,
        //        contactName: "Test User",
        //        contactPhone: "123-123-1234",
        //        contactEmail: "dave@dave.com",
        //        costPerRegistrant: 12,
        //        costDescription: "Here is the main reason for the cost and what you get",
        //        acceptCreditCard: false,
        //        allowToPayLater: true,
        //        registrationMode: RegistrationMode.Register,
        //        accountId: ActivityOwnerAccount.ID,
        //        scheduleId: schedule.ID,
        //        locationId: location.ID,
        //        groupId: activityGroup.ID,
        //        minGradeLevelId: gradeLevels.First().ID,
        //        maxGradeLevelId: gradeLevels.Last().ID,
        //        loggedInUser: this.LoggedInUser,
        //        msgs: msgs,
        //        dc: dc
        //        );

        //    var newActivityName = "Test Activity Update";

        //    msgs.AddRange(activity.UpdateValues(
        //        activityName: newActivityName,
        //        activityDetails: activity.Description,
        //        seatsAvailable: activity.SeatsAvailable.Value,
        //        waitingListSeatsAvailable: activity.WaitingListSeats,
        //        contactName: activity.ContactName
        //        contactPhone: activity.ContactPhone
        //        contactEmail: "dave@dave.com",
        //        costPerRegistrant: 12,
        //        costDescription: "Here is the main reason for the cost and what you get",
        //        acceptCreditCard: false,
        //        allowToPayLater: true,
        //        registrationMode: RegistrationMode.Register,
        //        accountId: ActivityOwnerAccount.ID,
        //        scheduleId: schedule.ID,
        //        locationId: location.ID,
        //        groupId: activityGroup.ID,
        //        minGradeLevelId: gradeLevels.First().ID,
        //        maxGradeLevelId: gradeLevels.Last().ID,
        //        loggedInUser: this.LoggedInUser,
        //        dc: dc
        //        ));

        //    Assert.NotNull(activity);
        //}

    }
}
