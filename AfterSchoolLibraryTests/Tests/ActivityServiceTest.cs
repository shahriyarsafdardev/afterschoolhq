using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System;

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

        //[Fact(DisplayName = "Payment plan is saved as an option on an activity")]
        //public void AddPaymentPlanActivityObjectTest()
        //{
        //    var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
        //    var plan = activity.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

        //    Assert.Single(SubscriptionPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, SubscriptionPlanType.PaymentPlan, msgs, dc));
        //}

        [Fact(DisplayName = "Payment plan is saved as an option on an activity group")]
        public void AddPaymentPlanActivityGroupObjectTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var plan = activityGroup.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

            Assert.Single(SubscriptionPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, SubscriptionPlanType.PaymentPlan, msgs, dc));
        }

        [Fact(DisplayName = "GetParticipationObjectActivityTestFull")]
        public void GetParticipationObjectActivityTestFull()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);
            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            var location = LocationFactory.Create(ActivityOwnerAccount, msgs, dc);
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location,null, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc);
            var period = RegistrationPeriodFactory.Create(ActivityOwnerAccount, msgs, dc);
            var arp = ActivityRegistrationPeriodFactory.Create(activity, period, msgs, dc);

            Assert.IsType<ParticipationObject>(activity.GetParticipationObject(msgs, dc));
        }

        [Fact(DisplayName = "GetParticipationObjectActivityTestSimple")]
        public void GetParticipationObjectActivityTestSimple()
        {           
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            Assert.IsType<ParticipationObject>(activity.GetParticipationObject(msgs, dc));
        }

        [Fact(DisplayName = "GetPaymentPlansTest")]
        public void GetPaymentPlansTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = SubscriptionPlanFactory.Create(ActivityOwnerAccount, activity, SubscriptionPlanType.PaymentPlan, msgs, dc);

            Assert.Single(activity.GetActivePaymentPlans(msgs, dc));
        }

        [Fact(DisplayName = "GetSubscriptionPlansTest")]
        public void GetSubscriptionPlansTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);
            var plan = SubscriptionPlanFactory.Create(ActivityOwnerAccount, activity, SubscriptionPlanType.Subscription, msgs, dc);

            Assert.Single(activity.GetActiveSubscriptionPlans(msgs, dc));
        }

        [Fact(DisplayName = "CreateNewTest")]
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

        [Theory(DisplayName = "GenerateUniqueName")]
        [InlineData("My Sample Account", "MySampleAccount")]
        [InlineData("My Sample Account Rocks Bud Boy", "MySampleAccountRocksBud")]
        [InlineData("My!@#$%^&*()-_+<>/,.?[]{}\\|'\":;~`", "My-_")]
        public void GenerateUniqueName(string input, string expectedOutput)
        {
            var output = ActivityService.GenerateUniqueName(input, null, ActivityOwnerAccount.ID, msgs, dc);

            Assert.Equal(expectedOutput, output);
        }

        [Fact(DisplayName = "FinalizeStartAndEndDateForSubscriptionTest")]
        public void FinalizeStartAndEndDateForSubscriptionTest()
        {
            DateTime dayOfMonth = new DateTime(2021, 06, 06);

            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, msgs, dc);

            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity, BillingPeriodType.Month, System.DayOfWeek.Monday, dayOfMonth, msgs,dc);

            Assert.True(paymentStart == dayOfMonth);
        }    
        
        [Fact(DisplayName = "FinalizeStartAndEndDateForWeekSubscriptionTest")]
        public void FinalizeStartAndEndDateForOneWeekSubscriptionTest()
        {
            DateTime dayOfMonth = new DateTime();

            var activityGroup = ActivityGroupFactory.Create(ActivityOwnerAccount, ActivityGroupType.Session, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);
            
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, msgs, dc);

            var (paymentStart, paymentEnd) = ActivityService.FinalizeStartAndEndDateForSubscription(activity, BillingPeriodType.Week, System.DayOfWeek.Monday, dayOfMonth, msgs,dc);


            Assert.True(paymentStart.HasValue && paymentEnd.HasValue);
        }
        
        
    }
}