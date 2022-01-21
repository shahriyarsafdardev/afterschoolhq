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

        [Fact]
        public void SaveTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            stripeAccount.LastModifiedBy = 1;
            msgs.AddRange(stripeAccount.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var stripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);

            msgs.AddRange(stripeAccount.Delete(dc));

            Assert.Null(StripeAccount.Load(stripeAccount.ID.Value));
        }

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
