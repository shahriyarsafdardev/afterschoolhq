using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    class PackageFeatureFactory
    {
        public static PackageFeature Create(Package package, FeatureType featureType, decimal max, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new PackageFeature()
            {
                PackageId = package.ID.Value,
                FeatureTypeId = featureType,
                Max = max,
            };

            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
