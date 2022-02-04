using System;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using Xunit;

namespace AfterSchoolLibraryTests.FeatureFlag
{
    public class FeatureFlagTests
    {
        [Fact]
        public void EnableTest()
        {
            var featureFlagRepository = new FeatureFlagRepository(TestHelpers.TestDatabaseCredentials());
            var testFlagName = $"testFlag{DateTime.Now:u}";

            Assert.False(featureFlagRepository.IsEnabled(testFlagName));

            featureFlagRepository.Enable(testFlagName);

            Assert.True(featureFlagRepository.IsEnabled(testFlagName));
        }
    }
}