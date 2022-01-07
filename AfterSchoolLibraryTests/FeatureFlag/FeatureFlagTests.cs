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
            var featureFlagRepository = new FeatureFlagRepository(Database.LocalTestCredentials);
            var testFlagName = $"testFlag{DateTime.Now:o}";

            Assert.False(featureFlagRepository.IsEnabled(testFlagName));

            featureFlagRepository.Enable(testFlagName);

            Assert.True(featureFlagRepository.IsEnabled(testFlagName));
        }
    }
}