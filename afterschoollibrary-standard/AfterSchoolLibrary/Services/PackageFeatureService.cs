using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class PackageFeatureService
    {
        public static PackageFeature AddPackageFeature(int? PackageId, FeatureType featureTypeId, decimal maxValue, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var packageFeature = new PackageFeature();

            packageFeature.PackageId = PackageId;
            packageFeature.FeatureTypeId = featureTypeId;
            packageFeature.CreateDate = DateTime.UtcNow;
            packageFeature.CreatedBy = loggedInUser?.ID.IfEmpty(0);
            packageFeature.Max = maxValue;
            packageFeature.LastModifiedDate = DateTime.UtcNow;
            packageFeature.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);

            msgs.AddRange(packageFeature.Save(dc: dc));
            return packageFeature;
        }
    }
}
