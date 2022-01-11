using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class AddFeatureTypeManualStripePayouts_20211128_Test : BaseTest
    {
        private WebUser SchoolWebUser { get; set; }
        private Account SchoolAccount { get; set; }

        public AddFeatureTypeManualStripePayouts_20211128_Test(ITestOutputHelper output) : base(output)
        {
            SchoolWebUser = WebUserFactory.Create(msgs, dc);
            SchoolAccount = AccountFactory.CreateWithFreePackage(SchoolWebUser, AccountType.School, msgs, dc);
        }

        [Fact(DisplayName = "Test Migration Success")]
        public void RunMigration1()
        {
            // Arrange
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            AfterSchoolLibrary.Helpers.DatabaseInit.AdditionalData(dc);

            // Act                   
            AfterSchoolLibrary.Migrations.AddFeatureTypeManualStripePayouts_20211128.Do(msgs, dc);

            var expectedFeature = SchoolAccount.GetAccountPackageFeature(FeatureType.ManualStripePayouts, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(expectedFeature);

            Assert.Equal(expected: FeatureType.ManualStripePayouts, actual: expectedFeature.FeatureTypeId);
            Assert.Equal(expected: 0, actual: expectedFeature.GetMaxValue());
        }
    }
}
