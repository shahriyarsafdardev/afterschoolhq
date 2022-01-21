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
            SchoolWebUser = WebUserFactory.Create(msgs, dc);
            SchoolAccount = AccountFactory.CreateWithFreePackage(SchoolWebUser, AccountType.School, msgs, dc);
        }

        [Fact(Skip = "Use for debugging")]
        public void Test()
        {
            var freePackage = PackageFactory.CreateFreePackage(msgs, dc);

            var apProvider = ProviderAccount.AddPackage(freePackage.ID.Value, BillingPeriodType.Month, ProviderWebUser, msgs, dc);
            var apSchool = SchoolAccount.AddPackage(freePackage.ID.Value, BillingPeriodType.Month, SchoolWebUser, msgs, dc);

            var providerPercentage = .15M;
            var providerFlat = 1;
            var schoolPercentage = .12M;
            var schoolFlat = 2;

            apProvider.AddOrUpdatePackageFeature(FeatureType.Participants, 2000, ProviderWebUser, msgs, dc);
            apProvider.AddOrUpdatePackageFeature(FeatureType.ManageForms, 1, ProviderWebUser, msgs, dc);
            apProvider.AddOrUpdatePackageFeature(FeatureType.UseStripeConnectedAccount, 0, ProviderWebUser, msgs, dc);
            apProvider.AddOrUpdatePackageFeature(FeatureType.PaymentsPercentageFee, providerPercentage, ProviderWebUser, msgs, dc);
            apProvider.AddOrUpdatePackageFeature(FeatureType.PaymentsPercentageFlatFee, providerFlat, ProviderWebUser, msgs, dc);

            apSchool.AddOrUpdatePackageFeature(FeatureType.Participants, 2000, ProviderWebUser, msgs, dc);
            apSchool.AddOrUpdatePackageFeature(FeatureType.ManageForms, 1, ProviderWebUser, msgs, dc);
            apSchool.AddOrUpdatePackageFeature(FeatureType.UseStripeConnectedAccount, 0, ProviderWebUser, msgs, dc);
            apSchool.AddOrUpdatePackageFeature(FeatureType.PaymentsPercentageFee, schoolPercentage, ProviderWebUser, msgs, dc);
            apSchool.AddOrUpdatePackageFeature(FeatureType.PaymentsPercentageFlatFee, schoolFlat, ProviderWebUser, msgs, dc);

            AfterSchoolLibrary.Migrations.ResetFreeAcctsToFreePackage_20210805.Do(msgs, dc);

            var providerAccountPackage = ProviderAccount.GetActiveAccountPackage(msgs, dc);
            var schoolAccountPackage = SchoolAccount.GetActiveAccountPackage(msgs, dc);

            foreach (var pf in freePackage.GetFeatures(msgs, dc))
            {
                var apfProvider = providerAccountPackage.GetFeature(pf.FeatureTypeId, msgs, dc);
                var apfSchool = schoolAccountPackage.GetFeature(pf.FeatureTypeId, msgs, dc);

                if (pf.FeatureTypeId.NotIn(new FeatureType[] { FeatureType.ManageLeagues, FeatureType.PaymentsPercentageFee, FeatureType.PaymentsPercentageFlatFee }))
                {
                    Assert.True(apfProvider.Max >= pf.Max);

                    if (pf.FeatureTypeId == FeatureType.UseStripeConnectedAccount)
                        Assert.Equal(0M, apfSchool.Max.Value);
                    else
                        Assert.True(apfSchool.Max >= pf.Max);
                }
                else
                {
                    switch(pf.FeatureTypeId)
                    {
                        case FeatureType.ManageLeagues:
                            Assert.Equal(0M, apfProvider.Max.Value);
                            Assert.Equal(0M, apfSchool.Max.Value);
                            break;
                        case FeatureType.PaymentsPercentageFlatFee:
                            Assert.Equal(providerFlat, apfProvider.Max.Value);
                            Assert.Equal(schoolFlat, apfSchool.Max.Value);
                            break;
                        case FeatureType.PaymentsPercentageFee:
                            Assert.Equal(providerPercentage, apfProvider.Max.Value);
                            Assert.Equal(schoolPercentage, apfSchool.Max.Value);
                            break;
                    }
                }
            }

            Assert.True(msgs.HasNoErrors());

        }
    }
}
       