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
        private Account PlanOwner { get; set; }

        public ActivityServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact(DisplayName = "Payment plan is saved as an option on an activity")]
        public void AddPaymentPlanActivityObjectTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);
            var plan = activity.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

            Assert.Single(PaymentPlan.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc));
        }

        [Fact(DisplayName = "Payment plan is saved as an option on an activity group")]
        public void AddPaymentPlanActivityGroupObjectTest()
        {
            var activityGroup = ActivityGroupFactory.Create(PlanOwner, msgs, dc);
            var plan = activityGroup.AddPaymentPlan(35, BillingPeriodType.Month, 12, LoggedInUser, msgs, dc);

            Assert.Single(PaymentPlan.LoadByParticipationObject(activityGroup.ID.Value, ParticipationObjectType.ActivityGroup, msgs, dc));
        }

    }
}
