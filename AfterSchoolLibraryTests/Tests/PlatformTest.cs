using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using System;
using AfterSchoolLibrary.Billing;
using BasicLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class PlatformTest : BaseTest
    {
        public PlatformTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory(DisplayName = "Get Total Checkout Cost with Credit Card ")]
        [InlineData(.01, 2)]
        [InlineData(20, 22.5)]
        [InlineData(50, 55)]
        [InlineData(100, 110)]
        [InlineData(200, 218)]
        public void GetCheckoutCostWithCreditCardTest(decimal registrationFee, decimal expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateWithFreePackage(accountOwner, AccountType.ActivityProvider, msgs, dc);
            var calcObj = new FeeCalculationObject(registrationFee, account, msgs, dc);

            Assert.Equal(expectedResult, calcObj.GetFinalTotalCost(PaymentType.CreditCard));
        }

        [Theory(DisplayName = "Get Total Checkout Cost without Credit Card ")]
        [InlineData(.01, 1.5)]
        [InlineData(20, 21.5)]
        [InlineData(50, 53)]
        [InlineData(100, 106)]
        [InlineData(200, 211)]
        public void GetCheckoutCostWithoutCreditCardTest(decimal registrationFee, decimal expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.CreateFreePackage(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var calcObj = new FeeCalculationObject(registrationFee, account, msgs, dc);

            Assert.Equal(expectedResult, calcObj.GetCostWithConvenienceFee());
        }
    }
}
