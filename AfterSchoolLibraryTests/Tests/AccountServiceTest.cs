using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using System;
using AfterSchoolLibrary.Billing;
using BasicLibrary;
using AfterSchool.Models;
using System.Linq;
using Amazon.S3.Model;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountServiceTest : BaseTest
    {
        private WebUser user { get; set; }
        private Account account { get; set; }

        public AccountServiceTest(ITestOutputHelper output) : base(output)
        {
            user = WebUserFactory.Create(msgs, dc);
            account = AccountFactory.Create(user, msgs, dc);
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
                timeZone: null,
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
        [Fact(DisplayName = "Approve Provider Access to a Host Account")]
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

            Assert.Single(Account.LoadByAccountTypeIds(accountTypes, false, null, null, msgs, dc));
        }

        [Theory(DisplayName = "Get Platform Flat Fee")]
        [InlineData(.99, 1, .99)]
        [InlineData(3, 1, 3)]
        [InlineData(.99, 0, .99)]
        [InlineData(3, 0, 3)]
        public void GetPlatformFlatFeeTest(decimal platformFlatFee, decimal stripeConnectedAccountValue, decimal expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, stripeConnectedAccountValue, msgs, dc);
            
            var fee = account.GetPlatformConvenienceFlatFee(msgs, dc);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, fee);
        }

        [Theory(DisplayName = "Get Platform Percent Fee")]
        [InlineData(.05, 1, .05)]
        [InlineData(.34, 1, .34)]
        [InlineData(.05, 0, 0.05)]
        [InlineData(.34, 0, 0.34)]
        public void GetPlatformPercentFeeTest(decimal platformPercentFee, decimal stripeConnectedAccountValue, decimal expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentFee, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, stripeConnectedAccountValue, msgs, dc);

            var fee = account.GetPlatformConveniencePercentFee(msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, fee);
        }

        /// <summary>
        /// This method is not being used anymore, but I kept it as it might be needed.
        /// </summary>
        /// <param name="tuitionFee"></param>
        /// <param name="platformFlatFee"></param>
        /// <param name="platformPercentageFee"></param>
        /// <param name="expectedPlatformFee"></param>
        [Theory(DisplayName = "Calculate Platform Flat Fee Amount")]        
        [InlineData(100, 0.99, 0.05, 5.99)]
        [InlineData(100, 0, 0.05, 5.0)]
        public void CalculatePlatformFeeAmountTest(decimal tuitionFee, decimal platformFlatFee, decimal platformPercentageFee, decimal expectedPlatformFee)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentageFee, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, 1, msgs, dc);

            var fee = account.CalculatePlatformConvenienceFeeAmount(tuitionFee, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedPlatformFee, fee);
        }

        /// <summary>
        /// should calculate the percentage that AHQ gets when processing a subscription payment
        /// Should be equal to 
        /// feeAmount / totalCost
        /// to get the final percentage
        /// </summary>
        /// <param name="platformFlatFee"></param>
        /// <param name="platformPercentageFee"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="expectedPlatformPercentage"></param>
        [Theory]
        [InlineData(.99, .05, 2, 1, 0.5)]
        [InlineData(.99, .05, 20, 1, 0.1489)]
        [InlineData(.99, .05, 100, 1, 0.0991)]
        [InlineData(.99, .05, 2, 0, 0.5)]
        [InlineData(.99, .05, 20, 0, 0.1489)]
        [InlineData(.99, .05, 100, 0, 0.0991)]
        public void GetSubscriptionPlatformFeePercentTest(decimal platformFlatFee, decimal platformPercentageFee, decimal costPerPeriod, int stripeConnectedAccountValue, decimal expectedPlatformPercentage)
        {
            var accountOwner = WebUserFactory.Create(msgs: msgs, dc: dc);
            var account = AccountFactory.Create(user: accountOwner, msgs: msgs, dc: dc);
            var package = PackageFactory.Create(msgs: msgs, dc: dc);
            var accountPackage = AccountPackageFactory.Create(account: account, package: package, msgs: msgs, dc: dc);
            
            AccountPackageFeatureFactory.Create(accountPackage: accountPackage, featureType: FeatureType.PaymentsPercentageFee, value: platformPercentageFee, msgs: msgs, dc: dc);
            AccountPackageFeatureFactory.Create(accountPackage: accountPackage, featureType: FeatureType.PaymentsPercentageFlatFee, value: platformFlatFee, msgs: msgs, dc: dc);
            AccountPackageFeatureFactory.Create(accountPackage: accountPackage, featureType: FeatureType.UseStripeConnectedAccount, value: stripeConnectedAccountValue, msgs: msgs, dc: dc);
            
            var activity = ActivityFactory.Create(account: account, msgs: msgs, dc: dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: account, activity: activity, planType: SubscriptionPlanType.Subscription, costPerPeriod: costPerPeriod, msgs: msgs, dc: dc);
            var subscriptionPlanFeePercent = account.GetSubscriptionPlatformFeePercent(plan: subscriptionPlan, paymentType: PaymentType.CreditCard, msgs: msgs, dc: dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedPlatformPercentage, subscriptionPlanFeePercent);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void CanProcessPaymentsTest(bool addPaymentFeatureToAccount, bool expectedResult)
        {
            var accountOwner = WebUserFactory.Create(isActive: true, msgs: msgs, dc: dc);
            var account = AccountFactory.CreateAccount(user: accountOwner, accountType: AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs: msgs, dc: dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.AcceptPayments, 1, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage: accountPackage, featureType: FeatureType.PaymentsPercentageFee, value: 0.05m, msgs: msgs, dc: dc);
            AccountPackageFeatureFactory.Create(accountPackage: accountPackage, featureType: FeatureType.PaymentsPercentageFlatFee, value: 0.99m, msgs: msgs, dc: dc);
            AccountPackageFeatureFactory.Create(accountPackage: accountPackage, featureType: FeatureType.UseStripeConnectedAccount, value: 1, msgs: msgs, dc: dc);

            if (addPaymentFeatureToAccount)
                AccountFactory.CreateStripeAccountAndCustomer(account: account, msgs: msgs, dc: dc);

            var canProcessPayments = account.CanProcessPayments(msgs, dc);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, canProcessPayments);
        }

        [Theory(DisplayName = "HasAccountPackageFeature")]
        [InlineData(FeatureType.AcceptPayments, 10, true)]
        [InlineData(FeatureType.AcceptPayments, 1, true)]
        [InlineData(FeatureType.AcceptPayments, 0, false)]
        public void HasAccountPackageFeature(FeatureType featureType, decimal value, bool expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var accountPackageFeature = AccountPackageFeatureFactory.Create(accountPackage, featureType, value, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, account.HasAccountPackageFeature(featureType, msgs, dc));
        }

        [Fact]
        public void CreateStripeCustomAccountTestIndividual()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            AccountFactory.CreateStripeAccountAndCustomer(account: account, msgs: msgs, dc: dc);
            var stripeAccount = account.GetStripeCustomAccount(msgs);

            stripeAccount.Requirements.CurrentlyDue.ForEach(field =>
            {
                switch ((string)field)
                {
                    case "legal_entity.personal_id_number":
                        msgs.Add("It has been determined your Full SSN required in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                    case "legal_entity.verification.document":
                        msgs.Add("It has been determined that a scan of a Valid Driver's license or Passport is required to be uploaded in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                    default:
                        msgs.Add($"{field}", MessageType.Warning);
                        break;
                }
            });

            Assert.True(msgs.HasNoErrors() && msgs.HasNo(new MessageType[] { MessageType.Warning }));
        }

        [Fact]
        public void CreateStripeCustomAccountTestCompany()
        {
            var createOptions = AccountFactory.GetAccountCreateOptions();
            var personCreateOptions = AccountFactory.GetPersonCreateOptions();

            var stripeAccount = StripeHelper.CreateCustomAccount(createOptions, msgs);
            var accountCreator = StripeHelper.CreatePerson(stripeAccount.Id, personCreateOptions, msgs);

            stripeAccount = StripeHelper.GetCustomAccount(stripeAccount.Id, msgs);
            stripeAccount.Requirements.CurrentlyDue.ForEach(field =>
            {
                switch ((string)field)
                {
                    case "legal_entity.personal_id_number":
                        msgs.Add("It has been determined your Full SSN required in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                    case "legal_entity.verification.document":
                        msgs.Add("It has been determined that a scan of a Valid Driver's license or Passport is required to be uploaded in order to process payments. Please update your information.", MessageType.Warning);
                        break;
                    default:
                        msgs.Add($"{field}", MessageType.Warning);
                        break;
                }
            });

            Assert.True(msgs.HasNoErrors() && msgs.HasNo(new MessageType[] { MessageType.Warning }));
        }

        [Theory(DisplayName = "GetBillerAccountTransactionsTest")]
        [InlineData(1)]
        [InlineData(5)]

        public void GetBillerAccountTransactionsTest(int expectedNumberOfTransactions)
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var parentUser = Factories.WebUserFactory.Create(msgs, dc);
            var billeraccount = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payerAccount = Factories.AccountFactory.CreateWithFreePackage(parentUser, AccountType.Registrant, msgs, dc);

            var transactionIds = new List<int>();

            for (int i = 0; i < expectedNumberOfTransactions; i++)
            {
                var transaction = Factories.TransactionFactory.Create(billeraccount, payerAccount, PaymentType.ACH, msgs, dc);
                transactionIds.Add(transaction.ID.Value);
            }

            var actualNumberOfTransactions = billeraccount.GetBillerAccountTransactions(transactionIds, msgs, dc);

            Assert.Equal(expectedNumberOfTransactions, actualNumberOfTransactions.Count);
        }

        [Theory(DisplayName = "GetPayerAccountTransactionsTest")]
        [InlineData(1)]
        [InlineData(5)]

        public void GetPayerAccountTransactionsTest(int expectedNumberOfTransactions)
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var parentUser = Factories.WebUserFactory.Create(msgs, dc);
            var billeraccount = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payerAccount = Factories.AccountFactory.CreateWithFreePackage(parentUser, AccountType.Registrant, msgs, dc);

            var transactionIds = new List<int>();

            for (int i = 0; i < expectedNumberOfTransactions; i++)
            {
                var transaction = Factories.TransactionFactory.Create(billeraccount, payerAccount, PaymentType.ACH, msgs, dc);
                transactionIds.Add(transaction.ID.Value);
            }

            var actualNumberOfTransactions = payerAccount.GetPayerAccountTransactions(transactionIds, msgs, dc);

            Assert.Equal(expectedNumberOfTransactions, actualNumberOfTransactions.Count);
        }

        [Fact(DisplayName = "GetBillerTransactionTest")]
        public void GetBillerTransactionTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var payingUser = Factories.WebUserFactory.Create(msgs, dc);
            var billingAccount = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payingAccount = Factories.AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var expectedTransaction = Factories.TransactionFactory.Create(billingAccount, payingAccount, PaymentType.ACH, msgs, dc);
            var actualTransaction = billingAccount.GetBillerTransaction(expectedTransaction.Uid, msgs, dc);

            Assert.Equal(expectedTransaction.ID, actualTransaction.ID);
        }


        [Fact(DisplayName = "GetPayerTransactionTest")]
        public void GetPayerTransactionTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var payingUser = Factories.WebUserFactory.Create(msgs, dc);
            var billingAccount = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payingAccount = Factories.AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var expectedTransaction = Factories.TransactionFactory.Create(billingAccount, payingAccount, PaymentType.ACH, msgs, dc);
            var actualTransaction = payingAccount.GetPayerTransaction(expectedTransaction.Uid, msgs, dc);

            Assert.Equal(expectedTransaction.ID, actualTransaction.ID);
        }

        [Theory(DisplayName = "GetAllActivitiesForAccountTest")]
        [InlineData(1)]
        [InlineData(5)]
        public void GetAllActivitiesForAccountTest(int expectedNumberOfActivities)
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var expectedActivities = new List<Activity>();

            for (int i = 0; i < expectedNumberOfActivities; i++)
            {
                var expectedActivity = Factories.ActivityFactory.Create(account, msgs, dc);
                expectedActivities.Add(expectedActivity);
            }

            var actualActivities = account.GetAllActivitiesForAccount(msgs, dc);

            Assert.Equal(expectedActivities.Count, actualActivities.Count);
        }

        [Fact]
        public void GetActivityScheduleTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);

            // Act
            var expected = account.GetActivitySchedule(schedule.ID.Value, msgs, dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: schedule.ID);
            Assert.Equal(expected: expected.Name, actual: schedule.Name);
        }

        [Fact]
        public void GetActivitySchedulesTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);
            var schedule2 = ActivityScheduleFactory.Create(account, msgs, dc);

            // Act
            var activitySchedules = account.GetActivitySchedules(msgs, dc);

            // Assert
            Assert.Equal(expected: 2, actual: activitySchedules.Count);
            Assert.Equal(expected: schedule.ID, actual: activitySchedules[0].ID);
            Assert.Equal(expected: schedule2.ID, actual: activitySchedules[1].ID);
        }

        [Fact]
        public void GetActivityMessageTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account: account, msgs: msgs, dc: dc);
            var activityMessage = ActivityMessageFactory.Create(account, activity, msgs, dc);

            // Act
            var expected = account.GetActivityMessage(Uid: activityMessage.Uid, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: activityMessage.ID);
            Assert.Equal(expected: expected.Uid, actual: activityMessage.Uid);
        }

        [Fact]
        public void GetActivityByIDTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);

            // Act
            var expected = account.GetActivity(activity.ID.Value, msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(expected: expected.ID, actual: activity.ID);
            Assert.Equal(expected: expected.Uid, actual: activity.Uid);
            Assert.Equal(expected: expected.Name, actual: activity.Name);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByIDFailingTest()
        {
            // Act
            var expected = account.GetActivity(84930248, msgs, dc);

            // Assert
            Assert.Null(expected);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByUIDTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);

            // Act
            var expected = account.GetActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: activity.ID);
            Assert.Equal(expected: expected.Uid, actual: activity.Uid);
            Assert.Equal(expected: expected.Name, actual: activity.Name);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByUIDFailingTest()
        {
            // Act
            var expected = account.GetActivity(Guid.NewGuid(), msgs, dc);

            // Assert
            Assert.Null(expected);
            Assert.True(msgs.HasNoErrors());            
        }

        [Fact]
        public void GetActivityByLocationHostAccountTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expectedById = schoolAccount.GetActivity(activity.ID.Value, msgs, dc);
            var expectedByUid = schoolAccount.GetActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expectedById.ID, actual: activity.ID);
            Assert.Equal(expected: expectedById.Uid, actual: activity.Uid);
            Assert.Equal(expected: expectedById.Name, actual: activity.Name);

            Assert.Equal(expected: expectedByUid.ID, actual: activity.ID);
            Assert.Equal(expected: expectedByUid.Uid, actual: activity.Uid);
            Assert.Equal(expected: expectedByUid.Name, actual: activity.Name);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByLocationHostAccountFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expectedById = schoolAccount.GetActivity(activity.ID.Value, msgs, dc);
            var expectedByUid = schoolAccount.GetActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Null(expectedById);
            Assert.Null(expectedByUid);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetHostedActivityByIdTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expectedById = schoolAccount.GetHostedActivity(activity.ID.Value, msgs, dc);
            var expectedByUid = schoolAccount.GetHostedActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expectedById.ID, actual: activity.ID);
            Assert.Equal(expected: expectedById.Uid, actual: activity.Uid);
            Assert.Equal(expected: expectedById.Name, actual: activity.Name);

            Assert.Equal(expected: expectedByUid.ID, actual: activity.ID);
            Assert.Equal(expected: expectedByUid.Uid, actual: activity.Uid);
            Assert.Equal(expected: expectedByUid.Name, actual: activity.Name);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetHostedActivityByIdFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expectedById = schoolAccount.GetHostedActivity(94378, msgs, dc);
            var expectedByUid = schoolAccount.GetHostedActivity(Guid.NewGuid(), msgs, dc);

            // Assert
            Assert.Null(expectedById);
            Assert.Null(expectedByUid);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetHostedActivityByActivityTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expected = schoolAccount.GetHostedActivity(activity, msgs, dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: activity.ID);
            Assert.Equal(expected: expected.Uid, actual: activity.Uid);
            Assert.Equal(expected: expected.Name, actual: activity.Name);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetHostedActivityByActivityFailingTest()
        {
            // Arrange
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);

            // Act
            var expected = schoolAccount.GetHostedActivity(null, msgs, dc);

            // Assert
            Assert.Null(expected);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetExternallyProvidedActivityTest()
        {
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var expected = ActivityFactory.Create(schoolAccount, location, msgs, dc);
            var expected2 = ActivityFactory.Create(account, location, msgs, dc);

            expected.ProviderAccountId = account.ID.Value;
            msgs.AddRange(expected.Save(dc));

            var actual = account.GetExternallyProvidedActivity(expected.Uid, msgs, dc);
            var actual2 = account.GetExternallyProvidedActivity(expected.ID.Value, msgs, dc);
            var actual3 = account.GetExternallyProvidedActivity(expected2.ID.Value, msgs, dc);

            Assert.NotNull(actual);
            Assert.NotNull(actual2);
            Assert.Equal(expected.Uid, actual.Uid);
            Assert.Equal(expected.ID, actual.ID);
            Assert.Null(actual3);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivitiesTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var activity2 = ActivityFactory.Create(account, msgs, dc);

            // Act
            var activities = account.GetActivities(msgs, dc);

            // Assert
            Assert.Equal(expected: 2, actual: activities.Count);
            Assert.Equal(expected: activity.ID, actual: activities[0].ID);
            Assert.Equal(expected: activity2.ID, actual: activities[1].ID);
        }

        [Theory(DisplayName = "GetActivitiesByIds Test")]
        [InlineData(5)]
        public void GetActivitiesByIdsTest(int n)
        {
            // Arrange
            List<int> activityIds = new List<int>();
            for (var i = 0; i < n; i++)
            {
                var activity = ActivityFactory.Create(account, msgs, dc);
                activityIds.Add(activity.ID.Value);
            }

            // Act
            var expected = account.GetActivities(ActivityIds: activityIds, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.Count, actual: n);            
        }

        [Fact]
        public void GetActivityGroupTest()
        {
            // Arrange
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            var expected = account.GetActivityGroup(Uid: activityGroup.Uid, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: activityGroup.ID);
            Assert.Equal(expected: expected.Uid, actual: activityGroup.Uid);
            Assert.Equal(expected: expected.Name, actual: activityGroup.Name);
        }

        [Theory(DisplayName = "GetActivityGroups Test")]
        [InlineData(5)]
        public void GetActivityGroupsTest(int n)
        {
            // Arrange
            List<int> activityGroupIds = new List<int>();
            for (var i = 0; i < n; i++)
            {
                var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
                activityGroupIds.Add(activityGroup.ID.Value);
            }

            // Act
            var expected = account.GetActivityGroups(activityGroupIds: activityGroupIds, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.Count, actual: n);            
        }

        [Fact]
        public void GetPublishedActivitiesTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var period = RegistrationPeriodFactory.Create(account, msgs, dc);

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);

            // Act 
            var activities = account.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: activity.ID.Value, actual: activities.First().ID.Value);
        }

        [Fact]
        public void GetPublishedActivitiesFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var period = RegistrationPeriodFactory.Create(account, msgs, dc);

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(user.ID.Value, dc));

            // Act 
            var activities = account.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Empty(activities);
            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetPublishedOpenActivitiesTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var activity2 = ActivityFactory.Create(account, msgs, dc);
            var activity3 = ActivityFactory.Create(account, msgs, dc);
            var activity4 = ActivityFactory.Create(account, msgs, dc);
            var period = RegistrationPeriodFactory.Create(account, msgs, dc);
            var period2 = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(12), msgs, dc);
            var period3 = RegistrationPeriodFactory.Create(account, DateTime.UtcNow.AddDays(-12), DateTime.UtcNow.AddDays(-1), msgs, dc);
            var period4 = RegistrationPeriodFactory.Create(account, msgs, dc);

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));
            msgs.AddRange(period2.AddActivity(activity2, true, user.ID.Value, dc));
            msgs.AddRange(period3.AddActivity(activity3, true, user.ID.Value, dc));
            msgs.AddRange(period4.AddActivity(activity4, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);
            activity2.Publish(user, msgs, dc);
            activity3.Publish(user, msgs, dc);

            // Act 
            var activities = account.GetPublishedOpenedActivities(msgs: msgs, dc: dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: activity.ID.Value, actual: activities.First().ID.Value);
        }

        [Fact]
        public void GetPublishedActivitiesSchoolAccountTest()
        {
            // Arrange
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var activity = ActivityFactory.Create(schoolAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            activity.ProviderAccountId = account.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);

            // Act 
            var activities = account.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: activity.ID.Value, actual: activities.First().ID.Value);
        }

        [Fact]
        public void GetPublishedActivitiesSchoolAccountFailingTest()
        {
            // Arrange
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var activity = ActivityFactory.Create(schoolAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            activity.ProviderAccountId = account.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(user.ID.Value, dc));

            // Act 
            var activities = account.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Empty(activities);
            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetPublishedActivitiesInLocationsTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(account, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);

            // Act 
            var activities = account.GetPublishedActivitiesInLocations(location.ID.Value.AsList(), msgs, dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expected: activity.ID.Value, actual: activities.First().ID.Value);
        }

        [Fact]
        public void GetPublishedActivitiesInLocationsFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(account, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, user.ID.Value, dc));

            activity.Publish(user, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(user.ID.Value, dc));

            // Act 
            var activities = account.GetPublishedActivitiesInLocations(location.ID.Value.AsList(), msgs, dc);

            // Assert
            Assert.Empty(activities);
            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetRegistrantTest()
        {
            // Arrange
            var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc);

            // Act                
            var expected = account.GetRegistrant(Uid: registrant.Uid, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: registrant.ID);
            Assert.Equal(expected: expected.Uid, actual: registrant.Uid);
            Assert.Equal(expected: expected.Email, actual: registrant.Email);            
        }

        [Fact]
        public void GetPayoutTest()
        {
            // Arrange
            var payout = PayoutFactory.Create(account, msgs, dc);

            // Act                
            var expected = account.GetPayout(Uid: payout.Uid, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: payout.ID);
            Assert.Equal(expected: expected.Uid, actual: payout.Uid);
            Assert.Equal(expected: expected.AccountId, actual: payout.AccountId);
            Assert.Equal(expected: expected.Amount, actual: payout.Amount);
            Assert.Equal(expected: expected.PayoutStatusId, actual: payout.PayoutStatusId);            
        }

        [Fact]
        public void GetCouponTest()
        {
            // Arrange
            var coupon = CouponFactory.Create(account, msgs, dc);

            // Act 
            var expected = account.GetCoupon(coupon.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: coupon.ID);
            Assert.Equal(expected: expected.Uid, actual: coupon.Uid);            
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void GetCouponsTest(int expectedNumberOfCoupons)
        {
            // Arrange
            var expected = new List<Coupon>();

            for (int i = 0; i < expectedNumberOfCoupons; i++)
            {
                var expectedCoupon = CouponFactory.Create(account, msgs, dc);
                expected.Add(expectedCoupon);
            }

            // Act 
            var coupons = account.GetCoupons(msgs, dc);

            // Assert
            Assert.Equal(expected: expected.Count, actual: coupons.Count);            
        }

        [Fact]
        public void GetRefundRequestTest()
        {
            // Arrange
            var payingUser = WebUserFactory.Create(msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var transaction = TransactionFactory.Create(account, payingAccount, PaymentType.ACH, msgs, dc);
            var expectedRefundRequest = RefundRequestFactory.Create(transaction.Amount.Value, transaction, account, "reason", payingUser, msgs, dc);

            // Act 
            var actualRefundRequest = account.GetRefundRequest(expectedRefundRequest.Uid, msgs, dc);

            // Assert
            Assert.Equal(expectedRefundRequest.ID, actualRefundRequest.ID);            
        }

        [Fact]
        public void GetRefundRequestsTest()
        {
            // Arrange
            var payingUser = WebUserFactory.Create(msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var transaction = TransactionFactory.Create(account, payingAccount, PaymentType.ACH, msgs, dc);
            var expectedRefundRequest = RefundRequestFactory.Create(transaction.Amount.Value, transaction, account, "reason", payingUser, msgs, dc);
            var expectedRefundRequest2 = RefundRequestFactory.Create(transaction.Amount.Value, transaction, account, "reason", payingUser, msgs, dc);

            // Act 
            var actualRefundRequest = account.GetRefundRequests(msgs, dc);

            // Assert
            Assert.Equal(2, actualRefundRequest.Count);            
        }

        [Fact]
        public void GetAccountPackageTest()
        {
            // Arrange
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act                
            var expected = account.GetAccountPackage(Uid: accountPackage.Uid, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: accountPackage.ID);
            Assert.Equal(expected: expected.Uid, actual: accountPackage.Uid);
            Assert.Equal(expected: expected.Price, actual: accountPackage.Price);            
        }

        [Fact]
        public void GetAccountRelationshipTest()
        {            
            // Arrange
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);
            var accountRelationship = AccountRelationshipService.CreateNew(
                hostAccount, 
                visitorAccount, 
                ApprovalStatus.Approved, 
                HostPermissionType.Edit,
                true,
                false,
                user, 
                msgs, 
                dc);
                
            // Act                
            var expected = hostAccount.GetAccountRelationship(accountRelationshipId: accountRelationship.ID.Value, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: accountRelationship.ID);
        }

        [Fact]
        public void GetFormTest()
        {
            // Arrange                
            var form = FormFactory.Create(account, msgs, dc);

            // Act                
            var expected = account.GetForm(Id: form.ID.Value, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: form.ID);
        }

        [Fact]
        public void GetAccountWebUserTest()
        {
            // Arrange                
            var webUser = AccountWebUserFactory.Create(account, user, UserType.Student, true, msgs, dc);

            // Act                
            var expected = account.GetAccountWebUser(webUserId: user.ID.Value, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: webUser.ID);
        }

        [Fact(DisplayName = "Get Account Owner test")]
        public void GetAccountOwnerTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            var GetOwner = AccountService.GetAccountOwner(account, msgs, dc);

            // Assert
            Assert.NotNull(GetOwner);            
        }

        [Fact(DisplayName = "HasFeature test")]
        public void HasFeatureTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateWithFreePackage(accountOwner, AccountType.Church, msgs, dc);

            // Act
            var AccountWithFeature = account.HasFeature(FeatureType.AcceptACH, msgs, dc);

            // Assert
            Assert.True(AccountWithFeature);            
        }

        [Fact(DisplayName = "HasFeature False Case Test")]
        public void HasFeatureEmptyAccountTest()
        {
            // Arrange
            var accountWithNoPackage = AccountFactory.Create(user, msgs, dc);
                
            // Act
            var accountWithFeature = AccountService.HasFeature(account: null, FeatureType.AcceptACH, msgs, dc);
            var accountWithNoPackageFeature = AccountService.HasFeature(account: accountWithNoPackage, FeatureType.AcceptACH, msgs, dc);
                
            // Assert
            Assert.False(accountWithFeature);
            Assert.False(accountWithNoPackageFeature);
        }

        [Fact(DisplayName = "GetActiveAccountPackage test")]
        public void GetActiveAccountPackageTest()
        {
            // Arrange
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.GetActiveAccountPackage(msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(package.Name, expected.PackageName);
        }

        [Fact(DisplayName = "GetActiveAccountPackage test 2")]
        public void GetActiveAccountPackageEmptyAccountTest()
        {
            // Arrange & Act
            var expected = AccountService.GetActiveAccountPackage(account: null, msgs: msgs, dc: dc);

            // Assert
            Assert.Null(expected);
        }

        [Fact(DisplayName = "GetCheckPaymentAccount test")]
        public void GetCheckPaymentAccountTest()
        {
            // Arrange
            var checkPaymentAccount = CheckPaymentAccountFactory.Create(account, msgs, dc);

            // Act
            var expected = account.GetCheckPaymentAccount(msgs: msgs, dc: dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(checkPaymentAccount.Name, expected.Name);
            Assert.Equal(checkPaymentAccount.LegalEntityTypeId, expected.LegalEntityTypeId);
        }

        [Fact(DisplayName = "GetCheckPaymentAccount test 2")]
        public void GetCheckPaymentAccountEmptyAccountTest()
        {
            // Arrange & Act
            var expected = AccountService.GetCheckPaymentAccount(account: null, msgs: msgs, dc: dc);

            // Assert
            Assert.Null(expected);
        }

        [Fact(DisplayName = "GetActivityCount test")]
        public void GetActivityCountTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
            var GetActivityCount = AccountService.GetActivityCount(account, msgs, dc);

            // Assert
            Assert.True(GetActivityCount > 0);
        }

        [Fact(DisplayName = "GetActiveAndPublishedActivityCount test")]
        public void GetActiveAndPublishedActivityCountTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schoolAccountOwner = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolAccountOwner, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, msgs, dc);
            addactivity.Publish(accountOwner, msgs, dc);

            var schoolActivity = ActivityFactory.Create(schoolAccount, msgs, dc);
            schoolActivity.SetProvider(account, schoolAccountOwner, msgs, dc);
            schoolActivity.Publish(schoolAccountOwner, msgs, dc);

            var schoolActivity2 = ActivityFactory.Create(schoolAccount, msgs, dc);
            schoolActivity2.Publish(schoolAccountOwner, msgs, dc);

            var GetActivityCount = AccountService.GetActiveAndPublishedActivityCount(account, true, msgs, dc);

            // Assert
            Assert.Equal(2, GetActivityCount);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact(DisplayName = "GetAllActiveAndNonPublishedActivities test")]
        public void GetAllActiveAndNonPublishedActivitiesTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            for (int i = 1; i <= 5; i++)
            {
                var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);                    
                if (i%2 == 0)
                {
                    addactivity.Publish(accountOwner, msgs, dc);
                }
            }
                                
            var GetActivities = AccountService.GetAllActiveAndNonPublishedActivities(account, msgs, dc);

            // Assert
            Assert.True(GetActivities.Count() == 3);            
        }

        [Fact(DisplayName = "GetActiveAndNonPublishedActivityCount test")]
        public void GetActiveAndNonPublishedActivityCountTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
            var GetActivityCount = AccountService.GetActiveAndNonPublishedActivityCount(account, msgs, dc);

            // Assert
            Assert.True(GetActivityCount > 0);
        }

        [Fact(DisplayName = "GetUnpublishActivityCount test")]
        public void GetUnpublishActivityCountTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
            var GetActivityCount = AccountService.GetActiveAndNonPublishedActivityCount(account, msgs, dc);

            // Assert
            Assert.True(GetActivityCount > 0);
        }

        [Fact(DisplayName = "HasActivities test")]
        public void HasActivitiesTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateWithFreePackage(accountOwner, AccountType.Church, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
            var HasActivity = AccountService.HasActivities(account, msgs, dc);

            // Assert
            Assert.True(HasActivity);
        }

        [Fact(DisplayName = "HasActiveActivities test")]
        public void HasActiveActivitiesTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
            addactivity.Publish(accountOwner, msgs, dc);
            var HasActivity = AccountService.HasActiveActivities(account, msgs, dc);

            // Assert
            Assert.True(HasActivity);
        }

        [Fact]
        public void GetExternallyProvidedActivityCountTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schoolAccountOwner = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolAccountOwner, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, msgs, dc);
            addactivity.Publish(accountOwner, msgs, dc);

            var schoolActivity = ActivityFactory.Create(schoolAccount, msgs, dc);
            schoolActivity.SetProvider(account, schoolAccountOwner, msgs, dc);
            schoolActivity.Publish(schoolAccountOwner, msgs, dc);

            var schoolActivity2 = ActivityFactory.Create(schoolAccount, msgs, dc);
            schoolActivity2.Publish(schoolAccountOwner, msgs, dc);

            Assert.Equal(1, account.GetExternallyProvidedActivityCount(msgs, dc));
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetExternallyProvidedActivitiesTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var schoolAccountOwner = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolAccountOwner, msgs, dc);

            // Act
            var addactivity = ActivityFactory.Create(account, msgs, dc);
            addactivity.Publish(accountOwner, msgs, dc);

            var schoolActivity = ActivityFactory.Create(schoolAccount, msgs, dc);
            schoolActivity.SetProvider(account, schoolAccountOwner, msgs, dc);
            schoolActivity.Publish(schoolAccountOwner, msgs, dc);

            var schoolActivity2 = ActivityFactory.Create(schoolAccount, msgs, dc);
            schoolActivity2.Publish(schoolAccountOwner, msgs, dc);

            var activities = account.GetExternallyProvidedActivities(msgs, dc);

            Assert.Single(activities);
            Assert.Equal(schoolActivity.ID, activities.First().ID);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact(DisplayName = "AddContact test")]
        public void AddContactTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            var addContact = AccountService.AddContact(account, "Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);

            // Assert
            Assert.NotNull(addContact);
        }

        [Fact(DisplayName = "SearchContactCount test")]
        public void SearchContactTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            AccountService.AddContact(account, "Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var searchContact = AccountService.SearchContactCount(account, "Test", null, dc);

            // Assert
            Assert.True(searchContact > 0);
        }

        [Fact(DisplayName = "GetContactCount test")]
        public void GetContactCountTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            AccountService.AddContact(account, "Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var GetContactCount = AccountService.GetContactCount(account, dc);

            // Assert
            Assert.True(GetContactCount == 1);
        }

        [Fact(DisplayName = "LoadByActivitiesbyActivityGroupByAccountId test")]
        public void LoadByActivitiesbyActivityGroupByAccountIdTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
            addactivity.Publish(accountOwner, msgs, dc);

            // Act
            var activityList = AccountService.LoadByActivitiesbyActivityGroupByAccountId(account, activityGroup.ID.Value, msgs, dc);              

            // Assert
            Assert.True(activityList.Count() == 1);
        }


        [Fact(DisplayName = "GetRegistrationPeriod test")]
        public void GetRegistrationPeriodTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var ResgistrationPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);              

            // Act
            var RegistrationPeriod = AccountService.GetRegistrationPeriod(account, ResgistrationPeriod.ID.Value, msgs, dc);

            // Assert
            Assert.NotNull(RegistrationPeriod);
        }

        [Theory(DisplayName = "GetRegistrationPeriodsByGroupId Test")]
        [InlineData(1)]
        [InlineData(5)]
        public void GetRegistrationPeriodsByGroupIdTest(int expectedNumberOfResistrationPeriod)
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var registrationPeriods = new List<RegistrationPeriod>();

            for (int i = 0; i < expectedNumberOfResistrationPeriod; i++)
            {
                var regPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);
                registrationPeriods.Add(regPeriod);
            }

            // Act
            var RegistrationPeriodLists = AccountService.GetRegistrationPeriods(account,null,null, msgs, dc);


            // Assert
            Assert.Equal(expected: registrationPeriods.Count, actual: RegistrationPeriodLists.Count);
        }


        [Fact(DisplayName = "DoesContactExist test")]
        public void DoesContactExistTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            AccountService.AddContact(account, "Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var GetContact = AccountService.DoesContactExist(account, "Test", "Lastname", "test@test.com", msgs, dc);

            // Assert
            Assert.True(GetContact);
        }

        [Fact(DisplayName = "GetContacts test")]
        public void GetContactsTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            AccountService.AddContact(account, "Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var GetContactsCount = AccountService.GetContacts(account, null, null, dc);

            // Assert
            Assert.True(GetContactsCount.Count == 1);
        }

        [Fact]
        public void GetAttendanceRegistrantTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);

            // Act
            var expected = account.GetAttendanceRegistrant(activity, attendance.ID.Value, registrant.ID.Value, msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(expected: expected.ID, actual: attendanceRegistrant.ID);
        }

        [Fact]
        public void GetAttendanceRegistrantErrorTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);

            // Act
            activity.AccountId = -1;
            msgs.AddRange(activity.Save(dc: dc));
            var expectedError = account.GetAttendanceRegistrant(activity, attendance.ID.Value, registrant.ID.Value, msgs, dc);

            // Assert
            Assert.Null(expectedError);
            Assert.True(msgs.HasErrors());
        }

        [Fact]
        public void GetAttendanceRegistrantByDateRangeTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);
            var dateRange = new DateRange(
                    DateTime.UtcNow.AddDays(-1),
                    DateTime.UtcNow.AddDays(1)
                );

            // Act
            var expected = account.GetAttendanceRegistrantByDateRange(activity, registrant.ID.Value, dateRange, msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(expected: expected.FirstOrDefault().ID.Value, actual: attendanceRegistrant.ID);
        }

        [Fact]
        public void GetAttendanceRegistrantByDateRangeErrorTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);
            var dateRange = new DateRange(
                    DateTime.UtcNow.AddDays(-1),
                    DateTime.UtcNow.AddDays(1)
                );

            // Act
            activity.AccountId = -1;
            msgs.AddRange(activity.Save(dc: dc));
            var expectedError = account.GetAttendanceRegistrantByDateRange(activity, registrant.ID.Value, dateRange, msgs, dc);

            // Assert
            Assert.Null(expectedError);
            Assert.True(msgs.HasErrors());
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripe is activated test")]
        public void UseStripeConnectDuringCheckoutWithStripeTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                
            //UseStripeConnectedAccount is true for this package, so this test case will pass since it meets all criteria
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

            // Assert
            Assert.True(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripeChargesEnabled test")]
        public void UseStripeConnectDuringCheckoutWithStripeChargesEnabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);                
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

            // Assert
            Assert.True(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripeCharges is disabled test")]
        public void UseStripeConnectDuringCheckoutWithStripeChargesDisabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: false, stripePayoutsEnabled: true, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

            // Assert
            Assert.False(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout StripePayouts Enabled test")]
        public void UseStripeConnectDuringCheckoutWithStripePayoutsEnabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

            // Assert
            Assert.True(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout StripePayouts is Disabled test")]
        public void UseStripeConnectDuringCheckoutWithStripePayoutsDisabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: false, stripePayoutsEnabled: false, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

            // Assert
            Assert.False(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout Empty account test")]
        public void UseStripeConnectDuringCheckoutEmptyAccountTest()
        {
            // Arrange && Act
            var expected = AccountService.UseStripeConnectDuringCheckout(account: null, msgs: msgs, dc: dc);

            // Assert
            Assert.False(expected);           
        }

        [Fact]
        public void GetRegistrationTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(account, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registration = RegistrationFactory.Create(account, registrant, msgs, dc);

            // Act
            var expected = account.GetRegistration(Uid: registration.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected: expected.ID, actual: registration.ID);
            Assert.Equal(expected: expected.Uid, actual: registration.Uid);
            Assert.Equal(expected: expected.RegistrantId, actual: registration.RegistrantId);            
        }

        [Fact]
        public void IsApprovedAsVisitorToTest()
        {
            // Arrange
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);
            var accountRelationship = AccountRelationshipService.CreateNew(
                hostAccount,
                visitorAccount,
                ApprovalStatus.Approved,
                HostPermissionType.Edit,
                true,
                false,
                user,
                msgs,
                dc);

            // Act
            var expected = visitorAccount.IsApprovedAsVisitorTo(hostAccount: hostAccount, msgs: msgs, dc: dc);

            // Assert
            Assert.True(expected);
        }

        [Fact]
        public void IsApprovedAsVisitorToFailingTest()
        {
            // Arrange
            var visitorOwner = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
            var hostOwner = WebUserFactory.Create(msgs, dc);
            var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);
            var accountRelationship = AccountRelationshipService.CreateNew(
                hostAccount,
                visitorAccount,
                ApprovalStatus.Denied,
                HostPermissionType.Edit,
                true,
                false,
                user,
                msgs,
                dc);

            // Act
            var expected = visitorAccount.IsApprovedAsVisitorTo(hostAccount: hostAccount, msgs: msgs, dc: dc);

            // Assert
            Assert.False(expected);
        }

        [Theory]
        [InlineData(100, 4, 6)]
        [InlineData(10, 1, 2)]
        public void GetMaxAmountAvailableForRefundTest(decimal amount, decimal paymentTypeFee, decimal convenienceFee)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.CreateWithEmail("DonJohn@test.com", msgs, dc);
            var purchasingAccountOwner = WebUserFactory.CreateWithEmail("CristineTrump@test.com", msgs, dc);
            var billerAccount = AccountFactory.CreateWithFreePackage(billingAccountOwner, AccountType.School, msgs, dc);
            var payerAccount = AccountFactory.CreateWithFreePackage(purchasingAccountOwner, AccountType.School, msgs, dc);

            var transaction = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.CreditCard, amount, 0, 0, billerAccount.ID.Value, billerAccount.ID.Value, convenienceFee, paymentTypeFee, msgs, dc);

            // Act
            var expected = billerAccount.GetMaxAmountAvailableForRefund(search: null, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected: expected, actual: amount + convenienceFee);
        }

        [Theory(DisplayName = "CreatePurchaseTransactionPayment test")]
        [InlineData(100, 6, 4, 110, 100, false)] //no pro-rate
        [InlineData(100, 6, 4, 90, 83.33, true)] //assume user joins mid-period and so as we have to keep pro-rated amount, its 94.3m(otherwise it would 100m)
        [InlineData(10, 2, 0, 12, 10, false)] //no pro-rate
        [InlineData(10, 2, 0, 9.7, 9.1, true)]
        public void CreatePurchaseTransactionPaymentTest(decimal amount, decimal convenienceFee, decimal paymentTypeFee, decimal amountPaidByParent, decimal amountReceivedByProvider, bool isAmountProRated = false)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
            var activityGroup = ActivityGroupFactory.Create(billingAccount, ActivityGroupType.Session, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);
            purchasingAccount.CreateStripeCustomer(msgs, dc);

            var purchase = PurchaseFactory.Create(purchasingAccount, amount, registrantId: registrant.ID.Value, registrationId: registration.ID.Value, msgs, dc); // check: we are inserting row amount here
            var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan, purchase, amount: amount, convenienceFee: convenienceFee, paymentTypeFee: paymentTypeFee, msgs, dc);
            var participationObject = subscriptionPlan.GetParticipationObject(msgs, dc);

            Account convenienceFeePayer;
            Account transactionFeePayer;

            if (registrantSubscriptionPlan.ConvenienceFeesPaidBy == FeesPaidByType.Parent)
                convenienceFeePayer = purchasingAccount;
            else
                convenienceFeePayer = billingAccount;

            if (registrantSubscriptionPlan.PaymentTypeFeesPaidBy == FeesPaidByType.Parent)
                transactionFeePayer = purchasingAccount;
            else
                transactionFeePayer = billingAccount;

            // save transaction info and track payment to the activity as well.
            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                OverrideAmount = amount,
                AmountPaidByParent = amountPaidByParent,
                AmountReceivedByProvider = amountReceivedByProvider,
                IsAmountProRated = isAmountProRated,
                ConvenienceFeeOverrideAmount = registrantSubscriptionPlan.ConvenienceFee,
                PaymentTypeFeeOverrideAmount = registrantSubscriptionPlan.PaymentTypeFee,
                ParticipationObject = participationObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = registrantSubscriptionPlan.PaymentTypeId,
                SubscriptionPlan = subscriptionPlan.PlanTypeId == SubscriptionPlanType.Subscription ? subscriptionPlan : null,
                PaymentPlan = subscriptionPlan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? subscriptionPlan : null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                RegistrantSubscriptionPlan = registrantSubscriptionPlan,
                ConvenienceFeePayer = convenienceFeePayer,
                CreditCardFeePayer = transactionFeePayer,
                Purchase = purchase
            };

            // Act
            AccountService.CreatePurchaseTransactionPayment(checkoutObject, user, msgs, dc);

            // Assert
            //account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            //activity
            Assert.NotNull(activity);
            Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

            //registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant?.AccountId, purchasingAccount.ID.Value);

            //registration
            Assert.NotNull(registration);
            Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration?.AccountId, billingAccount.ID.Value);                

            //registrantSubscriptionPlan
            Assert.NotNull(registrantSubscriptionPlan);
            Assert.Equal(registrantSubscriptionPlan?.Amount, amount);
            Assert.Equal(registrantSubscriptionPlan?.SubscriptionPlanId, subscriptionPlan.ID.Value);
            Assert.Equal(registrantSubscriptionPlan?.RegistrantId, registrant.ID.Value);
            Assert.Equal(registrantSubscriptionPlan?.ConvenienceFee, convenienceFee);
            Assert.Equal(registrantSubscriptionPlan?.PaymentTypeFee, paymentTypeFee);

            //purchase
            Assert.NotNull(purchase);
            Assert.Equal(purchase?.ID, purchase.ID.Value);
            Assert.Equal(purchase?.AmountPaid, amount);
            Assert.Equal(purchase?.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase?.RegistrantId, registrant.ID.Value);
            Assert.Equal(purchase?.PurchaserAccountId, purchasingAccount.ID.Value);

            //subscription plan
            Assert.NotNull(subscriptionPlan);
            Assert.Equal(subscriptionPlan?.ParticipationObjectId, participationObject.ObjectID.Value);
            Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participationObject.ObjectType);
            Assert.Equal(subscriptionPlan?.CostPerPeriod, amountPaidByParent); //here subscription plan will have amount with all fees
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            //transaction
            var transaction = Transaction.LoadByBillerAccountIdByIsDeleted(BillerAccountId: billingAccount.ID.Value, IsDeleted: false, msgs, dc).FirstOrDefault();
            Assert.NotNull(transaction);
            Assert.Equal(transaction?.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction?.PaymentTypeFeeCharged, paymentTypeFee);
            Assert.Equal(transaction?.ConvenienceFeeCharged, convenienceFee);
            Assert.Equal(transaction?.PaymentTypeFeePayerAccountId, transactionFeePayer.ID.Value);
            Assert.Equal(transaction?.ConvenienceFeePayerAccountId, convenienceFeePayer.ID.Value);
            Assert.Equal(transaction?.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction?.PayerAccountId, purchasingAccount.ID.Value);

            //activityPayment
            var activityPayment = ActivityPayment.LoadByParticipationObjectByRegistrant(participationObject.ObjectID.Value, participationObject.ObjectType, registrant.ID.Value, false, msgs, dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment?.Amount, amountReceivedByProvider);
            Assert.Equal(activityPayment?.AmountPaidByParent, amountPaidByParent);
            Assert.Equal(activityPayment?.ParticipationObjectId, participationObject.ObjectID.Value);
            Assert.Equal(activityPayment?.IsProRated, isAmountProRated);
            Assert.Equal(activityPayment?.RegistrantId, registrant.ID.Value);

            //Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetOpenPublishedSessionsTest()
        {
            var activityGroup1 = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var activityGroup2 = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var activityGroup3 = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
            var regPeriod1 = RegistrationPeriodFactory.Create(account, activityGroup1, msgs, dc);
            var regPeriod2 = RegistrationPeriodFactory.Create(account, activityGroup2, msgs, dc);
            var regPeriod3 = RegistrationPeriodFactory.Create(account, activityGroup2, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(4), msgs, dc);

            msgs.AddRange(activityGroup1.SetIsPublished(true, user.ID.Value, dc));
            msgs.AddRange(activityGroup2.SetIsPublished(true, user.ID.Value, dc));
            msgs.AddRange(activityGroup3.SetIsPublished(true, user.ID.Value, dc));

            activityGroup2.SetIsDeleted(user.ID.Value, true, msgs, dc);

            var sessions = account.GetOpenPublishedSessions(msgs, dc);

            Assert.Single(sessions);
            Assert.Equal(activityGroup1.ID, sessions.First().ID);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void IsLocationOwnerTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            var schoolUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.SetLocation(location, user, msgs, dc);

            var isUserAccountLocationOwner = account.IsLocationOwner(activity, msgs, dc);
            var isSchoolProviderLocationOwner = schoolAccount.IsLocationOwner(activity, msgs, dc);

            Assert.False(isUserAccountLocationOwner);
            Assert.True(isSchoolProviderLocationOwner);
        }

        [Theory(DisplayName = "Basic registration fee check with ACH")]
        [InlineData(100, 7)]
        public void ProcessRegistrationFeeTest1(decimal amount, decimal convenienceFee)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, amount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var bankAccountPurchaseObject = new BankAccountPurchaseObject() { PlaidPublicToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.ACH,
                BankAccountPurchaseObject = bankAccountPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                SavePaymentInfo = true,
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, amount + convenienceFee);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);
            Assert.Null(purchase.CouponId);

            // activityPayment                
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount);            
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);            
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            // in registration fee case, we will not be storing amount paid by parent
            // it will be only with the case of pro-rate and with plans
            Assert.Null(activityPayment.AmountPaidByParent); 

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, amount + convenienceFee);
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);            
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(0, transaction.PaymentTypeFeeCharged);
            Assert.Null(transaction.PaymentTypeFeePayerAccountId);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);
            Assert.Equal(0, transaction.CouponValue);
            Assert.Null(transaction.CouponId);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (amount + convenienceFee) * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Basic registration fee check with CC")]
        [InlineData(100, 7, 4)]
        public void ProcessRegistrationFeeTest2(decimal amount, decimal convenienceFee, decimal paymentTypeFee)
        {            
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, amount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount =  AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };
            
            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,                    
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                CreditCardFeePayer = billingAccount,
                SavePaymentInfo = true,
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);                
            Assert.Equal(purchase.AmountPaid, amount + convenienceFee + paymentTypeFee);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);
            Assert.Null(purchase.CouponId);

            // activityPayment                
            var activityPayment =  ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount);            
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);            
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, amount + convenienceFee + paymentTypeFee);
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.PaymentTypeFeeCharged, paymentTypeFee);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);
            Assert.Equal(0, transaction.CouponValue);
            Assert.Null(transaction.CouponId);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (amount + convenienceFee + paymentTypeFee) * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Registration fee check with ACH with coupon")]
        [InlineData(100, 7, 10, (int)CouponAmountType.FlatAmount)]
        [InlineData(100, 7, 50, (int)CouponAmountType.FlatAmount)]
        [InlineData(100, 7, 10, (int)CouponAmountType.Percentage)]
        [InlineData(100, 7, 50, (int)CouponAmountType.Percentage)]
        public void ProcessRegistrationFeeTest3(decimal amount, decimal convenienceFee, decimal couponAmount, int couponAmountType)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, amount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var coupon = CouponFactory.Create(account: billingAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: (CouponAmountType)couponAmountType, msgs: msgs, dc: dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var bankAccountPurchaseObject = new BankAccountPurchaseObject() { PlaidPublicToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                Coupon = coupon,
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                IsAmountProRated = false,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.ACH,
                BankAccountPurchaseObject = bankAccountPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                SavePaymentInfo = true,
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, coupon.GetDiscountedValue(amount + convenienceFee));
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to purchase
            Assert.Equal(purchase.CouponId, coupon.ID.Value);

            // activityPayment                
            var activityPayments = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc);
            var activityPayment = activityPayments.FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount - coupon.GetValue(amount + convenienceFee));
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, coupon.GetDiscountedValue(amount + convenienceFee));
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(0, transaction.PaymentTypeFeeCharged);
            Assert.Null(transaction.PaymentTypeFeePayerAccountId);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to transaction
            Assert.Equal(coupon.GetValue(amount + convenienceFee), transaction.CouponValue);
            Assert.Equal(transaction.CouponId, coupon.ID.Value);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (coupon.GetDiscountedValue(amount + convenienceFee)) * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Registration fee check with CC with coupon")]
        [InlineData(100, 7, 4, 10, (int)CouponAmountType.FlatAmount)]
        [InlineData(100, 7, 4, 50, (int)CouponAmountType.FlatAmount)]
        [InlineData(100, 7, 4, 10, (int)CouponAmountType.Percentage)]
        [InlineData(100, 7, 4, 50, (int)CouponAmountType.Percentage)]
        public void ProcessRegistrationFeeTest4(decimal amount, decimal convenienceFee, decimal paymentTypeFee, decimal couponAmount, int couponAmountType)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, amount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var coupon = CouponFactory.Create(account: billingAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: (CouponAmountType)couponAmountType, msgs: msgs, dc: dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            var checkoutObject = new CheckoutObject()
            {
                Coupon = coupon,
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,                
                IsAmountProRated = false,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                CreditCardFeePayer = billingAccount,
                SavePaymentInfo = true,
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, coupon.GetDiscountedValue(amount + convenienceFee) + paymentTypeFee);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to purchase
            Assert.Equal(purchase.CouponId, coupon.ID.Value);

            // activityPayment                
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount - coupon.GetValue(amount + convenienceFee));
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, coupon.GetDiscountedValue(amount + convenienceFee) + paymentTypeFee);
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.PaymentTypeFeeCharged, paymentTypeFee);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to transaction
            Assert.Equal(coupon.GetValue(amount + convenienceFee), transaction.CouponValue);
            Assert.Equal(transaction.CouponId, coupon.ID.Value);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (coupon.GetDiscountedValue(amount + convenienceFee) + paymentTypeFee) * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Registration fee check for selected dates(Individual classes) with ACH")]
        [InlineData(100, 7, 1)]
        [InlineData(100, 7, 2)]
        [InlineData(100, 7, 5)]
        public void ProcessRegistrationFeeTest5(decimal amount, decimal convenienceFee, int classQuantity)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.Now, DateTime.Now.AddDays(10), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, amount, msgs, dc: dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var bankAccountPurchaseObject = new BankAccountPurchaseObject() { PlaidPublicToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };            

            // this will add number of class for ClassQuantity
            var selectedDates = new List<DateTime>();
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(classQuantity);
            selectedDates.AddRange(Enumerable.Range(0, end.Subtract(start).Days).Select(offset => start.AddDays(offset)).ToArray());

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.ACH,
                BankAccountPurchaseObject = bankAccountPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                SavePaymentInfo = true,
                SelectedDates = selectedDates
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, (amount + convenienceFee) * classQuantity);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);
            Assert.Null(purchase.CouponId);

            // activityPayment                
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount * classQuantity);
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, (amount + convenienceFee) * classQuantity);
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee * classQuantity);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(0, transaction.PaymentTypeFeeCharged);
            Assert.Null(transaction.PaymentTypeFeePayerAccountId);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);
            Assert.Equal(0, transaction.CouponValue);
            Assert.Null(transaction.CouponId);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (amount + convenienceFee) * classQuantity * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Registration fee check for selected dates(Individual classes) with CC")]
        [InlineData(100, 7, 4, 1)]
        [InlineData(100, 7, 4, 2)]
        [InlineData(100, 7, 4, 5)]
        public void ProcessRegistrationFeeTest6(decimal amount, decimal convenienceFee, decimal paymentTypeFee, int classQuantity)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);            
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.Now, DateTime.Now.AddDays(10), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, amount, msgs, dc: dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };            

            // this will add number of class for ClassQuantity
            var selectedDates = new List<DateTime>();
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(classQuantity);
            selectedDates.AddRange(Enumerable.Range(0, end.Subtract(start).Days).Select(offset => start.AddDays(offset)).ToArray());

            var checkoutObject = new CheckoutObject()
            {
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                CreditCardFeePayer = billingAccount,
                SavePaymentInfo = true,
                SelectedDates = selectedDates
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, (amount + convenienceFee + paymentTypeFee) * classQuantity);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);
            Assert.Null(purchase.CouponId);

            // activityPayment                
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, amount * classQuantity);
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, (amount + convenienceFee + paymentTypeFee) * classQuantity);
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.PaymentTypeFeeCharged, paymentTypeFee * classQuantity);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee * classQuantity);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);
            Assert.Equal(0, transaction.CouponValue);
            Assert.Null(transaction.CouponId);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (amount + convenienceFee + paymentTypeFee) * classQuantity * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Registration fee check for selected dates(Individual classes) with ACH & coupon")]
        [InlineData(100, 7, 1, 10, (int)CouponAmountType.FlatAmount)] // single class & 10$ off
        [InlineData(100, 7, 2, 10, (int)CouponAmountType.FlatAmount)] // two class
        [InlineData(100, 7, 5, 10, (int)CouponAmountType.FlatAmount)] // five class

        [InlineData(100, 7, 1, 50, (int)CouponAmountType.FlatAmount)] // single class & 50$ off
        [InlineData(100, 7, 2, 50, (int)CouponAmountType.FlatAmount)] // two class
        [InlineData(100, 7, 5, 50, (int)CouponAmountType.FlatAmount)] // five class

        [InlineData(100, 7, 1, 10, (int)CouponAmountType.Percentage)] // single class & 10% off
        [InlineData(100, 7, 2, 10, (int)CouponAmountType.Percentage)] // two class
        [InlineData(100, 7, 5, 10, (int)CouponAmountType.Percentage)] // five class

        [InlineData(100, 7, 1, 50, (int)CouponAmountType.Percentage)] // single class & 50$ off
        [InlineData(100, 7, 2, 50, (int)CouponAmountType.Percentage)] // two class
        [InlineData(100, 7, 5, 50, (int)CouponAmountType.Percentage)] // five class
        public void ProcessRegistrationFeeTest7(decimal amount, decimal convenienceFee, int classQuantity, decimal couponAmount, int couponAmountType)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.Now, DateTime.Now.AddDays(10), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, amount, msgs, dc: dc);
            var coupon = CouponFactory.Create(account: billingAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: (CouponAmountType)couponAmountType, msgs: msgs, dc: dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var bankAccountPurchaseObject = new BankAccountPurchaseObject() { PlaidPublicToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            // this will add number of class for ClassQuantity
            var selectedDates = new List<DateTime>();
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(classQuantity);
            selectedDates.AddRange(Enumerable.Range(0, end.Subtract(start).Days).Select(offset => start.AddDays(offset)).ToArray());

            var checkoutObject = new CheckoutObject()
            {
                Coupon = coupon,
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.ACH,
                BankAccountPurchaseObject = bankAccountPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                SavePaymentInfo = true,
                SelectedDates = selectedDates
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            //discounted amount
            var discountedAmount = coupon.GetDiscountedValue((amount + convenienceFee) * classQuantity);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, discountedAmount);
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to purchase
            Assert.Equal(purchase.CouponId, coupon.ID.Value);

            // activityPayment                
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, (amount * classQuantity) - coupon.GetValue((amount + convenienceFee) * classQuantity));
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, discountedAmount);
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee * classQuantity);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(0, transaction.PaymentTypeFeeCharged);
            Assert.Null(transaction.PaymentTypeFeePayerAccountId);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to transaction
            Assert.Equal(coupon.GetValue((amount + convenienceFee) * classQuantity), transaction.CouponValue);
            Assert.Equal(transaction.CouponId, coupon.ID.Value);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, discountedAmount * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "Registration fee check for selected dates(Individual classes) with CC & coupon")]
        [InlineData(100, 7, 4, 1, 10, (int)CouponAmountType.FlatAmount)] // single class & 10$ off
        [InlineData(100, 7, 4, 2, 10, (int)CouponAmountType.FlatAmount)] // two class
        [InlineData(100, 7, 4, 5, 10, (int)CouponAmountType.FlatAmount)] // five class

        [InlineData(100, 7, 4, 1, 50, (int)CouponAmountType.FlatAmount)] // single class & 50$ off
        [InlineData(100, 7, 4, 2, 50, (int)CouponAmountType.FlatAmount)] // two class
        [InlineData(100, 7, 4, 5, 50, (int)CouponAmountType.FlatAmount)] // five class

        [InlineData(100, 7, 4, 1, 10, (int)CouponAmountType.Percentage)] // single class & 10% off
        [InlineData(100, 7, 4, 2, 10, (int)CouponAmountType.Percentage)] // two class
        [InlineData(100, 7, 4, 5, 10, (int)CouponAmountType.Percentage)] // five class

        [InlineData(100, 7, 4, 1, 50, (int)CouponAmountType.Percentage)] // single class & 50$ off
        [InlineData(100, 7, 4, 2, 50, (int)CouponAmountType.Percentage)] // two class
        [InlineData(100, 7, 4, 5, 50, (int)CouponAmountType.Percentage)] // five class
        public void ProcessRegistrationFeeTest8(decimal amount, decimal convenienceFee, decimal paymentTypeFee, int classQuantity, decimal couponAmount, int couponAmountType)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, isMaster: true, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);

            var schedule = ActivityScheduleFactory.Create(billingAccount, DateTime.Now, DateTime.Now.AddDays(10), msgs, dc);
            var activity = ActivityFactory.Create(billingAccount, schedule, amount, msgs, dc: dc);
            var coupon = CouponFactory.Create(account: billingAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: (CouponAmountType)couponAmountType, msgs: msgs, dc: dc);
            var participantObject = activity.GetParticipationObject(msgs: msgs, dc: dc);

            // Update stripe account and customer of payer/parent
            var stripePurchasingAccount = AccountFactory.CreateStripeAccountAndCustomer(account: purchasingAccount, msgs: msgs, dc: dc);
            var sourceId = purchasingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePurchasingAccount.Item2, msgs, dc);
            var creditCardPurchaseObject = new CreditCardPurchaseObject() { StripeToken = StripeHelper.TestSourceToken, StripeSourceId = sourceId };

            // this will add number of class for ClassQuantity
            var selectedDates = new List<DateTime>();
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(classQuantity);
            selectedDates.AddRange(Enumerable.Range(0, end.Subtract(start).Days).Select(offset => start.AddDays(offset)).ToArray());

            var checkoutObject = new CheckoutObject()
            {
                Coupon = coupon,
                PayingAccount = purchasingAccount,
                BillingAccount = billingAccount,
                ParticipationObject = participantObject,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SubscriptionPlan = null,
                PaymentPlan = null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                RegistrantSubscriptionPlan = null,
                Registration = registration,
                ConvenienceFeePayer = billingAccount,
                CreditCardFeePayer = billingAccount,
                SavePaymentInfo = true,
                SelectedDates = selectedDates
            };

            // Act
            AccountService.ProcessRegistrationFee(checkoutObject, user, msgs, dc);

            // Assert
            // account
            Assert.NotNull(billingAccount);
            Assert.NotNull(purchasingAccount);

            // activity
            Assert.NotNull(activity);
            Assert.Equal(activity.AccountId, billingAccount.ID.Value);
            Assert.Equal(activity.CostPerRegistrant, amount);

            // registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant.AccountId, purchasingAccount.ID.Value);

            // registration
            Assert.NotNull(registration);
            Assert.Equal(registration.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration.AccountId, billingAccount.ID.Value);

            //discounted amount
            var discountedAmount = coupon.GetDiscountedValue((amount + convenienceFee) * classQuantity);

            // purchase
            var purchase = registration.GetPurchase(msgs, dc);            
            Assert.NotNull(purchase);
            Assert.Equal(purchase.AmountPaid, discountedAmount + (paymentTypeFee * classQuantity));
            Assert.Equal(purchase.StripeSourceId, sourceId);
            Assert.Equal(purchase.RegistrationId, registration.ID.Value);
            Assert.Equal(purchase.RegistrantId, masterRegistrant.ID.Value);
            Assert.Equal(purchase.PurchaserAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to purchase
            Assert.Equal(purchase.CouponId, coupon.ID.Value);

            // activityPayment                
            var activityPayment = ActivityPayment.LoadByParticipationObject(ParticipationObjectId: activity.ID.Value,
                        ParticipationObjectTypeId: ParticipationObjectType.Activity,
                        IsDeleted: false,
                        msgs: msgs,
                        dc: dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment.Amount, (amount * classQuantity) - coupon.GetValue((amount + convenienceFee) * classQuantity));
            Assert.Equal(activityPayment.ParticipationObjectId, activity.ID.Value);
            Assert.Equal(activityPayment.RegistrantId, registrant.ID.Value);
            Assert.False(activityPayment.IsProRated);
            Assert.Null(activityPayment.AmountPaidByParent);

            // transaction
            var transaction = activityPayment.GetTransaction(msgs, dc);
            Assert.NotNull(transaction);
            Assert.Equal(transaction.Amount, discountedAmount + (paymentTypeFee * classQuantity));
            Assert.Equal(transaction.StripeChargeOrRefundId, checkoutObject.StripeChargeId);
            Assert.Equal(transaction.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction.PaymentTypeFeeCharged, paymentTypeFee * classQuantity);
            Assert.Equal(transaction.ConvenienceFeeCharged, convenienceFee * classQuantity);
            Assert.Equal(transaction.PaymentTypeFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.ConvenienceFeePayerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction.PayerAccountId, purchasingAccount.ID.Value);

            // check proper coupon is attached to transaction
            Assert.Equal(coupon.GetValue((amount + convenienceFee) * classQuantity), transaction.CouponValue);
            Assert.Equal(transaction.CouponId, coupon.ID.Value);

            // Stripe
            var customer = purchasingAccount.GetStripeCustomer(msgs);
            Assert.NotNull(customer);

            var charge = StripeHelper.GetCharge(stripeChargeId: checkoutObject.StripeChargeId, msgs);
            Assert.NotNull(charge);

            // confirm that stripe charge matches transaction
            Assert.Equal(charge.Amount, (discountedAmount + (paymentTypeFee * classQuantity)) * 100);
            Assert.Equal(charge.CustomerId, customer.Id);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact(DisplayName = "Test RequestNewPayout with Check")]
        public void RequestNewPayoutCheckTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerUser = WebUserFactory.Create(msgs, dc);
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var billerAccount = AccountFactory.Create(billerUser, msgs, dc);
            var transaction = TransactionFactory.Create(billerAccount: billerAccount, payerAccount: payerAccount, paymentType: PaymentType.Check, amount: 100,
                platformPercentFee: 0, platformFlatFee: 0, convenienceFeePayerAccountId: billerUser.ID.Value, paymentTypeFeePayerAccountId: payerAccount.ID.Value,
                convenienceFeeCharged: 7, paymentTypeFeeCharged: 4, msgs: msgs, dc: dc, transactionType: TransactionType.Charge);

            // Act
            var payout = billerAccount.RequestNewPayout(statementDescriptor: "Ahq Payout - Test", transactions: transaction.AsList(), loggedInUser: loggedInUser, msgs: msgs, dc: dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(payout);
            Assert.NotNull(payout.ID);
            Assert.Equal(expected: "Ahq Payout - Test", actual: payout.StatementDescriptor);
            Assert.Equal(expected: PaymentType.Check, actual: payout.PaymentTypeId);
            Assert.Equal(expected: PayoutStatus.Requested, actual: payout.PayoutStatusId);

            var payoutTransactions = payout.GetTransactions(msgs: msgs, dc: dc);
            Assert.Single(payoutTransactions);
            Assert.Equal(payoutTransactions.FirstOrDefault().ID, transaction.ID);
        }

        [Fact(DisplayName = "Test RequestNewPayout with ACH")]
        public void RequestNewPayoutACHTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerUser = WebUserFactory.Create(msgs, dc);
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var billerAccount = AccountFactory.CreateAccountWithStripeFields(user: loggedInUser, accountType: AccountType.ActivityProvider, stripePayoutsEnabled: true, stripeChargesEnabled: true, stripeUserId: "1", msgs: msgs, dc: dc);
            var transaction = TransactionFactory.Create(billerAccount: billerAccount, payerAccount: payerAccount, paymentType: PaymentType.ACH, amount: 100,
                platformPercentFee: 0, platformFlatFee: 0, convenienceFeePayerAccountId: billerUser.ID.Value, paymentTypeFeePayerAccountId: payerAccount.ID.Value,
                convenienceFeeCharged: 7, paymentTypeFeeCharged: 4, msgs: msgs, dc: dc, transactionType: TransactionType.Charge);

            // Act
            var payout = billerAccount.RequestNewPayout(statementDescriptor: "Ahq Payout - Test", transactions: transaction.AsList(), loggedInUser: loggedInUser, msgs: msgs, dc: dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(payout);
            Assert.NotNull(payout.ID);
            Assert.Equal(expected: "Ahq Payout - Test", actual: payout.StatementDescriptor);
            Assert.Equal(expected: PaymentType.ACH, actual: payout.PaymentTypeId);
            Assert.Equal(expected: PayoutStatus.Requested, actual: payout.PayoutStatusId);

            var payoutTransactions = payout.GetTransactions(msgs: msgs, dc: dc);
            Assert.Single(payoutTransactions);
            Assert.Equal(payoutTransactions.FirstOrDefault().ID, transaction.ID);
        }

        [Fact(DisplayName = "Failing Test case for DisableStripePayoutForConnectAccountFailureTest")]
        public void DisableStripePayoutForConnectAccountFailureTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerAccount = AccountFactory.CreateAccountWithStripeFields(user: loggedInUser, accountType: AccountType.ActivityProvider, stripePayoutsEnabled: true, stripeChargesEnabled: true, stripeUserId: "1", msgs: msgs, dc: dc);

            // Act
            var stripeAccount = StripeHelper.ToggleStripePayoutForConnectedAccount(customAccountsStripeUserId: billerAccount.StripeUserId, accountName: billerAccount.Name, payoutIntervalType: PayoutIntervalType.Daily, msgs: msgs);

            // Assert
            Assert.True(msgs.HasErrors()); //stripe account will not found
            Assert.Null(stripeAccount);
        }

        [Fact(DisplayName = "Disable stripe auto payout and move to manual AHQ payout")]
        public void DisableStripeAutoPayoutForConnectAccountTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerAccount = AccountFactory.CreateAccount(user: loggedInUser, accountType: AccountType.School, stripePayoutsEnabled: true, stripeChargesEnabled: true, msgs: msgs, dc: dc);

            // Update stripe account and customer to billerAccount
            AccountFactory.CreateStripeAccountAndCustomer(account: billerAccount, msgs: msgs, dc: dc);

            // Act
            var stripeAccount = StripeHelper.ToggleStripePayoutForConnectedAccount(customAccountsStripeUserId: billerAccount.StripeUserId, accountName: billerAccount.Name, payoutIntervalType: PayoutIntervalType.Manual, msgs: msgs);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(stripeAccount);
            Assert.Equal(expected: PayoutIntervalType.Manual.ToString().ToLower(), actual: stripeAccount.Settings.Payouts.Schedule.Interval);            
            Assert.True(msgs.HasMessageContainingText($"Account: ({billerAccount.Name}) updated with manual AHQ payout option."));
        }

        [Fact(DisplayName = "Enable stripe auto payout to those accounts which already had manual AHQ payout")]
        public void EnableStripeAutoPayoutForConnectAccountTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerAccount = AccountFactory.CreateAccount(user: loggedInUser, accountType: AccountType.School, stripePayoutsEnabled: true, stripeChargesEnabled: true, msgs: msgs, dc: dc);

            // Update stripe account and customer to billerAccount
            AccountFactory.CreateStripeAccountAndCustomer(account: billerAccount, msgs: msgs, dc: dc);

            // Act
            var stripeAccountWithManualPayout = StripeHelper.ToggleStripePayoutForConnectedAccount(customAccountsStripeUserId: billerAccount.StripeUserId, accountName: billerAccount.Name, payoutIntervalType: PayoutIntervalType.Manual, msgs: msgs);
            var stripeAccountWithAutoPayout = StripeHelper.ToggleStripePayoutForConnectedAccount(customAccountsStripeUserId: billerAccount.StripeUserId, accountName: billerAccount.Name, payoutIntervalType: PayoutIntervalType.Daily, msgs: msgs);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(stripeAccountWithAutoPayout);
            Assert.Equal(expected: PayoutIntervalType.Daily.ToString().ToLower(), actual: stripeAccountWithAutoPayout.Settings.Payouts.Schedule.Interval);
            Assert.True(msgs.HasMessageContainingText($"Account: ({billerAccount.Name}) updated with manual AHQ payout option."));
            Assert.True(msgs.HasMessageContainingText($"Account: ({billerAccount.Name}) updated with daily Stripe payout option."));            
        }

        [Theory(DisplayName = "Basic Test for PrepareManualPayouts")]
        [InlineData(100)]
        public void PrepareManualPayoutsTest(decimal amount)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("billingUser", "A", "billingUserA@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccountWithFreePackage(accountName: "billingAccount-A", accountEmail: billingAccountOwner.Email, user: billingAccountOwner, accountType: AccountType.School,
                stripePayoutsEnabled: true, stripeChargesEnabled: true, msgs: msgs, dc: dc);

            var purchasingAccountOwner = WebUserFactory.Create("purchasingUser", "A", "purchasingUserA@test.com", true, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.Registrant, msgs, dc);

            var period = RegistrationPeriodFactory.Create(billingAccount, msgs, dc);
            var activity = ActivityFactory.Create(account: billingAccount, costPerRegistrant: amount, msgs: msgs, dc: dc);
            msgs.AddRange(period.AddActivity(activity, true, billingAccountOwner.ID.Value, dc));
            activity.Publish(billingAccountOwner, msgs, dc);

            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var transaction = TransactionFactory.CreateTransactionReadyForPayout(billerAccount: billingAccount, payerAccount: purchasingAccount, amount: amount, paymentType: PaymentType.CreditCard, msgs: msgs, dc: dc);
            var activityPayment = ActivityPaymentFactory.Create(account: billingAccount, amount: amount, activity: activity, registrant: registrant, transaction: transaction, msgs: msgs, dc: dc);

            // Update stripe account and customer to billerAccount
            AccountFactory.CreateStripeAccountAndCustomer(account: billingAccount, msgs: msgs, dc: dc);

            DelayExecution(10000);

            // update stripe account with manual payout
            StripeHelper.ToggleStripePayoutForConnectedAccount(customAccountsStripeUserId: billingAccount.StripeUserId, accountName: billingAccount.Name, payoutIntervalType: PayoutIntervalType.Manual, msgs: msgs);

            // Act
            AccountService.PreparePayouts(msgs: msgs, dc: dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.True(msgs.HasMessageContainingText($"Payout job started on at {DateTime.UtcNow.ToShortDateString()}."));
            Assert.True(msgs.HasMessageContainingText($"Payout job completed on at {DateTime.UtcNow.ToShortDateString()}."));

            // check payout
            var payout = billingAccount.GetLastPayout(msgs, dc);
            Assert.NotNull(payout);
            Assert.Equal(billingAccount.ID.Value, payout.AccountId);
            Assert.Equal(PayoutStatus.Completed, payout.PayoutStatusId);
            Assert.Equal(PaymentType.ACH, payout.PaymentTypeId);
            Assert.Equal($"AHQ {billingAccount.UniqueName.MaxChars(10)} {activity.UniqueName.MaxChars(7)}", payout.StatementDescriptor);

            // verify status of stripe account
            var stripeAccount = StripeHelper.GetCustomAccount(stripeAccountId: billingAccount.StripeUserId, msgs: msgs);
            Assert.NotNull(stripeAccount);
            Assert.Equal(stripeAccount.Settings.Payouts.Schedule.Interval, PayoutIntervalType.Manual.ToString().ToLower());

            // verify payout amount of stripe
            var stripePayout = StripeHelper.GetPayout(payoutId: payout.ExternalPayoutId, stripeAccountId: stripeAccount.Id, msgs: msgs);
            Assert.NotNull(stripePayout);
            Assert.Equal(payout.Amount, stripePayout.Amount / 100);
        }
        [Theory(DisplayName = "Get Activities For Instructor Test")]
        [InlineData(1, 0, 1)]
        [InlineData(10, 0, 5)]
        [InlineData(30, 10, 5)]
        public void GetActivitiesForInstructorsTest(int numberOfActivities, int start, int length)
        {
            // Arrange
            var instructorWebUser = WebUserFactory.Create(msgs, dc);
            var accountUser = AccountFactory.CreateAccount(instructorWebUser, msgs, dc);
            var accountWebUser = AccountWebUserFactory.Create(accountUser, instructorWebUser, UserType.Instructor, true, msgs, dc);
            List<Activity> arrangeActivities = new List<Activity>();


            for (int i = 0; i < numberOfActivities; i++)
            {
                var activityGroup = ActivityGroupFactory.Create(accountUser, ActivityGroupType.Session, msgs, dc: dc);
                var activity = ActivityFactory.Create(accountUser, ActivityType.Activity, activityGroup, msgs, dc);

                if (i % 2 == 0)
                   ActivityWebUserFactory.Create(activity, instructorWebUser, false, msgs, dc);

                arrangeActivities.Add(activity);
            }

            // Act
            var actActivities = accountUser.GetActivities(accountWebUser.UserTypeId, instructorWebUser, ActivityType.Activity, false, false, false, start, length, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(length, actActivities.Count);
        }

        [Theory(DisplayName = "ProcessSubscriptionPlan Test")]
        [InlineData(110, 5)]
        [InlineData(12, 5)]
        public void ProcessSubscriptionPlanTest(decimal amount, decimal paymentTypeFeeOverrideAmount)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

            //UseStripeConnectedAccount is true for this package
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            AccountPackageFactory.Create(billingAccount, package, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amount, msgs, dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

            var participationObject = activity.GetParticipationObject(msgs, dc);
            var checkoutObject = new CheckoutObject()
            {
                BillingAccount = billingAccount,
                PayingAccount = payingAccount,
                Coupon = null,
                OverrideAmount = amount,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                ParticipationObject = participationObject,
                SubscriptionPlan = subscriptionPlan,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                //ConvenienceFeeOverrideAmount = 5,
                PaymentTypeFeeOverrideAmount = paymentTypeFeeOverrideAmount,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SavePaymentInfo = true,
                Registration = registration
            };

            //Act
            AccountService.ProcessSubscriptionPlan(
                checkoutObject,
                masterRegistrant,
                payingAccountOwner,
                msgs,
                dc);

            //Assert
            //account
            Assert.NotNull(billingAccount);
            Assert.NotNull(payingAccount);

            //activity
            Assert.NotNull(activity);
            Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

            //registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant?.AccountId, payingAccount.ID.Value);

            //registration
            Assert.NotNull(registration);
            Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participationObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participationObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            if (isSubscriptionRequired)
            {
                //registrantSubscriptionPlan
                var registrantSubscriptionPlan = checkoutObject.RegistrantSubscriptionPlan;
                Assert.NotNull(registrantSubscriptionPlan);
                Assert.Equal(registrantSubscriptionPlan?.Amount, amount);
                Assert.Equal(registrantSubscriptionPlan?.SubscriptionPlanId, subscriptionPlan.ID.Value);

                //here we will use masterRegistrant as we created plan for him
                Assert.Equal(registrantSubscriptionPlan?.RegistrantId, masterRegistrant.ID.Value);
                Assert.Equal(registrantSubscriptionPlan?.ConvenienceFee, checkoutObject.GetSubscriptionCostTodayConvenienceFee(msgs, dc));
                Assert.Equal(registrantSubscriptionPlan?.PaymentTypeFee, checkoutObject.GetSubscriptionCostTodayCreditCardFee(msgs, dc));

                //subscription plan
                Assert.NotNull(subscriptionPlan);
                Assert.Equal(subscriptionPlan?.ParticipationObjectId, participationObject.ObjectID.Value);
                Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participationObject.ObjectType);
                Assert.Equal(subscriptionPlan?.CostPerPeriod, amount);
                Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
                Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
                Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

                //purchase
                var purchase = checkoutObject.Purchase;
                Assert.NotNull(purchase);
                Assert.Equal(purchase?.ID, purchase.ID.Value);
                Assert.Equal(purchase?.AmountPaid, amount);
                Assert.Equal(purchase?.RegistrationId, registration.ID.Value);
                Assert.Equal(purchase?.RegistrantId, masterRegistrant.ID.Value);
                Assert.Equal(purchase?.PurchaserAccountId, payingAccount.ID.Value);
            }

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "ProcessPaymentPlan Test")]
        [InlineData(110, 5)]
        [InlineData(12, 5)]
        public void ProcessPaymentPlanTest(decimal amount, decimal paymentTypeFeeOverrideAmount)
        {
            // Arrange
            var StartDate = DateTime.UtcNow;
            var EndDate = DateTime.UtcNow.AddDays(35);
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

            //UseStripeConnectedAccount is true for this package
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            AccountPackageFactory.Create(billingAccount, package, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.PaymentPlan, amount, msgs, dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

            var participationObject = activity.GetParticipationObject(msgs, dc);
            var checkoutObject = new CheckoutObject()
            {
                BillingAccount = billingAccount,
                PayingAccount = payingAccount,
                Coupon = null,
                OverrideAmount = amount,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                ParticipationObject = participationObject,
                SubscriptionPlan = subscriptionPlan,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                //ConvenienceFeeOverrideAmount = 5,
                PaymentTypeFeeOverrideAmount = paymentTypeFeeOverrideAmount,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SavePaymentInfo = true,
                Registration = registration,
                PaymentPlan = subscriptionPlan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? subscriptionPlan : null,

            };

            //Act
            AccountService.ProcessPaymentPlan(
                checkoutObject,
                masterRegistrant,
                payingAccountOwner,
                msgs,
                dc);
           
          
            Assert.True(msgs.HasNoErrors());
           
            //Assert
            //account
            Assert.NotNull(billingAccount);
            Assert.NotNull(payingAccount);

            //activity
            Assert.NotNull(activity);
            Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

            //registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant?.AccountId, payingAccount.ID.Value);

            //registration
            Assert.NotNull(registration);
            Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participationObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participationObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            if (isSubscriptionRequired)
            {
                //registrantSubscriptionPlan
                var registrantSubscriptionPlan = checkoutObject.RegistrantSubscriptionPlan;
                Assert.NotNull(registrantSubscriptionPlan);
                Assert.Equal(registrantSubscriptionPlan?.Amount, amount);
                Assert.Equal(registrantSubscriptionPlan?.SubscriptionPlanId, subscriptionPlan.ID.Value);

                //here we will use masterRegistrant as we created plan for him
                Assert.Equal(registrantSubscriptionPlan?.RegistrantId, masterRegistrant.ID.Value);
                Assert.Equal(registrantSubscriptionPlan?.ConvenienceFee, checkoutObject.GetSubscriptionCostTodayConvenienceFee(msgs, dc));
                Assert.Equal(registrantSubscriptionPlan?.PaymentTypeFee, checkoutObject.GetSubscriptionCostTodayCreditCardFee(msgs, dc));

                //subscription plan
                Assert.NotNull(subscriptionPlan);
                Assert.Equal(subscriptionPlan?.ParticipationObjectId, participationObject.ObjectID.Value);
                Assert.Equal(subscriptionPlan?.ParticipationObjectTypeId, participationObject.ObjectType);
                Assert.Equal(subscriptionPlan?.CostPerPeriod, amount);
                Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
                Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
                Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

                //purchase
                var purchase = checkoutObject.Purchase;
                Assert.NotNull(purchase);
                Assert.Equal(purchase?.ID, purchase.ID.Value);
                Assert.Equal(purchase?.AmountPaid, amount);
                Assert.Equal(purchase?.RegistrationId, registration.ID.Value);
                Assert.Equal(purchase?.RegistrantId, masterRegistrant.ID.Value);
                Assert.Equal(purchase?.PurchaserAccountId, payingAccount.ID.Value);
            }

            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "ProcessPostPayPlan Test")]
        [InlineData(110, 5)]
        [InlineData(12, 5)]
        public void ProcessPostPayPlanTest(decimal amount, decimal paymentTypeFeeOverrideAmount)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
           
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(payingAccount, isMaster: true, msgs, dc);

            //UseStripeConnectedAccount is true for this package
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            AccountPackageFactory.Create(billingAccount, package, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
            var registrant = RegistrantFactory.Create(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.PostPayPlan, amount, msgs, dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

            var participationObject = activity.GetParticipationObject(msgs, dc);
            var checkoutObject = new CheckoutObject()
            {
                BillingAccount = billingAccount,
                PayingAccount = payingAccount,
                Coupon = null,
                OverrideAmount = amount,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                ParticipationObject = participationObject,
                SubscriptionPlan = subscriptionPlan,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                //ConvenienceFeeOverrideAmount = 5,
                PaymentTypeFeeOverrideAmount = paymentTypeFeeOverrideAmount,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                ConvenienceFeePayer = billingAccount,
                CreditCardFeePayer = billingAccount,
                SavePaymentInfo = true,
                Registration = registration,
                PostPayPlan = subscriptionPlan
            };

            //Act
            AccountService.ProcessPostPayPlan(
                checkoutObject,
                user,
                msgs,
                dc);

            //Assert
            //account
            Assert.NotNull(billingAccount);
            Assert.NotNull(payingAccount);

            //activity
            Assert.NotNull(activity);
            Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

            //registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant?.AccountId, payingAccount.ID.Value);

            //registration
            Assert.NotNull(registration);
            Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participationObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participationObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);
        
            //Stripe
            var product = StripeHelper.GetProduct(activity.Uid.ToString(), stripeAccountId: null, msgs);
            Assert.NotNull(product);

            Assert.True(msgs.HasNoErrors());
        }

        [Theory(DisplayName = "ProcessRegistrationWithPayment Test")]
        [InlineData(110, 5, SubscriptionPlanType.Subscription,10)]
        public void ProcessRegistrationWithPaymentTest(decimal amount, decimal paymentTypeFeeOverrideAmount, SubscriptionPlanType subscriptionPlanType, decimal costPerPeriod)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var payingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateAccount(billingAccountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", payingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.CreateWithNames(payingAccount, isMaster: true, msgs, dc);

            //UseStripeConnectedAccount is true for this package
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            AccountPackageFactory.Create(billingAccount, package, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
            var registrant = RegistrantFactory.CreateWithNames(billingAccount, isMaster: false, msgs, dc: dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(planOwnerAccount: account, activity: activity, planType: subscriptionPlanType , costPerPeriod: costPerPeriod, msgs: msgs, dc: dc);

            AccountFactory.CreateStripeAccountAndCustomer(billingAccount, msgs, dc);
            var stripePayingObject = AccountFactory.CreateStripeAccountAndCustomer(payingAccount, msgs, dc);
            var sourceId = payingAccount.CreateStripeSourceFromToken(StripeHelper.TestSourceToken, stripePayingObject.Item2, msgs, dc);

            var creditCardPurchaseObject = new CreditCardPurchaseObject()
            {
                StripeToken = StripeHelper.TestSourceToken,
                StripeSourceId = sourceId
            };

            // Will wait for 10 seconds for stripe to create this account
            DelayExecution(10000);

            var participationObject = activity.GetParticipationObject(msgs, dc);
            var checkoutObject = new CheckoutObject()
            {
                BillingAccount = billingAccount,
                PayingAccount = payingAccount,
                Coupon = null,
                OverrideAmount = amount,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                ParticipationObject = participationObject,
                SubscriptionPlan = subscriptionPlan,
                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                //ConvenienceFeeOverrideAmount = 5,
                PaymentTypeFeeOverrideAmount = paymentTypeFeeOverrideAmount,
                PaymentType = PaymentType.CreditCard,
                CreditCardPurchaseObject = creditCardPurchaseObject,
                SavePaymentInfo = true,
                Registration = registration
            };

            //Act
            AccountService.ProcessRegistrationWithPayment(
                checkoutObject,
                payingAccountOwner,
                msgs,
                dc);

            //Assert
            //account
            Assert.NotNull(billingAccount);
            Assert.NotNull(payingAccount);

            //activity
            Assert.NotNull(activity);
            Assert.Equal(activity?.AccountId, billingAccount.ID.Value);

            //registrant
            Assert.NotNull(registrant);
            Assert.NotNull(masterRegistrant);
            Assert.Equal(registrant?.AccountId, billingAccount.ID.Value);
            Assert.Equal(masterRegistrant?.AccountId, payingAccount.ID.Value);

            //registration
            Assert.NotNull(registration);
            Assert.Equal(registration?.RegistrantId, registrant.ID.Value);
            Assert.Equal(registration?.AccountId, billingAccount.ID.Value);

            var startDateTime = masterRegistrant.GetPlanStartDate(subscriptionPlan, participationObject, msgs, dc);
            var endDateTime = subscriptionPlan.GetPlanEndDateForRegistrantSubscription(startDateTime, participationObject, msgs);
            var isSubscriptionRequired = true;

            if (startDateTime.IsNotEmpty() && endDateTime.IsNotEmpty())
                isSubscriptionRequired = subscriptionPlan.CheckIfSubscriptionRequired(startDateTime, endDateTime.Value);

            Assert.True(msgs.HasNoErrors());
        }        
    }
}
