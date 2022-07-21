using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class PackageService
    {
        public static PackageFeature AddFeature(this Package package, FeatureType featureType, decimal maxValue,
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

        public static void SetAsDefaultCustomerPackage(this Package package, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            msgs.AddRange(Package.Update(
                new Hashtable() { { "IsDefaultCustomerPackage", false } },
                new Hashtable(),
                Package.ObjectMap,
                dc));

            package.IsDefaultCustomerPackage = true;
            package.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            package.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(package.Save(dc));
        }

        public static Package GetDefaultCustomerPackage(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var package = Package.LoadByIsDefaultCustomerPackage(true, msgs, dc);

            if (package.IsEmpty())
            {
                package = Package.Load(start: 0, length: 1, msgs: msgs, dc: dc).First();
                package.SetAsDefaultCustomerPackage(null, msgs, dc);
            }

            return package;
        }
        
        public static Package CreateDefaultPackage(WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var package = new Package
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                Description = "Standard Package for AfterSchool customers",
                IsActive = true,
                IsAfterSchool = true,
                IsPopular = true,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                Name = "Standard Package",
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal(),
            };

            msgs.AddRange(package.SaveWithStripe(dc));

            if (msgs.HasNoErrors()) msgs.AddRange(CreateDefaultPackageFeatures(package, loggedInUser, dc));

            return package;
        }

        public static SystemMessages CreateDefaultPackageFeatures(Package package, WebUser loggedInUser,
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
                    _ => 1m,
                };

                package.AddFeature(item, maxValue, loggedInUser, msgs, dc);
            }

            return msgs;
        }

        private static Package GetFirstPackageWithFeatureFromPackages(FeatureType feature, List<Package> packages,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return packages.FirstOrDefault(package => package.HasFeature(feature, msgs, dc));
        }

        private static Package GetFirstPackageWithFeatureFromPackages(FeatureType feature,
            List<PackageFeaturePrice> packageFeaturePrices)
        {
            return packageFeaturePrices.FirstOrDefault(package => package.FeatureTypeIds.Contains((int) feature))
                ?.Package;
        }

        private static Package GetFirstPackageWithFeatureFromPackages(FeatureType feature, List<Package> packages,
            int featureCount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            foreach (var package in packages)
                if (package.HasFeature(feature, msgs, dc) &&
                    package.GetFeature(feature, msgs, dc).GetMaxValue() > featureCount)
                    return package;
            return null;
        }

        public static Package GetPackageWithFeatureLowestPrice(FeatureType feature, AccountPackage accountPackage,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var packages = Package.LoadByIsActive(true, msgs, dc);
            var packageFeaturePrices = packages
                .Where(package =>
                    package.Price > 0
                    && package.PricePerYear > 0
                    && package.ID != accountPackage.PackageId
                    && (package.Price > accountPackage.Price || package.PricePerYear > accountPackage.Price))
                .Select(package => new PackageFeaturePrice
                {
                    Package = package,
                    FeatureTypeIds = package.GetFeatures(msgs, dc).Select(x => (int) x.FeatureTypeId).ToList(),
                })
                .OrderBy(p => p.Package.Price).ToList();

            return GetPackageWithFeatureNextPriceTier(feature, accountPackage, packageFeaturePrices, msgs);
        }

        public static Package GetPackageWithFeatureNextPriceTier(FeatureType feature, AccountPackage accountPackage,
            List<PackageFeaturePrice> packageFeaturePrices, SystemMessages msgs)
        {
            var features = ((FeatureType[]) Enum.GetValues(typeof(FeatureType))).ToList();

            if (!feature.In(features))
            {
                msgs.Add("feature type does not exist", MessageType.Error);
                return null;
            }

            var paidPackages = packageFeaturePrices
                .Where(pfp =>
                    pfp.Package.IsActive
                    && pfp.Package.Price > 0
                    && pfp.Package.PricePerYear > 0
                    && pfp.Package.ID != accountPackage.PackageId
                    && pfp.Package.Price > accountPackage.Price)
                .OrderBy(p => p.Package.Price).ToList();

            var packageWithFeature = GetFirstPackageWithFeatureFromPackages(feature, paidPackages);

            return packageWithFeature.IsNotEmpty() ? packageWithFeature : null;
        }

        public static Package GetPackageWithFeatureMaximumAvailability(FeatureType feature, int featureCount,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var features = ((FeatureType[]) Enum.GetValues(typeof(FeatureType))).ToList();
            var packages = Package.LoadByIsActive(true, msgs, dc);
            var paidPackages = packages.Where(m => m.Price > 0 && m.PricePerYear > 0).OrderBy(p => p.Price).ToList();

            if (!feature.In(features))
            {
                msgs.Add("feature type does not exist", MessageType.Error);
                return null;
            }

            var packageWithFeature =
                GetFirstPackageWithFeatureFromPackages(feature, paidPackages, featureCount, msgs, dc);

            if (packageWithFeature.IsNotEmpty())
                return packageWithFeature;

            return null;
        }

        public static Package GetPackageWithAnyFeaturesLowestPrice(List<FeatureType> features,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var allFeatures = ((FeatureType[]) Enum.GetValues(typeof(FeatureType))).ToList();
            var packages = Package.LoadByIsActive(true, msgs, dc);
            packages.OrderBy(p => p.Price);

            foreach (var feature in features)
            {
                if (!feature.In(allFeatures))
                {
                    msgs.Add("feature type does not exist", MessageType.Error);
                    return null;
                }

                var packageWithFeature = GetFirstPackageWithFeatureFromPackages(feature, packages);

                if (packageWithFeature.IsNotEmpty())
                    return packageWithFeature;
            }

            return null;
        }
    }

    public class PackageFeaturePrice
    {
        public Package Package { get; set; }
        public List<int> FeatureTypeIds { get; set; }
    }
}