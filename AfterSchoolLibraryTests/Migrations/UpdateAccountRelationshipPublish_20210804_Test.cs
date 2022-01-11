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
    public class UpdateAccountRelationshipPublish_20210804_Test : BaseTest
    {
        private WebUser SchoolWebUser { get; set; }
        private WebUser ProviderWebUser { get; set; }
        private Account SchoolAccount { get; set; }
        private Account ProviderAccount { get; set; }

        public UpdateAccountRelationshipPublish_20210804_Test(ITestOutputHelper output) : base(output)
        {
            SchoolWebUser = WebUserFactory.Create(msgs, dc);
            SchoolAccount = AccountFactory.CreateWithFreePackage(SchoolWebUser, AccountType.School, msgs, dc);
            ProviderWebUser = WebUserFactory.Create(msgs, dc);
            ProviderAccount = AccountFactory.CreateWithFreePackage(ProviderWebUser, AccountType.ActivityProvider, msgs, dc);
        }

        [Fact]
        public void RunMigration()
        {
            AfterSchoolLibrary.Helpers.DatabaseInit.AdditionalData(dc);

            // Act                   
            msgs.AddRange(SchoolAccount.AddVisitorAccount(ProviderAccount, HostPermissionType.ApproveOnly, ApprovalStatus.Approved, false, true, SchoolWebUser, dc));

            AfterSchoolLibrary.Migrations.UpdateAccountRelationshipPublish_20210804.Do(msgs, dc);

            var ar = ProviderAccount.GetRequestForAccount(SchoolAccount, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(ar);
            Assert.True(ar.RequiresPublishPermission);
            Assert.True(ar.ShowAllPrograms);
        }
    }
}
       