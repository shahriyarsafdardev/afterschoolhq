using System;
using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests.Factories
{
    public class PackageFactory
    {
        public static Package Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package
            {
                Name = "Package Test",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Package Create(decimal pricePerMonth, decimal pricePerYear, SystemMessages msgs,
            DatabaseConnection dc)
        {
            var obj = new Package
            {
                Name = "Package Test",
                IsActive = true,
                Price = pricePerMonth,
                PricePerYear = pricePerYear,
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Package CreateWithStripe(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package
            {
                Name = "Package Test",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.SaveWithStripe(dc));
            return obj;
        }

        public static Package CreateFreePackage(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package
            {
                Name = "Test Package Free",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.Save(dc));

            PackageFeatureFactory.Create(obj, FeatureType.AcceptACH, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AcceptPayments, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowApplications, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowInvites, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowPayLater, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowPaymentPlans, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowStudentRegistration, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowSubscriptionPlans, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Attendance, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.CustomUrl, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.EmailMessaging, 500.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Lottery, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageActivityGroups, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageContacts, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageExternalLeagues, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageFieldTrips, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageForms, 5.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLeagues, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLearningPods, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLocations, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManagePermissionSlips, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Participants, 500.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.PaymentsPercentageFee, 0.05.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.PaymentsPercentageFlatFee, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.StudentCRM, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TeamAccounts, 100.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TextMessaging, 500.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TrialDays, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.UseStripeConnectedAccount, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ViewInsights, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.WaitingList, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManualStripePayouts, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.FreeActivityLimit, 1.ToDecimal(), msgs, dc);

            return obj;
        }

        public static Package CreateFreePackageWithFlatFee(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package
            {
                Name = "Test Package Free",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.Save(dc));

            PackageFeatureFactory.Create(obj, FeatureType.AcceptACH, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AcceptPayments, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowApplications, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowInvites, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowPayLater, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowPaymentPlans, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowStudentRegistration, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowSubscriptionPlans, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Attendance, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.CustomUrl, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.EmailMessaging, 500.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Lottery, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageActivityGroups, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageContacts, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageExternalLeagues, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageFieldTrips, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageForms, 5.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLeagues, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLearningPods, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLocations, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManagePermissionSlips, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Participants, 500.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.PaymentsPercentageFee, 0.05.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.PaymentsPercentageFlatFee, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.StudentCRM, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TeamAccounts, 100.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TextMessaging, 500.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TrialDays, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.UseStripeConnectedAccount, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ViewInsights, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.WaitingList, 1.ToDecimal(), msgs, dc);

            return obj;
        }

        public static Package CreatePackage(string Name, string Description, decimal price, WebUser loggedInUser,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var package = new Package
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                Description = Description,
                IsActive = true,
                IsAfterSchool = true,
                IsPopular = true,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                Name = Name,
                Price = price,
                PricePerYear = price * 10,
            };

            msgs.AddRange(package.SaveWithStripe(dc));

            if (msgs.HasNoErrors())
                msgs.AddRange(CreatePackageFeatures(package, loggedInUser, dc));

            return package;
        }

        public static SystemMessages CreatePackageFeatures(Package package, WebUser loggedInUser,
            DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            foreach (var item in Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>())
            {
                var maxValue = item switch
                {
                    FeatureType.TrialDays => 120,
                    FeatureType.TeamAccounts => 1000,
                    FeatureType.Participants => 1000,
                    FeatureType.PaymentsPercentageFee => .05m, // means 5 percent
                    FeatureType.PaymentsPercentageFlatFee => .99m,
                    FeatureType.ManagePermissionSlips => 3,
                    _ => 1m,
                };

                AddFeature(package, item, maxValue, loggedInUser, msgs, dc);
            }

            return msgs;
        }

        public static PackageFeature AddFeature(Package package, FeatureType featureType, decimal maxValue,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var packageFeature = new PackageFeature
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                FeatureTypeId = featureType,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                Max = maxValue,
                PackageId = package.ID,
            };

            msgs.AddRange(packageFeature.Save(dc));

            return packageFeature;
        }

        public static Package CreateFreePackageWithUseStripeConnectedAccount(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package
            {
                Name = "Test Package Free",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.Save(dc));

            PackageFeatureFactory.Create(obj, FeatureType.AcceptACH, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AcceptPayments, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowApplications, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowInvites, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowPayLater, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowPaymentPlans, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowStudentRegistration, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AllowSubscriptionPlans, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Attendance, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.CustomUrl, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.EmailMessaging, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Lottery, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageActivityGroups, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageContacts, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageExternalLeagues, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageFieldTrips, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageForms, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLeagues, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLearningPods, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManageLocations, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ManagePermissionSlips, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Participants, 1000.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.PaymentsPercentageFee, 0.05.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.PaymentsPercentageFlatFee, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.StudentCRM, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TeamAccounts, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TextMessaging, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.TrialDays, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.UseStripeConnectedAccount, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ViewInsights, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.WaitingList, 1.ToDecimal(), msgs, dc);

            return obj;
        }
    }
}