using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityGroupServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private WebUser LoggedInUser { get; set; }
        private Account ActivityGroupOwner { get; set; }

        public ActivityGroupServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            LoggedInUser = WebUserFactory.Create(msgs, dc);
            ActivityGroupOwner = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void ToParticipationObjectTestSimple()
        {
            var actvityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.League, msgs, dc);

            Assert.NotNull(actvityGroup.ToParticipationObject(msgs, dc));
        }

        [Fact]
        public void ToParticipationObjectTestFull()
        {
            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(ActivityGroupOwner, msgs, dc);
            var actvityGroup = ActivityGroupFactory.Create(
                ActivityGroupOwner, 
                ActivityGroupType.Session, 
                schedule, 
                GenderRestrictionType.FemaleOnly,
                gradeLevels.First().ID,
                gradeLevels.Last().ID,
                35,
                CostUnitType.PerActivity,
                true,
                msgs, 
                dc);
            var activityInstance = ActivityInstanceFactory.Create(actvityGroup, schedule, msgs, dc);
            var period = RegistrationPeriodFactory.Create(ActivityGroupOwner, actvityGroup, msgs, dc);

            Assert.NotNull(actvityGroup.ToParticipationObject(msgs, dc));
        }

        [Fact]
        public void GetPaymentPlansTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.League, msgs, dc);
            var plan = PaymentPlanFactory.Create(ActivityGroupOwner, activityGroup, msgs, dc);

            Assert.Single(activityGroup.GetPaymentPlans(msgs, dc));
        }

        [Fact]
        public void GetSubscriptionPlansTest()
        {
            var activityGroup = ActivityGroupFactory.Create(ActivityGroupOwner, ActivityGroupType.League, msgs, dc);
            var plan = SubscriptionPlanFactory.Create(ActivityGroupOwner, activityGroup, msgs, dc);

            Assert.Single(activityGroup.GetSubscriptionPlans(msgs, dc));
        }

    }
}
