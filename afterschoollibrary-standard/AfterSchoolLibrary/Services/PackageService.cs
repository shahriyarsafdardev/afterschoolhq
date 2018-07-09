using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class PackageService
    {
        public static Package CreateDefaultPackage(WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var package = new Package()
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
                PricePerYear = 0.ToDecimal()
            };

            msgs.AddRange(package.SaveWithStripe(dc));

            if (msgs.HasNoErrors())
            {
                foreach(var item in Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>())
                {
                    var maxValue = 1;

                    switch(item)
                    {
                        case FeatureType.Activities:
                            maxValue = 999999;
                            break;
                        case FeatureType.TrialDays:
                            maxValue = 120;
                            break;
                        case FeatureType.TeamAccounts:
                        case FeatureType.Participants:
                            maxValue = 999;
                            break;
                    }

                    package.AddFeature(item, maxValue, loggedInUser, msgs, dc);
                }
            }

            return package;
        }

        public static PackageFeature AddFeature(this Package package, FeatureType featureType, int maxValue, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var packageFeature = new PackageFeature()
            {
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                FeatureTypeId = featureType,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                Max = maxValue,
                PackageId = package.ID
            };

            msgs.AddRange(packageFeature.Save(dc));

            return packageFeature;

        }
    }
}
