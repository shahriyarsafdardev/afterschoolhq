using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountServiceTest : BaseTest
    {
        public AccountServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(DisplayName = "CreateNewTest")]
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

        [Fact(DisplayName = "CreateSampleAccountTest")]
        public void CreateSampleAccountTest()
        {
            var accountCount = Account.GetCount(msgs: msgs, dc: dc);
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var package = PackageService.CreateDefaultPackage(loggedInUser, msgs, dc);
            msgs.AddRange(AccountService.CreateSampleAccount(AccountType.School, package.ID.Value, dc));

            Assert.Equal(accountCount + 1, Account.GetCount(msgs: msgs, dc: dc));
        }

        [Theory(DisplayName = "GenerateUniqueName")]
        [InlineData("My Sample Account", "MySampleAccount")]
        [InlineData("My Sample Account Rocks Bud Boy", "MySampleAccountRocksBud")]
        [InlineData("My!@#$%^&*()-_+<>/,.?[]{}\\|'\":;~`", "My-_")]
        public void GenerateUniqueName(string input, string expectedOutput)
        {
            var output = AccountService.GenerateUniqueName(input, null, msgs, dc);

            Assert.Equal(expectedOutput, output);
        }


        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// need to ask about sending emails and name parameters!
        /// </summary>
        [Fact(DisplayName = "Request access to a host account")]
        public void RequestAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var webuser = WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var webuser2 = WebUserFactory.CreateWithEmail("tes2t@test.com", msgs, dc);
            var hostaccount = AccountFactory.Create(webuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(webuser2, msgs, dc);
            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact(DisplayName = "Approve Provider Access to a Host Account", Skip = "Need to set all variables for email to work")]
        public void ApproveAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostUser = WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var visitorUser = WebUserFactory.CreateWithEmail("test2@test.com", msgs, dc);
            var hostaccount = AccountFactory.Create(hostUser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);
            var relationship1 = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
            var relationship2 = hostaccount.ApproveAccessToAccount(visitoraccount, loggedInUser, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact(DisplayName = "Host account denies request to visitor account")]
        public void DenyAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.CreateWithEmail("test@test", msgs, dc);
            var visitorUser = WebUserFactory.CreateWithEmail("test2@test", msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// Will request access to an account, deny it. Then list off the denied visitor accounts the host has
        /// The expected result will a list filled with one visitor account.
        /// TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>

        [Fact(DisplayName = "Host account gets list of denied visitor accounts")]
        public void GetDeniedVisitorAccountsTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.Create(true, msgs, dc);
            var visitorUser = WebUserFactory.Create(true, msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly, loggedInUser, msgs, dc);
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.Single(hostaccount.GetDeniedVisitorAccounts(visitoraccount.AccountTypeId, msgs, dc));
        }

        [Fact(DisplayName = "Get or create default form for account")]
        public void GetOrCreateDefaultForm()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var form = account.GetOrCreateDefaultForm(accountOwner, msgs, dc);

            Assert.NotNull(form);

        }

        [Theory(DisplayName = "Get leagues that a school has been invited to")]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void GetExternalLeaguesTest(int numberToCreate)
        {
            var leagueAccountOwner = WebUserFactory.Create(msgs, dc);
            var leagueAccount = AccountFactory.Create(leagueAccountOwner, msgs, dc);

            var invitedAccountOwner = WebUserFactory.Create(msgs, dc);
            var invitedAccount = AccountFactory.Create(invitedAccountOwner, msgs, dc);

            for (var i = 0; i < numberToCreate; i++)
            {
                var activityGroup = ActivityGroupFactory.Create(leagueAccount, ActivityGroupType.League, msgs, dc);
                msgs.AddRange(activityGroup.InviteAccount(invitedAccount, leagueAccountOwner, dc));
            }

            Assert.Equal(numberToCreate, invitedAccount.GetExternalLeagueCount(msgs, dc));

        }

        [Theory(DisplayName = "Load accounts by list of account type ids")]
        [InlineData(AccountType.School)]
        public void LoadByAccountTypeIdsTest(AccountType accountType)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, accountType, msgs, dc);
            var accountTypes = new List<AccountType>()
            {
                AccountType.ActivityProvider,
                AccountType.Registrant,
                AccountType.School,
                AccountType.SchoolDistrict
            };

            Assert.Single(Account.LoadByAccountTypeIds(accountTypes, null, null, msgs, dc));
        }

        [Theory(DisplayName = "Get Platform Flat Fee")]
        [InlineData(.99)]
        [InlineData(3)]
        public void GetPlatformFlatFeeTest(decimal platformFlatFee)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var accountPackageFeature = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);

            Assert.Equal(platformFlatFee, account.GetPlatformFlatFee(msgs, dc));
        }

        [Theory(DisplayName = "Get Platform Percent Fee")]
        [InlineData(.05)]
        [InlineData(.34)]
        public void GetPlatformPercentFeeTest(decimal platformPercentFee)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var accountPackageFeature = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentFee, msgs, dc);

            Assert.Equal(platformPercentFee, account.GetPlatformPercentFee(msgs, dc));
        }

        [Theory(DisplayName = "CalculatePlatformFeeAmountTest")]
        [InlineData(.99, .05, 20, 1.99)]
        [InlineData(.99, .05, 100, 5.99)]
        public void CalculatePlatformFeeAmountTest(decimal platformFlatFee, decimal platformPercentageFee, decimal totalAmountToCharge, decimal expectedPlatformFee)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var apfPlatformFeePercentage = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentageFee, msgs, dc);
            var apfPlatformFeeFlat = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);

            Assert.Equal(expectedPlatformFee, account.CalculatePlatformFeeAmount(totalAmountToCharge, msgs, dc));
        }

        [Theory(DisplayName = "CalculateSubscriptionPlatformFeeAmountTest")]
        [InlineData(.99, .05, 2, 0)]
        [InlineData(.99, .05, 20, 1.00)]
        [InlineData(.99, .05, 100, 5.00)]
        public void CalculateSubscriptionPlatformFeeAmountTest(decimal platformFlatFee, decimal platformPercentageFee, decimal totalAmountToCharge, decimal expectedPlatformFee)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var apfPlatformFeePercentage = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentageFee, msgs, dc);
            var apfPlatformFeeFlat = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);

            Assert.Equal(expectedPlatformFee, account.CalculateSubscriptionPlatformFeeAmount(totalAmountToCharge, msgs, dc));
        }

        [Theory(DisplayName = "GetSubscriptionPlatformFeePercentTest")]
        [InlineData(.99, .05, 2, 0)]
        [InlineData(.99, .05, 20, 0.006)]
        [InlineData(.99, .05, 100, 0.018)]
        public void GetSubscriptionPlatformFeePercentTest(decimal platformFlatFee, decimal platformPercentageFee, decimal costPerPeriod, decimal expectedPlatformPercentage)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var apfPlatformFeePercentage = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentageFee, msgs, dc);
            var apfPlatformFeeFlat = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(account, activity, SubscriptionPlanType.Subscription, costPerPeriod, msgs, dc);            
            Assert.Equal(expectedPlatformPercentage, account.GetSubscriptionPlatformFeePercent(subscriptionPlan, msgs, dc));
        }
    }
}
