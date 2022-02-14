using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class Migration_20220214_UpdateActivityContinuousEnrollmentFlag_Test : BaseTest
    {
        private WebUser ProviderWebUser { get; set; }
        private Account ProviderAccount { get; set; }

        public Migration_20220214_UpdateActivityContinuousEnrollmentFlag_Test(ITestOutputHelper output) : base(output)
        {
            ProviderWebUser = WebUserFactory.Create(msgs, dc);
            ProviderAccount = AccountFactory.CreateWithFreePackage(ProviderWebUser, AccountType.ActivityProvider, msgs, dc);
        }

        [Fact(Skip = "Use for debugging")]
        public void Test()
        {            
            // Arrange
            var actWithCE = ActivityFactory.CreateWithCE(account: ProviderAccount, isContinuousEnrollment: true, msgs: msgs, dc: dc);
            var actWithoutCE = ActivityFactory.CreateWithCE(account: ProviderAccount, isContinuousEnrollment: false, msgs: msgs, dc: dc);
            var actWithoutCE2 = ActivityFactory.Create(account: ProviderAccount, msgs: msgs, dc: dc);

            // Act
            AfterSchoolLibrary.Migrations.Migration_20220214_UpdateActivityContinuousEnrollmentFlag.Do(msgs, dc);

            // Assert
            var activities = Activity.LoadList(ids: new List<int>() { actWithCE.ID.Value, actWithoutCE.ID.Value, actWithoutCE2.ID.Value }, msgs: msgs, dc: dc);
            Assert.True(msgs.HasNoErrors());
            Assert.True(activities.IsNotEmpty() && activities.Count() == 3);
            Assert.True(activities.Where(m => m.IsContinuousEnrollment == true).Count() == 1);
            Assert.True(activities.Where(m => m.IsContinuousEnrollment == false).Count() == 2);
        }
    }
}
       