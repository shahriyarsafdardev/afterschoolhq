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
                    var accounts = Account.Load(msgs: amsgs, dc: adc);
                    var webUser = new WebUser { ID = 0 };

                    accounts.ForEach(account =>
                    {
                        var accountPackage = AccountPackage.LoadByAccountIdByIsActive(account.ID.Value, true, amsgs, adc);

                        if(accountPackage.IsNotEmpty())
                        {
                            //Allow Invites: 1
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowInvites, 1, webUser, amsgs, adc);
                            //Allow Pay Later: 1
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowPayLater, 1, webUser, amsgs, adc);
                            //Allow Payment Plans: 1
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowPaymentPlans, 1, webUser, amsgs, adc);
                            //Allow Student Registration: 0
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowStudentRegistration, 0, webUser, amsgs, adc);
                            //Allow Subscription Plans: 1
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.AllowSubscriptionPlans, 1, webUser, amsgs, adc);
                            //Manage Field Trips: 0
                            if (!account.HasFeature(FeatureType.ManageFieldTrips, amsgs, adc))
                                accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageFieldTrips, 0, webUser, amsgs, adc);
                            //Manage Leagues: 0
                            if (!account.HasFeature(FeatureType.ManageLeagues, amsgs, adc))
                                accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageLeagues, 0, webUser, amsgs, adc);
                            //Manage Learning Pods: 0
                            if (!account.HasFeature(FeatureType.ManageLearningPods, amsgs, adc))
                                accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageLearningPods, 0, webUser, amsgs, adc);
                            //Manage Permission Slips: 0
                            if (!account.HasFeature(FeatureType.ManagePermissionSlips, amsgs, adc))
                                accountPackage.AddOrUpdatePackageFeature(FeatureType.ManagePermissionSlips, 0, webUser, amsgs, adc);
                            //Student CRM: 1
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.StudentCRM, 1, webUser, amsgs, adc);
                            //Text Messaging: 500
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.TextMessaging, 500, webUser, amsgs, adc);
                            //Email Messaging: 500
                            accountPackage.AddOrUpdatePackageFeature(FeatureType.EmailMessaging, 500, webUser, amsgs, adc);
                        }
                      
                    });
                   
                },
                uniqueIdenfier: "UpdateAccountPackagesForSaasify-20201028",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
