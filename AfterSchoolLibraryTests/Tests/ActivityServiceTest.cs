using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System;
using System.Collections.Generic;

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
            var activity = ActivityFactory.Create(ActivityOwnerAccount, schedule, activityGroup, location, msgs, dc);
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

        /// <summary>
        /// returns the numberof attendance registrant objects asociated with the activity that are newly created against an attendace
        /// </summary>
        [Fact]
        public void CreateNewAttendanceRegistrantByRegistrantActivitiesTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc: dc);
            var activity = ActivityFactory.Create(account, msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var activityInstance = ActivityInstanceFactory.Create(activity, schedule, msgs, dc: dc);
            var form = FormFactory.Create(account, msgs, dc: dc);
            var activityForm = ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc: dc);
            var attendance = new Attendance() { ActivityId = activity.ID.Value, ActivityInstanceId = activityInstance.ID.Value };
            attendance.Save(dc: dc);
            var registrant = RegistrantFactory.Create(account, msgs, dc: dc);
            var attendanceRegistrant = new AttendanceRegistrant() { AttendanceId = attendance.ID.Value, NumberPresent = 1, RegistrantId = registrant.ID.Value, NumberPossible = 1, NumberLate = 0 };
            attendanceRegistrant.Save(dc: dc);
            var newRegistrant = RegistrantFactory.Create(account, msgs, dc: dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, newRegistrant, msgs, dc);

            List<AttendanceRegistrant> attendanceRegistrants = new List<AttendanceRegistrant>();
            attendanceRegistrants.Add(attendanceRegistrant);

            List<RegistrantActivity> registrantActivities = new List<RegistrantActivity>();
            registrantActivities.Add(registrantActivity);

            Assert.Single(activity.CreateNewAttendanceRegistrantByRegistrantActivities(attendance, attendanceRegistrants, registrantActivities,LoggedInUser, msgs, dc: dc));
        }

        /// <summary>
        /// Tests that an activity instance has schedule id or not 
        /// </summary>
        [Fact]
        public void IsValidActivityInstance()
        {
            var user = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var schedule = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, schedule, msgs, dc);
            var activityInstance = Factories.ActivityInstanceFactory.Create(activity, schedule, msgs, dc);


            Assert.True(activity.IsValidActivityInstance(activityInstance, msgs, dc));
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