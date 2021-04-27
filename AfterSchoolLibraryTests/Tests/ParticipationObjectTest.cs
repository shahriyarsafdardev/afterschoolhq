using System;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class PartcipationObjectTest : BaseTest
    {
        private WebUser LoggedInUser { get; set; }
        private WebUser AccountOwner { get; set; }
        private Account ActivityOwnerAccount { get; }
        private Account PlanOwner { get; }

        public PartcipationObjectTest(ITestOutputHelper output) : base(output)
        {
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            AccountOwner = WebUserFactory.Create(msgs, dc);
            ActivityOwnerAccount = AccountFactory.Create(AccountOwner, msgs, dc);
            PlanOwner = AccountFactory.CreateAccount(AccountOwner, msgs, dc);
        }

        [Fact]
        public void GetNumberOfClassesInCurrentCycleTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(PlanOwner, ParticipationObjectType.Activity, 3008, 100, BillingPeriodType.Month,
            PlanPaymentStartType.OnCheckout, null, null, true, true, false, false,
            true, 1004, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);

            schedule.AccountId = 3008;
            schedule.ID = 2027;
            schedule.ActivityGroupId = 15;

            ParticipationObject participationObject = new ParticipationObject(plan.ParticipationObjectId.Value, ParticipationObjectType.Activity, msgs,dc)
            {
                Schedule = schedule
            };

            var classes = participationObject.GetNumberOfClassesInCurrentCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());

        }
        
        [Fact]
        public void GetNumberOfRemainingClassesInNextCycleTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(PlanOwner, ParticipationObjectType.Activity, 3008, 100, BillingPeriodType.Month,
            PlanPaymentStartType.OnCheckout, null, null, true, true, false, false,
            true, 1004, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);

            schedule.AccountId = 3008;
            schedule.ID = 2027;
            schedule.ActivityGroupId = 15;

            ParticipationObject participationObject = new ParticipationObject(plan.ParticipationObjectId.Value, ParticipationObjectType.Activity, msgs, dc)
            {
                Schedule = schedule
            };

            var classes = participationObject.GetNumberOfRemainingClassesInNextCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());

        }  
        
        [Fact]
        public void GetNumberOfRemainingClassesInCurrentCycleTest()
        {
            var activity = ActivityFactory.Create(ActivityOwnerAccount, msgs, dc);

            var plan = SubscriptionPlanService.CreateNewSubscriptionPlan(PlanOwner, ParticipationObjectType.Activity, 3008, 100, BillingPeriodType.Month,
            PlanPaymentStartType.OnCheckout, null, null, true, true, false, false,
            true, 1004, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(ActivityOwnerAccount, msgs, dc);

            schedule.AccountId = 3008;
            schedule.ID = 2027;
            schedule.ActivityGroupId = 15;

            ParticipationObject participationObject = new ParticipationObject(plan.ParticipationObjectId.Value, ParticipationObjectType.Activity, msgs, dc)
            {
                Schedule = schedule
            };

            var classes = participationObject.GetNumberOfRemainingClassesInCurrentCycle(BillingPeriodType.Month, msgs, dc);

            Assert.True(classes.IsNotEmpty());

        }
    }
}

