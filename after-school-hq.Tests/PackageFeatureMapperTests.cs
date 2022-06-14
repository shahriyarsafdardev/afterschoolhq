using System.Linq;
using AfterSchool.Areas.Org.Controllers;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    [TestFixture]
    public class PackageFeatureMapperTests
    {
        [Test]
        public void FilterAndMapPackageFeaturesTest()
        {
            var featureTypes = new[]
            {
                (FeatureType.Attendance, 1m),
                ((FeatureType)6, 1m),
                (FeatureType.CustomUrl, 1m)
            };

            var result = AccountController.PackageFeatureMapper.FilterAndMapPackageFeatures(featureTypes).ToArray();
            
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.ElementAt(0).type, Is.EqualTo("Attendance"));
        }
    }
}