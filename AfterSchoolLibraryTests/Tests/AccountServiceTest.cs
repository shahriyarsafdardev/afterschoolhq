using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

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
                createWithStripe: true,
                loggedInUser: loggedInUser,
                isAfterSchool: true,
                msgs: msgs,
                dc: dc);

            Assert.True(account.ID.HasValue);
        }

        [Fact]
        public void CreateSampleAccountTest()
        {
            var accountCount = Account.GetCount(msgs: msgs, dc: dc);
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var package = PackageService.CreateDefaultPackage(loggedInUser, msgs, dc);
            msgs.AddRange(AccountService.CreateSampleAccount(AccountType.School, package.ID.Value, dc));

            Assert.Equal(accountCount + 1, Account.GetCount(msgs: msgs, dc: dc));
        }

        [Theory]
        [InlineData("My Sample Account", "MySampleAccount")]
        [InlineData("My Sample Account Rocks Bud Boy", "MySampleAccountRocksBud")]
        [InlineData("My!@#$%^&*()-_+<>/,.?[]{}\\|'\":;~`", "My-_")]
        public void GenerateUniqueName(string input, string expectedOutput)
        {
            var output = AccountService.GenerateUniqueName(input, null, msgs, dc);

            Assert.Equal(expectedOutput, output);
        }
    }
}
