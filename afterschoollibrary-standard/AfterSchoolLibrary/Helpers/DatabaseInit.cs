using AfterSchoolLibrary.Migrations;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Helpers
{
    public class DatabaseInit
    {
        public static SystemMessages InitDatabase(DatabaseCredentials databaseCredentials, string adminEmail = null,
            string adminPassword = null,
            string adminPhone = null,
            bool skipTableCreation = false, string accountName = "First Group")
        {
            var msgs = new SystemMessages();

            if (!skipTableCreation)
            {
                var dc2 = new DatabaseConnection(databaseCredentials.ConnectionString);

                msgs.AddRange(Database.CreateTables(dc2));
                msgs.AddRange(Database.AlterTables(dc2));
                msgs.AddRange(Database.UpdateData(dc2));
                msgs.AddRange(Database.CreateIndexes(dc2));
                dc2 = null;
            }

            msgs.Clear();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (WebUser.GetCount() == 0)
                        amsgs.AddRange(AccountService.CreateAfterSchoolAccount(adminEmail, adminPassword, adminPhone,
                            accountName, adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs)
            );

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(AdditionalData(adc));

                    UpdateRegistrantSubscriptionPlans_20190821.Do(amsgs, adc);
                    InvalidateRegistrantAccounts_20190721.Do(amsgs, adc);
                    UpdateRegistrantAccess_20190721.Do(amsgs, adc);
                    UpdateRequiresParentalConsent_20190721.Do(amsgs, adc);
                    UpdateActivityMessageUids_20190721.Do(amsgs, adc);
                    UpdateSendToStaff_20190721.Do(amsgs, adc);
                    InvalidateRegistrantAccounts_20190721.Do(amsgs, adc);
                    CreatePayoutsForConnectedAccounts_20191226.Do(amsgs, adc);
                    UpdateRequiresParentalConsentForActivityGroup_20191226.Do(amsgs, adc);
                    UpdatePlanPaymentStartType_20191230.Do(amsgs, adc);
                    AddFieldTripsFeatureToSchools_20200126.Do(amsgs, adc);
                    CleanUpExtraParentAccounts_20200215.Do(amsgs, adc);
                    ReattachTransactionsToNonDeletedAccounts_20200216.Do(amsgs, adc);
                    FixTransactionsForDeletedAccounts_20200227.Do(amsgs, adc);
                    FixAccountPackageIsActive_20210603.Do(amsgs, adc);

                    if (Utilities.IsProduction)
                        UpdateTransactionsFromStripe_20200320.Do(amsgs, adc);

                    UpdateRefundRequests_20200323.Do(amsgs, adc);
                    UpdateMissingActivityLocationTypes_20200328.Do(amsgs, adc);
                    UpdateFollowers_20200331.Do(amsgs, adc);
                    AddFeesRefundedToTransactions_20200507.Do(amsgs, adc);
                    UpdateIsClaimedOnAccounts_20200622.Do(amsgs, adc);
                    AddUsedStripeConnectToTransactions_20200705.Do(amsgs, adc);
                    SetExistingAccountsToOnboarded_20200724.Do(amsgs, adc);
                    SubscriberToPolymorphicRelationship_20200726.Do(amsgs, adc);
                    UpdateActivityMessageRegistrants_20201012.Do(amsgs, adc);
                    MigratePackagesToUseStripePricesAPI_20201021.Do(amsgs, adc);
                    UpdateAccountPackagesForSaasify_20201028.Do(amsgs, adc);
                    UpdateAccountOnboardingStepDone_20201106.Do(amsgs, adc);
                    AddPayerIdsToRegistrations_20201116.Do(amsgs, adc);
                    AddManageCouponFeatureInPremiumPackage_20201224.Do(amsgs, adc);
                    UpdateOldTransactionsFees_20210203.Do(amsgs, adc);
                    UpdateAccountPropertyValueForJobs_20210226.Do(amsgs, adc);
                    UpdateOldTransactions_20210216.Do(amsgs, adc);

                    UpdateLocationLatitudeLongitude_20210302.Do(amsgs, adc);

                    MigrateSessionToWebSession_20210312.Do(amsgs, adc);
                    UpdateWebSessionAction_20210513.Do(amsgs, adc);
                    UpdateWebSessionActionIndirectObjectType_20210517.Do(amsgs, adc);
                    AddFeatureTypeTeacher1099ToSchools_20210622.Do(amsgs, adc);
                    UpdateLocationLatLongColumn_20210725.Do(amsgs, adc);
                    UpdateAccountRelationshipPublish_20210804.Do(amsgs, adc);
                    ResetFreeAcctsToFreePackage_20210805.Do(amsgs, adc);

                    UpdateFormQuestionAnswers_20210918.Do(amsgs, adc);
                    SetExistingActivityCostFrequencyType_20211003.Do(amsgs, adc);
                    AddFeatureTypeSurvey_20211004.Do(amsgs, adc);
                    UpdateFormQuestionGroup_20211003.Do(amsgs, adc);

                    AddFeatureTypeManualStripePayouts_20211128.Do(amsgs, adc);

                    Migration_20220106_AddFeatureFlagTable.Do(amsgs, adc);
                    UpdateNameColumnFormQuestionGroup_20211119.Do(amsgs, adc);

                    // Migration_20220202_SessionActionToWebSessionAction.Do(amsgs, adc);     
                    Migration_20220214_UpdateActivityContinuousEnrollmentFlag.Do(amsgs, adc);
                    Migration_20220323_1_LatitudeLongitudeAlterColumns.Do(amsgs, adc);
                    Migration_20220323_2_LatitudeLongitudeRefetch.Do(amsgs, adc);
                    Migration_20220407_SetActivityRegistrantIsDeleted.Do(amsgs, adc);
                    Migration_20220411_AddFreeTrialFeatureFlag.Do(amsgs, adc);
                    Migration_20220412_IncreaseWebSessionActionActionStringColumnSize.Do(amsgs, adc);
                    Migration_20220420_AddFreeActivityLimitPackageFeature.Do(amsgs, adc);
                    Migration_20220601_AddStudentIdQuestionToAdminDefaultForm.Do(amsgs, adc);
                    Migration_20220603_AddDistrictofColumbiaToStateOptions.Do(amsgs, adc);
                    Migration_20220606_VolunteerManagement.Do(amsgs, adc);
                    Migration_20220707_EnsureFormTypeIds.Do(amsgs, adc);
                    Migration_20220709_AddPersonTypeToRegistrant.Do(amsgs, adc);
                    Migration_20220709_AddPersonTypeToRegistrant2.Do(amsgs, adc);
                    Migration_20220803_ActivityGroupUseDetailsNotDescription.Do(amsgs, adc);
                    Migration_20220823_InitializeActivityDetailsTable.Do(amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs)
            );

            return msgs;
        }

        public static SystemMessages AdditionalData(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(GradeLevel.CreateDefaultGradeLevels(dc));

            FormService.AddNewAfterSchoolDefaultStudentForm(msgs, dc);

            var packageCount = Package.GetCount(null, msgs, dc);
            if (packageCount == 0)
            {
                var package = PackageService.CreateDefaultPackage(null, msgs, dc);
                msgs.AddRange(AccountService.CreateSampleAccount(AccountType.School, package.ID.Value, dc));
            }

            return msgs;
        }
    }
}