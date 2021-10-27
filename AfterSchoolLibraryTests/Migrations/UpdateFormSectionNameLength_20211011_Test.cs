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
    public class UpdateFormSectionNameLength_20211011_Test : BaseTest
    {
        private WebUser ProviderWebUser { get; set; }
        private Account ProviderAccount { get; set; }
        private Form ProviderForm { get; set; }

        public UpdateFormSectionNameLength_20211011_Test(ITestOutputHelper output) : base(output)
        {
            ProviderWebUser = WebUserFactory.Create(msgs, dc);
            ProviderAccount = AccountFactory.CreateWithFreePackage(ProviderWebUser, AccountType.ActivityProvider, msgs, dc);
            ProviderAccount.AddOrUpdatePackageFeature(FeatureType.ManageForms, 10, ProviderWebUser, msgs, dc);
            msgs.AddRange(ProviderAccount.Save(dc));
            ProviderAccount.Reload(msgs, dc);
            ProviderForm = FormFactory.Create(ProviderAccount, msgs, dc);
        }


        [Fact]
        public void BeforeMigration()
        {
            try
            {
                // Arrange
                if (msgs.IsEmpty()) msgs = new SystemMessages();

                AfterSchoolLibrary.Helpers.Utilities.AdditionalData(dc);

                // Act                   
                var formQuestionGroup = FormQuestionGroupFactory.Create(ProviderForm.ID.Value,true, msgs, dc);
                formQuestionGroup.Name = "This is a really long Name for a Form Question Group Name it has so many characters we dont know what to do with all of them anymore please help";
                msgs.AddRange(formQuestionGroup.Save(dc));
                
                // Assert
                Assert.True(msgs.HasErrors());
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
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
                AfterSchoolLibrary.Migrations.UpdateFormSectionNameLength_20211011.Do(msgs, dc);
                var formQuestionGroup = FormQuestionGroupFactory.Create(ProviderForm.ID.Value, true, msgs, dc);
                formQuestionGroup.Name = "This is a really long Name for a Form Question Group Name it has so many characters we dont know what to do with all of them anymore please help";
                msgs.AddRange(formQuestionGroup.Save(dc));
            
                // Assert
                Assert.True(msgs.HasNoErrors());
        }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
       