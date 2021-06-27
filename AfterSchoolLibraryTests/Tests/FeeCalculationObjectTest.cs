using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class FeeCalculationObjectTest : BaseTest
    {
        private WebUser BillingWebUser { get; set; }
        private Account BillingAccount { get; set; }

        public FeeCalculationObjectTest(ITestOutputHelper output) : base(output)
        {
            BillingWebUser = WebUserFactory.Create(msgs, dc);
            BillingAccount = AccountFactory.CreateWithFreePackage(BillingWebUser, AccountType.ActivityProvider, msgs, dc);
        }

        [Fact]
        public void FactoryCreateTest()
        {
            // Act
            var feeCalculationObject = FeeCalculationObjectFactory.Create(BillingAccount, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(feeCalculationObject);
        }

        [Theory]
        [InlineData(0.15)]
        public void LoadConveniencePercentFee(decimal conveniencePercentFee)
        {
            // Arrange
            var accountPackageConveniencePercentFee = BillingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc);
            accountPackageConveniencePercentFee.Max = conveniencePercentFee;
            msgs.AddRange(accountPackageConveniencePercentFee.Save(dc));

            // Act
            var feeCalculationObject = FeeCalculationObjectFactory.Create(BillingAccount, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: conveniencePercentFee, actual: feeCalculationObject.ConveniencePercentFee);
        }

        [Theory]
        [InlineData(2.50)]
        public void LoadConvenienceFlatFee(decimal convenienceFlatFee)
        {
            // Arrange
            var accountPackageConvenienceFlatFee = BillingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc);
            accountPackageConvenienceFlatFee.Max = convenienceFlatFee;
            msgs.AddRange(accountPackageConvenienceFlatFee.Save(dc));

            // Act
            var feeCalculationObject = FeeCalculationObjectFactory.Create(BillingAccount, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: convenienceFlatFee, actual: feeCalculationObject.ConvenienceFlatFee);
        }


    }
}
