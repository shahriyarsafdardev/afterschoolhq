using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class TransactionTest : BaseTest
    {
        private WebUser BillerAccountOwner { get; set; }
        private Account BillerAccount { get; set; }
        private WebUser PayerAccountOwner { get; set; }
        private Account PayerAccount { get; set; }

        public TransactionTest(ITestOutputHelper output) : base(output)
        {
            BillerAccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            BillerAccount = Factories.AccountFactory.Create(BillerAccountOwner, msgs, dc);
            PayerAccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            PayerAccount = Factories.AccountFactory.Create(PayerAccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            Assert.True(Transaction.Load(transaction.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Transaction.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of transactions in the database
        /// </summary>
        /// <param name="numberToCreate"></param>
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);
            }

            Assert.Equal(numberToCreate, Transaction.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the plan to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            transaction.LastModifiedBy = 1;
            msgs.AddRange(transaction.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            msgs.AddRange(transaction.Delete(dc));

            Assert.Equal(0, Transaction.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            Assert.Equal(transaction.ID, transaction.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByBillerAccountIdTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            Assert.Single(Transaction.LoadByBillerAccountId(BillerAccount.ID.Value, msgs, dc));
        }

        [Fact]
        public void LoadByPayerAccountIdTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            Assert.Single(Transaction.LoadByPayerAccountId(PayerAccount.ID.Value, msgs, dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, msgs, dc);

            Assert.NotNull(Transaction.LoadByUid(transaction.Uid, msgs, dc));
        }

        [Fact]
        public void LoadByParticipationObjectTest()
        {
            var activity = Factories.ActivityFactory.Create(BillerAccount, msgs, dc);
            var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, activity.ID.Value, ParticipationObjectType.Activity, msgs, dc);

            Assert.Single(Transaction.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs, dc));
        }

        [Fact]
        public void LoadByBillSourceTest()
        {
            var activity = Factories.ActivityFactory.Create(BillerAccount, msgs, dc);
            var plan = Factories.SubscriptionPlanFactory.Create(BillerAccount, activity, msgs, dc);
            var registrant = Factories.RegistrantFactory.Create(msgs, dc);
            var registrantPlan = Factories.RegistrantSubscriptionPlanFactory.Create(registrant, plan, msgs, dc);
            var transaction = Factories.TransactionFactory.Create(
                BillerAccount, 
                PayerAccount, 
                PaymentType.ACH, 
                TransactionType.Credit, 
                activity.ID.Value, 
                ParticipationObjectType.Activity,
                registrantPlan.ID.Value,
                BillSourceType.RegistrantSubscriptionPlan,
                msgs, 
                dc);

            Assert.Single(Transaction.LoadByBillSource(registrantPlan.ID.Value, BillSourceType.RegistrantSubscriptionPlan, msgs, dc));
        }

        [Fact]
        public void GetTotalAmountReceivedByAccountIdTest()
        {
            var amounts = new List<decimal>()
            {
                5.23.ToDecimal(),
                100.95.ToDecimal(),
                345.ToDecimal(),
                0.55.ToDecimal()
            };

            foreach (var amount in amounts)
            {
                var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, amount, msgs, dc);
            }

            Assert.Equal(amounts.Sum(), Transaction.GetTotalAmountReceivedByAccountId(BillerAccount.ID.Value, msgs, dc));
        }

        [Fact]
        public void GetTotalAmountPaidByAccountIdTest()
        {
            var amounts = new List<decimal>()
            {
                5.23.ToDecimal(),
                100.95.ToDecimal(),
                345.ToDecimal(),
                0.55.ToDecimal()
            };

            foreach (var amount in amounts)
            {
                var transaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, TransactionType.Credit, amount, msgs, dc);
            }

            Assert.Equal(amounts.Sum(), Transaction.GetTotalAmountPaidByAccountId(PayerAccount.ID.Value, msgs, dc));
        }
    }
}
