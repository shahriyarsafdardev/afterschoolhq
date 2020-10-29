using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateAccountPackagesForSaasify_20201028
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var accounts = Account.Load(msgs: msgs, dc: dc);

                    accounts.ForEach(account =>
                    {
                        var accountPackage = AccountPackage.LoadByAccountIdByIsActive(account.ID.Value, true, msgs, dc);
                        //Allow Invites: 1
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowInvites, 1, null, msgs, dc);
                        //Allow Pay Later: 1
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowPayLater, 1, null, msgs, dc);
                        //Allow Payment Plans: 1
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowPaymentPlans, 1, null, msgs, dc);
                        //Allow Student Registration: 0
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowStudentRegistration, 0, null, msgs, dc);
                        //Allow Subscription Plans: 1
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowSubscriptionPlans, 1, null, msgs, dc);
                        //Manage Field Trips: 0
                        if(!account.HasFeature(FeatureType.ManageFieldTrips,msgs,dc))
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageFieldTrips, 0, null, msgs, dc);
                        //Manage Leagues: 0
                        if (!account.HasFeature(FeatureType.ManageLeagues, msgs, dc))
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageLeagues, 0, null, msgs, dc);
                        //Manage Learning Pods: 0
                        if (!account.HasFeature(FeatureType.ManageLearningPods, msgs, dc))
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageLearningPods, 0, null, msgs, dc);
                        //Manage Permission Slips: 0
                        if (!account.HasFeature(FeatureType.ManagePermissionSlips, msgs, dc))
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.ManagePermissionSlips, 0, null, msgs, dc);
                        //Student CRM: 1
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.StudentCRM, 1, null, msgs, dc);
                        //Text Messaging: 500
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.TextMessaging, 500, null, msgs, dc);
                        //Email Messaging: 500
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.EmailMessaging, 500, null, msgs, dc);
                    });
                   
                },
                uniqueIdenfier: "UpdateAccountPackagesForSaasify-20201028",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
