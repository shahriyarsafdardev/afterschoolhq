using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class AddFeatureTypeSurvey_20211004_Test : BaseTest
    {
        private WebUser SchoolWebUser { get; set; }
        private WebUser ProviderWebUser { get; set; }
        private Account SchoolAccount { get; set; }
        private Account ProviderAccount { get; set; }

        public AddFeatureTypeSurvey_20211004_Test(ITestOutputHelper output) : base(output)
        {
            SchoolWebUser = WebUserFactory.Create(msgs, dc);
            SchoolAccount = AccountFactory.CreateWithFreePackage(SchoolWebUser, AccountType.School, msgs, dc);
        }

        [Fact]
        public void RunMigration()
        {    
            try
            {
                // Arrange
                if (msgs.IsEmpty()) msgs = new SystemMessages();

                AfterSchoolLibrary.Helpers.Utilities.AdditionalData(dc);

                // Act                   
                 AfterSchoolLibrary.Migrations.AddFeatureTypeSurvey_20211004.Do(msgs, dc);
        
                var expectedSurveyFeature = SchoolAccount.GetAccountPackageFeature(FeatureType.Teacher1099, msgs, dc);

                // Assert
                Assert.True(msgs.HasNoErrors());
                Assert.NotNull(expectedSurveyFeature);
                Assert.Equal(expected: 0, actual: expectedSurveyFeature.GetMaxValue());
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
       