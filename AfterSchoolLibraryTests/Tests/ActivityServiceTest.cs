using System;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

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

    }
}
