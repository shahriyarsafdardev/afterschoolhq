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
        [InlineData(.01, 2.00)]
        [InlineData(20, 22.00)]
        [InlineData(50, 54.48)]
        [InlineData(100, 108.98)]
        [InlineData(200, 216.98)]
        public void GetCheckoutCostWithCreditCardTest(decimal registrationFee, double expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateWithFreePackage(accountOwner, AccountType.ActivityProvider, msgs, dc);
            var calcObj = new FeeCalculationObject(registrationFee, account, msgs, dc);

            Assert.Equal(expectedResult.ToDecimal(), calcObj.GetFinalTotalCost(PaymentType.CreditCard));
        }

        [Theory(DisplayName = "Get Total Checkout Cost without Credit Card ")]
        [InlineData(.01, 1.49)]
        [InlineData(20, 20.99)]
        [InlineData(50, 52.49)]
        [InlineData(100, 104.99)]
        [InlineData(200, 209.99)]
        public void GetCheckoutCostWithoutCreditCardTest(decimal registrationFee, double expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.CreateFreePackage(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var calcObj = new FeeCalculationObject(registrationFee, account, msgs, dc);

            Assert.Equal(expectedResult.ToDecimal(), calcObj.GetCostWithConvenienceFee());
        }
    }
}
