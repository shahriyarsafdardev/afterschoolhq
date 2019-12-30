using System;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class PaymentPlanServiceTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account PlanOwner { get; set; }

        public PaymentPlanServiceTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            PlanOwner = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void CreateNewActivityObjectTest()
        {
            var activity = ActivityFactory.Create(PlanOwner, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewPaymentPlan(
                PlanOwner,
                ParticipationObjectType.Activity,
                activity.ID.Value,
                35.ToDecimal(),
                BillingPeriodType.Month,
                12,
                PlanPaymentStartType.WhenActivityStarts,
                LoggedInUser.ID.Value,
                msgs,
                dc);

            Assert.True(plan.ID.HasValue);
        }

        [Fact(DisplayName = "CreateNewActivityGroupObjectTest")]
        public void CreateNewActivityGroupObjectTest()
        {
            var activityGroup = ActivityGroupFactory.Create(PlanOwner, ActivityGroupType.Session, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewPaymentPlan(
                PlanOwner,
                ParticipationObjectType.ActivityGroup,
                activityGroup.ID.Value,
                35.ToDecimal(),
                BillingPeriodType.Month,
                12,
                PlanPaymentStartType.WhenActivityStarts,
                LoggedInUser.ID.Value,
                msgs,
                dc);

            Assert.True(plan.ID.HasValue);
        }
    }
}
