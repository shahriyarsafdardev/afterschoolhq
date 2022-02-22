using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Specialized;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountProfileServiceTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public AccountProfileServiceTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
        }

        [Fact]
        public void UpdateProfileServicesOfferedTest()
        {
            var accountProfile = AccountProfileFactory.Create(account, msgs, dc);
            var sampleData = new NameValueCollection();
            sampleData.Add("servicesOffered", "Test");

            AccountProfileService.UpdateProfileServicesOffered(accountProfile, "", "", sampleData, sampleData, sampleData, sampleData, "", "", "1",user, msgs, dc);

            Assert.True(accountProfile.GetProperties(AccountPropertyType.ProgramsAllDayDuringWorkWeek, msgs, dc).Count == 1);
        }
    }
}
