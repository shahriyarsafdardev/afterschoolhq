using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class SubscriptionPlanTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account PlanOwnerAccount { get; set; }
        private Activity Activity { get; set; }
        private ActivityGroup ActivityGroup { get; set; }

        public SubscriptionPlanTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            PlanOwnerAccount = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
            Activity = Factories.ActivityFactory.Create(PlanOwnerAccount, msgs, dc);
            ActivityGroup = Factories.ActivityGroupFactory.Create(PlanOwnerAccount, ActivityGroupType.Session, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var plan = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, Activity, SubscriptionPlanType.Subscription, msgs, dc);

            Assert.True(SubscriptionPlan.Load(plan.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var plan = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, Activity, SubscriptionPlanType.Subscription, msgs, dc);

            plan.LastModifiedBy = 1;
            msgs.AddRange(plan.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var plan = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, Activity, SubscriptionPlanType.Subscription, msgs, dc);

            msgs.AddRange(plan.Delete(dc));

            Assert.Null(SubscriptionPlan.Load(plan.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var plan = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, Activity, SubscriptionPlanType.Subscription, msgs, dc);

            Assert.Equal(plan.ID, plan.Reload(msgs, dc).ID);
        }

        [Theory]
        [InlineData(ParticipationObjectType.Activity)]
        [InlineData(ParticipationObjectType.ActivityGroup)]
        public void LoadByParticipationObjectTest(ParticipationObjectType ParticipationObjectType)
        {
            var plan1 = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, Activity, SubscriptionPlanType.Subscription, msgs, dc);
            var plan2 = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, ActivityGroup, SubscriptionPlanType.Subscription, msgs, dc);

            var ParticipationObjectId = Activity.ID;
            if (ParticipationObjectType == ParticipationObjectType.ActivityGroup)
                ParticipationObjectId = ActivityGroup.ID;

            Assert.Single(SubscriptionPlan.LoadByParticipationObject(ParticipationObjectId.Value, ParticipationObjectType, SubscriptionPlanType.Subscription, msgs, dc));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        public void LoadByPlanOwnerAccountIdTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var plan = Factories.SubscriptionPlanFactory.Create(PlanOwnerAccount, Activity, SubscriptionPlanType.Subscription, msgs, dc);
            }

            Assert.Equal(numberToCreate, SubscriptionPlan.LoadByPlanOwnerAccountId(PlanOwnerAccount.ID.Value, msgs, dc).Count);
        }

    }
}
