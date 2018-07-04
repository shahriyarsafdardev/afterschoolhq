using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class SubscriptionPlanTest : BaseTest
    {
        public SubscriptionPlanTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);

            Assert.True(SubscriptionPlan.Load(plan.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            for (var i = 0; i < numberToCreate; i++)
            {
                var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                SubscriptionPlan.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of subscription plans in the database
        /// </summary>
        /// <param name="numberToCreate"></param>
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
                var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
                var activity = Factories.ActivityFactory.Create(account, msgs, dc);
                var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);
            }

            Assert.Equal(numberToCreate, SubscriptionPlan.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the plan to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);

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
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);

            msgs.AddRange(plan.Delete(dc));

            Assert.Equal(0, SubscriptionPlan.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);

            Assert.Equal(plan.ID, plan.Reload(msgs, dc).ID);
        }

        [Theory]
        [InlineData(ParticipationSourceType.Activity)]
        [InlineData(ParticipationSourceType.ActivityGroup)]
        public void LoadByParticipationSourceTest(ParticipationSourceType participationSourceType)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            var plan1 = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);
            var plan2 = Factories.SubscriptionPlanFactory.Create(account, activityGroup, msgs, dc);

            var participationSourceId = activity.ID;
            if (participationSourceType == ParticipationSourceType.ActivityGroup)
                participationSourceId = activityGroup.ID;

            Assert.Single(SubscriptionPlan.LoadByParticipationSource(participationSourceId.Value, participationSourceType, msgs, dc));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        public void LoadByPlanOwnerAccountIdTest(int numberToCreate)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);

            for (var i = 0; i < numberToCreate; i++)
            {
                var plan = Factories.SubscriptionPlanFactory.Create(account, activity, msgs, dc);
            }

            Assert.Equal(numberToCreate, SubscriptionPlan.LoadByPlanOwnerAccountId(account.ID.Value, msgs, dc).Count);
        }

    }
}
