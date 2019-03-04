using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionPlanServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account PlanOwner { get; set; }

        public SubscriptionPlanServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void CreateNewActivityObjectTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscription(
                PlanOwner,
                ParticipationObjectType.Activity,
                activity.ID.Value,
                35.ToDecimal(),
                BillingPeriodType.Month,
                true,
                LoggedInUser.ID.Value,
                msgs,
                dc);

            Assert.True(plan.ID.HasValue);
        }

        [Fact(DisplayName = "Create new Activity Group")]
        public void CreateNewActivityGroupTest()
        {
            var activityGroup = ActivityGroupFactory.Create(PlanOwner, ActivityGroupType.Session, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscription(
                PlanOwner,
                ParticipationObjectType.ActivityGroup,
                activityGroup.ID.Value,
                35.ToDecimal(),
                BillingPeriodType.Month,
                true,
                LoggedInUser.ID.Value,
                msgs,
                dc);

            Assert.True(plan.ID.HasValue);
        }

    }
}
