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
    public class ResetFreeAcctsToFreePackage_20210805_Test : BaseTest
    {
        private WebUser SchoolWebUser { get; set; }
        private WebUser ProviderWebUser { get; set; }
        private Account SchoolAccount { get; set; }
        private Account ProviderAccount { get; set; }

        public ResetFreeAcctsToFreePackage_20210805_Test(ITestOutputHelper output) : base(output)
        {
            ProviderWebUser = WebUserFactory.Create(msgs, dc);
            ProviderAccount = AccountFactory.CreateWithFreePackage(ProviderWebUser, AccountType.ActivityProvider, msgs, dc);
        }

        [Fact]
        public void RunMigration()
        {
            var freePackage = PackageFactory.CreateFreePackage(msgs, dc);

            var ap = ProviderAccount.AddPackage(freePackage.ID.Value, BillingPeriodType.Month, ProviderWebUser, msgs, dc);

            ap.AddOrUpdatePackageFeature(FeatureType.Participants, 2000, ProviderWebUser, msgs, dc);
            ap.AddOrUpdatePackageFeature(FeatureType.ManageForms, 1, ProviderWebUser, msgs, dc);

            AfterSchoolLibrary.Migrations.ResetFreeAcctsToFreePackage_20210805.Do(msgs, dc);

            var accountPackage = ProviderAccount.GetActiveAccountPackage(msgs, dc);

            foreach (var pf in freePackage.GetFeatures(msgs, dc))
            {
                var apf = accountPackage.GetFeature(pf.FeatureTypeId, msgs, dc);
                
                if (pf.FeatureTypeId.NotIn(new FeatureType[] { FeatureType.ManageLeagues, FeatureType.PaymentsPercentageFee, FeatureType.PaymentsPercentageFlatFee }))
                    Assert.True(apf.Max >= pf.Max);
            }

            Assert.True(msgs.HasNoErrors());

        }
    }
}
       