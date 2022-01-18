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
    public class AddFeatureTypeTeacher1099ToSchools_20210622_Test : BaseTest
    {
        private WebUser SchoolWebUser { get; set; }
        private WebUser ProviderWebUser { get; set; }
        private Account SchoolAccount { get; set; }
        private Account ProviderAccount { get; set; }

        public AddFeatureTypeTeacher1099ToSchools_20210622_Test(ITestOutputHelper output) : base(output)
        {
            SchoolWebUser = WebUserFactory.Create(msgs, dc);
            SchoolAccount = AccountFactory.CreateWithFreePackage(SchoolWebUser, AccountType.School, msgs, dc);
            ProviderWebUser = WebUserFactory.Create(msgs, dc);
            ProviderAccount = AccountFactory.CreateWithFreePackage(ProviderWebUser, AccountType.ActivityProvider, msgs, dc);
        }

        [Fact(Skip = "Use for debugging")]
        public void Test()
        {    
            try
            {
                // Arrange
                if (msgs.IsEmpty()) msgs = new SystemMessages();

                AfterSchoolLibrary.Helpers.DatabaseInit.AdditionalData(dc);

                // Act                   
                 AfterSchoolLibrary.Migrations.AddFeatureTypeTeacher1099ToSchools_20210622.Do(msgs, dc);
        
                var expectedSchoolTeacher1099Feature = SchoolAccount.GetAccountPackageFeature(FeatureType.Teacher1099, msgs, dc);
                var expectedProviderTeacher1099Feature = ProviderAccount.GetAccountPackageFeature(FeatureType.Teacher1099, msgs, dc);

                // Assert
                Assert.True(msgs.HasNoErrors());
                Assert.NotNull(expectedSchoolTeacher1099Feature);
                Assert.NotNull(expectedProviderTeacher1099Feature);
                Assert.Equal(expected: 1, actual: expectedSchoolTeacher1099Feature.GetMaxValue());
                Assert.Equal(expected: 0, actual: expectedProviderTeacher1099Feature.GetMaxValue());
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
       