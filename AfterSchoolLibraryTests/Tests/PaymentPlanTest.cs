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
    public class PaymentPlanTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account PlanOwnerAccount { get; set; }
        private Activity Activity { get; set; }
        private ActivityGroup ActivityGroup { get; set; }

        public PaymentPlanTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            PlanOwnerAccount = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
            Activity = Factories.ActivityFactory.Create(PlanOwnerAccount, msgs, dc);
            ActivityGroup = Factories.ActivityGroupFactory.Create(PlanOwnerAccount, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);

            Assert.True(PaymentPlan.Load(plan.ID.Value, msgs, dc).IsNotEmpty());
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
                var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                PaymentPlan.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of payment plans in the database
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
                var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);
            }

            Assert.Equal(numberToCreate, PaymentPlan.GetCount(msgs: msgs, dc: dc));
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
            var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);

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
            var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);

            msgs.AddRange(plan.Delete(dc));

            Assert.Equal(0, PaymentPlan.GetCount(msgs: msgs, dc: dc));
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
            var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);

            Assert.Equal(plan.ID, plan.Reload(msgs, dc).ID);
        }

        [Theory]
        [InlineData(ParticipationObjectType.Activity)]
        [InlineData(ParticipationObjectType.ActivityGroup)]
        public void LoadByParticipationObjectTest(ParticipationObjectType ParticipationObjectType)
        {
            var accountOwner = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.Create(accountOwner, msgs, dc);
            var activity = Factories.ActivityFactory.Create(account, msgs, dc);
            var activityGroup = Factories.ActivityGroupFactory.Create(account, msgs, dc);
            var plan1 = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);
            var plan2 = Factories.PaymentPlanFactory.Create(account, activityGroup, msgs, dc);

            var ParticipationObjectId = activity.ID;
            if (ParticipationObjectType == ParticipationObjectType.ActivityGroup)
                ParticipationObjectId = activityGroup.ID;

            Assert.Single(PaymentPlan.LoadByParticipationObject(ParticipationObjectId.Value, ParticipationObjectType, msgs, dc));
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
                var plan = Factories.PaymentPlanFactory.Create(account, activity, msgs, dc);
            }

            Assert.Equal(numberToCreate, PaymentPlan.LoadByPlanOwnerAccountId(account.ID.Value, msgs, dc).Count);
        }

    }
}
