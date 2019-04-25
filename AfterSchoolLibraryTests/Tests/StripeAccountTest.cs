using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests.Tests
{
    public class StripeAccountTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }

        public StripeAccountTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            Account = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
        }
        [Fact]
        public void LoadTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            Assert.True(StripeAccount.Load(stripeAccount.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                StripeAccount.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        /// <summary>
        /// Gets number of stripeAccounts in the database
        /// </summary>
        /// <param name="numberToCreate"></param>
        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);
            }

            Assert.Equal(numberToCreate, StripeAccount.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the plan to the DB
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            stripeAccount.LastModifiedBy = 1;
            msgs.AddRange(stripeAccount.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            msgs.AddRange(stripeAccount.Delete(dc));

            Assert.Equal(0, StripeAccount.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the plan using its own ID
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            Assert.Equal(stripeAccount.ID, stripeAccount.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            Assert.NotNull(StripeAccount.LoadByAccountId(Account.ID.Value, msgs, dc));
        }
    }
}
