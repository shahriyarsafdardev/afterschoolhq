using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountServiceTest : BaseTest
    {
        public AccountServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateNewTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var accountOwner = Factories.WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var package = Factories.PackageFactory.Create(msgs, dc);
            var account = AccountService.CreateNew(
                orgName: "test org",
                email: accountOwner.Email,
                phone: "123-123-1234",
                isActive: false,
                accountType: AccountType.School,
                accountOwner: accountOwner,
                packageId: package.ID,
                packageBillingPeriod: BillingPeriodType.Month,
                loggedInUser: loggedInUser,
                isAfterSchool: true,
                msgs: msgs,
                dc: dc);

            Assert.True(account.ID.HasValue);
        }
    }
}
