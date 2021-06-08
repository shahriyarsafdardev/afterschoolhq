using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantActivityServiceTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public RegistrantActivityServiceTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }

        [Fact]
        public void GetRegistrantActivityTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registration = RegistrationFactory.Create(account, registrant, msgs, dc);
            var actualRegistrantActivity = RegistrantActivityFactory.Create(activity, registrant, registration, msgs, dc);

            // Act    
            var expectedRegistrantActivity = RegistrantActivityService.GetRegistrantActivity(actualRegistrantActivity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expectedRegistrantActivity.ID, actual: actualRegistrantActivity.ID);
        }
    }
}
