using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using AfterSchoolLibrary.Billing;

namespace AfterSchoolLibraryTests.Tests
{
    public class StripeAccountServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        // private StripeAccount StripeAccount { get; set; }

        public StripeAccountServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            Account = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
            // StripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);
        }

        [Theory(Skip = "failing")]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void CanReceivePaymentDepositsTest(bool chargesEnabled, bool payoutsEnabled, bool expectedResult)
        {
            //StripeAccount.ChargesEnabled = chargesEnabled;
            //StripeAccount.PayoutsEnabled = payoutsEnabled;

            //Assert.Equal(expectedResult, StripeAccount.CanReceivePaymentDeposits());
        }

        [Fact(Skip = "failing")]
        public void SaveAndSendBankInfoToStripeTest()
        {
            Assert.True(false);
        }

        [Fact(Skip = "failing")]
        public void GetLatestStripeAccountDetailsTest()
        {
            Assert.True(false);
        }

        [Fact(Skip = "failing")]
        public void SaveAndSendLegalEntityToStripeTest()
        {
            //var package = PackageFactory.Create(msgs, dc);
            //var accountPackage = AccountPackageFactory.Create(Account, package, msgs, dc);

            
        }

        [Fact(Skip = "failing")]
        public void ParseAndUpdateStripeCustomAccountTest()
        {

            Assert.True(false);
        }

        [Fact(Skip = "failing")]
        public void GetAccount()
        {
            Assert.True(false);
        }

        [Fact(Skip = "failing")]
        public void SendPayout()
        {
            Assert.True(false);
        }
    }
}
