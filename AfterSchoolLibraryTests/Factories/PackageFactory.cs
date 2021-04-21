using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class PackageFactory
    {
        public static Package Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package()
            {
                Name = "Package Test",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Package Create(decimal pricePerMonth, decimal pricePerYear, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package()
            {
                Name = "Package Test",
                IsActive = true,
                Price = pricePerMonth,
                PricePerYear = pricePerYear
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Package CreateWithStripe(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package()
            {
                Name = "Package Test",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal()
            };
            msgs.AddRange(obj.SaveWithStripe(dc: dc));
            return obj;
        }

        public static Package CreateFreePackage(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package()
            {
                Name = "Test Package Free",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };
            msgs.AddRange(obj.Save(dc: dc));

            PackageFeatureFactory.Create(obj, FeatureType.AcceptACH, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.AcceptPayments, 1.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.Activities, 1000.ToDecimal(), msgs, dc);
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
            PackageFeatureFactory.Create(obj, FeatureType.UseStripeConnectedAccount, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.ViewInsights, 0.ToDecimal(), msgs, dc);
            PackageFeatureFactory.Create(obj, FeatureType.WaitingList, 1.ToDecimal(), msgs, dc);

            return obj;
        }
    }
}
