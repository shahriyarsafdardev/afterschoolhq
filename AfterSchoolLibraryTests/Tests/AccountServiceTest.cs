using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountServiceTest : BaseTest
    {
        private readonly WebUser person = new WebUser
            {FirstName = "Don", LastName = "John", Email = $"don+{DateTime.Now.Ticks}@example.com", IsActive = true};

        public AccountServiceTest(ITestOutputHelper output) : base(output)
        {
            User = WebUserFactory.Create(msgs, dc);
            DefaultAccount = AccountFactory.Create(User, msgs, dc);
        }

        private WebUser User { get; }
        private Account DefaultAccount { get; }

        [Fact(DisplayName = "CreateNewTest")]
        public void CreateNewTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var accountOwner = WebUserFactory.CreateWithEmail($"don+{DateTime.Now.Ticks}@example.com", msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var account = AccountService.CreateNew(
                $"test org{DateTime.Now.Ticks}",
                accountOwner.Email,
                $"317-555-{DateTime.Now.Ticks % 10000:0000}",
                false,
                AccountType.School,
                accountOwner,
                package.ID,
                BillingPeriodType.Month,
                null,
                createWithStripe: true,
                loggedInUser: loggedInUser,
                isAfterSchool: true,
                msgs: msgs,
                dc: dc);

            Assert.True(account.ID.HasValue);
        }

        [Theory(DisplayName = "ProcessStudentRegistrationTest")]
        [InlineData(false)] //Parent doesn't have a account
        [InlineData(true)] //Parent does have a account
        public void ProcessStudentRegistrationTest(bool parentAccountExists)
        {
            //Arrange
            var user = WebUserFactory.Create(msgs, dc);
            var parentAccount = AccountFactory.CreateWithFreePackage(user, AccountType.Registrant, msgs, dc);
            var form = FormFactory.Create(parentAccount, msgs, dc);
            var formQuestionGroup = FormQuestionGroupFactory.Create(form.ID.Value, false, msgs, dc);
            if (!parentAccountExists)
                parentAccount = null;

            var questionAnswerModel =
                FormQuestionFactory.GetPersonalInformationQuestionAnswerModel(formQuestionGroup, msgs, dc);

            //Act
            var studentDetails = AccountService.ProcessStudentRegistration(
                person.FirstName,
                person.LastName,
                person.Email,
                null,
                "12345678",
                null,
                true,
                true,
                questionAnswerModel,
                parentAccount,
                msgs);
            var studentAccount = studentDetails.Item1;
            var studentUser = studentDetails.Item2;
            var studentRegistrant = studentDetails.Item3;

            //Assert
            if (parentAccountExists)
            {
                Assert.True(studentAccount.IsEmpty());
                Assert.True(studentUser.IsNotEmpty());
                Assert.True(studentRegistrant.IsNotEmpty());
                Assert.True(studentRegistrant.AccountId == parentAccount.ID);
                Assert.True(studentRegistrant.WebUserId == studentUser.ID);
                Assert.True(studentRegistrant.IsParentOrGuardian == false);
                Assert.True(studentRegistrant.IsMaster);
            }
            else
            {
                Assert.True(studentAccount.IsNotEmpty());
                Assert.True(studentUser.IsNotEmpty());
                Assert.True(studentRegistrant.IsNotEmpty());
                Assert.True(studentAccount.CreatedBy == studentUser.ID);
                Assert.True(studentAccount.AccountTypeId == AccountType.Registrant);
                Assert.True(studentRegistrant.AccountId == studentAccount.ID);
                Assert.True(studentRegistrant.WebUserId == studentUser.ID);
                Assert.True(studentRegistrant.IsParentOrGuardian == false);
                Assert.True(studentRegistrant.IsMaster);
            }
        }

        [Theory(DisplayName = "ProcessStudentRegistrationTest")]
        [InlineData("https://qa.afterschoolhq.com/testaccount/activity/testactivity")]
        public void ProcessParentRegistrationTest(string url)
        {
            //Arrange
            var studentUser = WebUserFactory.Create(msgs, dc);
            var studentAccount = AccountFactory.CreateWithFreePackage(studentUser, AccountType.Registrant, msgs, dc);
            var form = FormFactory.Create(studentAccount, msgs, dc);
            var formQuestionGroup = FormQuestionGroupFactory.Create(form.ID.Value, false, msgs, dc);

            var questionAnswerModel =
                FormQuestionFactory.GetPersonalInformationQuestionAnswerModel(formQuestionGroup, msgs, dc);

            //Act
            var parentDetails = studentAccount.ProcessParentRegistration(
                person.FirstName,
                person.LastName,
                person.Email,
                url,
                true,
                questionAnswerModel,
                studentUser,
                msgs);

            var parentUser = parentDetails.Item1;
            var parentRegistrant = parentDetails.Item2;
            //ParentUser is created on Student Account && Parentregistrant is created on Student Account. 

            //Assert
            Assert.True(parentUser.IsNotEmpty());
            Assert.True(parentUser.CreatedBy == studentUser.ID);
            Assert.True(parentRegistrant.IsNotEmpty());
            Assert.True(parentRegistrant.AccountId == studentAccount.ID);
            Assert.True(parentRegistrant.IsParentOrGuardian);
            Assert.True(parentRegistrant.IsMaster);
            Assert.True(parentRegistrant.WebUserId == parentUser.ID);
        }

        [Fact(Skip = "Not sure")]
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
        ///     Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        ///     need to ask about sending emails and name parameters!
        /// </summary>
        [Fact(DisplayName = "Request access to a host account")]
        public void RequestAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var webuser = WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var webuser2 = WebUserFactory.CreateWithEmail("tes2t@test.com", msgs, dc);
            var hostaccount = AccountFactory.Create(webuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(webuser2, msgs, dc);
            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly,
                loggedInUser, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        ///     Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact(DisplayName = "Approve Provider Access to a Host Account")]
        public void ApproveAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostUser = WebUserFactory.CreateWithEmail("test@test.com", msgs, dc);
            var visitorUser = WebUserFactory.CreateWithEmail("test2@test.com", msgs, dc);
            var hostaccount = AccountFactory.Create(hostUser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);
            var relationship1 = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly,
                loggedInUser, msgs, dc);
            var relationship2 = hostaccount.ApproveAccessToAccount(visitoraccount, loggedInUser, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        ///     Will see if the RequestAccessToAccountTest() function will return a message with no Errors
        /// </summary>
        [Fact(DisplayName = "Host account denies request to visitor account")]
        public void DenyAccessToAccountTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.CreateWithEmail("test@test", msgs, dc);
            var visitorUser = WebUserFactory.CreateWithEmail("test2@test", msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            var relationship = visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly,
                loggedInUser, msgs, dc);
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        ///     Will request access to an account, deny it. Then list off the denied visitor accounts the host has
        ///     The expected result will a list filled with one visitor account.
        ///     TODO: figure out  how to make sure all of the needed parameters are passed in.
        /// </summary>
        [Fact(DisplayName = "Host account gets list of denied visitor accounts")]
        public void GetDeniedVisitorAccountsTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var hostuser = WebUserFactory.Create(true, msgs, dc);
            var visitorUser = WebUserFactory.Create(true, msgs, dc);
            var hostaccount = AccountFactory.Create(hostuser, msgs, dc);
            var visitoraccount = AccountFactory.Create(visitorUser, msgs, dc);

            visitoraccount.RequestAccessToAccount(hostaccount, HostPermissionType.ApproveOnly,
                loggedInUser, msgs, dc);
            msgs.AddRange(hostaccount.DenyAccessToAccount(visitoraccount, loggedInUser, dc));

            Assert.Single(hostaccount.GetDeniedVisitorAccounts(visitoraccount.AccountTypeId, msgs, dc));
        }

        [Fact(DisplayName = "Get or create default form for account")]
        public void GetOrCreateDefaultForm()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var form = account.GetOrCreateDefaultStudentForm(accountOwner, msgs, dc);

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

        [Theory(DisplayName = "Get Platform Flat Fee")]
        [InlineData(.99, 1, .99)]
        [InlineData(3, 1, 3)]
        [InlineData(.99, 0, .99)]
        [InlineData(3, 0, 3)]
        public void GetPlatformFlatFeeTest(decimal platformFlatFee, decimal stripeConnectedAccountValue,
            decimal expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee,
                msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount,
                stripeConnectedAccountValue, msgs, dc);

            var fee = account.GetPlatformConvenienceFlatFee(msgs, dc);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, fee);
        }

        [Theory(DisplayName = "Get Platform Percent Fee")]
        [InlineData(.05, 1, .05)]
        [InlineData(.34, 1, .34)]
        [InlineData(.05, 0, 0.05)]
        [InlineData(.34, 0, 0.34)]
        public void GetPlatformPercentFeeTest(decimal platformPercentFee, decimal stripeConnectedAccountValue,
            decimal expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentFee,
                msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount,
                stripeConnectedAccountValue, msgs, dc);

            var fee = account.GetPlatformConveniencePercentFee(msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, fee);
        }

        /// <summary>
        ///     This method is not being used anymore, but I kept it as it might be needed.
        /// </summary>
        /// <param name="tuitionFee"></param>
        /// <param name="platformFlatFee"></param>
        /// <param name="platformPercentageFee"></param>
        /// <param name="expectedPlatformFee"></param>
        [Theory(DisplayName = "Calculate Platform Flat Fee Amount")]
        [InlineData(100, 0.99, 0.05, 5.99)]
        [InlineData(100, 0, 0.05, 5.0)]
        public void CalculatePlatformFeeAmountTest(decimal tuitionFee, decimal platformFlatFee,
            decimal platformPercentageFee, decimal expectedPlatformFee)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee,
                platformPercentageFee, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee,
                msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, 1, msgs, dc);

            var fee = account.CalculatePlatformConvenienceFeeAmount(tuitionFee, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedPlatformFee, fee);
        }

        /// <summary>
        ///     should calculate the percentage that AHQ gets when processing a subscription payment
        ///     Should be equal to
        ///     feeAmount / totalCost
        ///     to get the final percentage
        /// </summary>
        /// <param name="platformFlatFee"></param>
        /// <param name="platformPercentageFee"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="expectedPlatformPercentage"></param>
        [Theory(Skip = "Stripe Dependency")]
        [InlineData(.99, .05, 2, 1, 0.5)]
        [InlineData(.99, .05, 20, 1, 0.1489)]
        [InlineData(.99, .05, 100, 1, 0.0991)]
        [InlineData(.99, .05, 2, 0, 0.5)]
        [InlineData(.99, .05, 20, 0, 0.1489)]
        [InlineData(.99, .05, 100, 0, 0.0991)]
        public void GetSubscriptionPlatformFeePercentTest(decimal platformFlatFee, decimal platformPercentageFee,
            decimal costPerPeriod, int stripeConnectedAccountValue, decimal expectedPlatformPercentage)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee,
                platformPercentageFee, msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee,
                msgs, dc);
            AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount,
                stripeConnectedAccountValue, msgs, dc);

            var activity = ActivityFactory.Create(account, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(account, activity, SubscriptionPlanType.Subscription,
                costPerPeriod, msgs, dc);
            var subscriptionPlanFeePercent =
                account.GetSubscriptionPlatformFeePercent(subscriptionPlan, PaymentType.CreditCard, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedPlatformPercentage, subscriptionPlanFeePercent);
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
            
            AccountPackageFeatureFactory.Create(accountPackage, featureType, value, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Equal(expectedResult, account.HasAccountPackageFeature(featureType, msgs, dc));
        }

        [Theory(DisplayName = "GetBillerAccountTransactionsTest")]
        [InlineData(1)]
        [InlineData(5)]
        public void GetBillerAccountTransactionsTest(int expectedNumberOfTransactions)
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var parentUser = WebUserFactory.Create(msgs, dc);
            var billeraccount =
                AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payerAccount = AccountFactory.CreateWithFreePackage(parentUser, AccountType.Registrant, msgs, dc);

            var transactionIds = new List<int>();

            for (var i = 0; i < expectedNumberOfTransactions; i++)
            {
                var transaction = TransactionFactory.Create(billeraccount, payerAccount, PaymentType.ACH, msgs, dc);
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
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var parentUser = WebUserFactory.Create(msgs, dc);
            var billeraccount =
                AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payerAccount = AccountFactory.CreateWithFreePackage(parentUser, AccountType.Registrant, msgs, dc);

            var transactionIds = new List<int>();

            for (var i = 0; i < expectedNumberOfTransactions; i++)
            {
                var transaction = TransactionFactory.Create(billeraccount, payerAccount, PaymentType.ACH, msgs, dc);
                transactionIds.Add(transaction.ID.Value);
            }

            var actualNumberOfTransactions = payerAccount.GetPayerAccountTransactions(transactionIds, msgs, dc);

            Assert.Equal(expectedNumberOfTransactions, actualNumberOfTransactions.Count);
        }

        [Fact(DisplayName = "GetBillerTransactionTest")]
        public void GetBillerTransactionTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var payingUser = WebUserFactory.Create(msgs, dc);
            var billingAccount =
                AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var expectedTransaction =
                TransactionFactory.Create(billingAccount, payingAccount, PaymentType.ACH, msgs, dc);
            var actualTransaction = billingAccount.GetBillerTransaction(expectedTransaction.Uid, msgs, dc);

            Assert.Equal(expectedTransaction.ID, actualTransaction.ID);
        }


        [Fact(DisplayName = "GetPayerTransactionTest")]
        public void GetPayerTransactionTest()
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var payingUser = WebUserFactory.Create(msgs, dc);
            var billingAccount =
                AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var expectedTransaction =
                TransactionFactory.Create(billingAccount, payingAccount, PaymentType.ACH, msgs, dc);
            var actualTransaction = payingAccount.GetPayerTransaction(expectedTransaction.Uid, msgs, dc);

            Assert.Equal(expectedTransaction.ID, actualTransaction.ID);
        }

        [Theory(DisplayName = "GetAllActivitiesForAccountTest")]
        [InlineData(1)]
        [InlineData(5)]
        public void GetAllActivitiesForAccountTest(int expectedNumberOfActivities)
        {
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var expectedActivities = new List<Activity>();

            for (var i = 0; i < expectedNumberOfActivities; i++)
            {
                var expectedActivity = ActivityFactory.Create(account, msgs, dc);
                expectedActivities.Add(expectedActivity);
            }

            var actualActivities = account.GetAllActivitiesForAccount(msgs, dc);

            Assert.Equal(expectedActivities.Count, actualActivities.Count);
        }

        [Fact]
        public void GetActivityScheduleTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(DefaultAccount, msgs, dc);

            // Act
            var expected = DefaultAccount.GetActivitySchedule(schedule.ID.Value, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, schedule.ID);
            Assert.Equal(expected.Name, schedule.Name);
        }

        [Fact]
        public void GetActivitySchedulesTest()
        {
            // Arrange
            var schedule = ActivityScheduleFactory.Create(DefaultAccount, msgs, dc);
            var schedule2 = ActivityScheduleFactory.Create(DefaultAccount, msgs, dc);

            // Act
            var activitySchedules = DefaultAccount.GetActivitySchedules(msgs, dc);

            // Assert
            Assert.Equal(2, activitySchedules.Count);
            Assert.Equal(schedule.ID, activitySchedules[0].ID);
            Assert.Equal(schedule2.ID, activitySchedules[1].ID);
        }

        [Fact]
        public void GetActivityMessageTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var activityMessage = ActivityMessageFactory.Create(DefaultAccount, activity, msgs, dc);

            // Act
            var expected = DefaultAccount.GetActivityMessage(activityMessage.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, activityMessage.ID);
            Assert.Equal(expected.Uid, activityMessage.Uid);
        }

        [Fact]
        public void GetActivityByIDTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);

            // Act
            var expected = DefaultAccount.GetActivity(activity.ID.Value, msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(expected.ID, activity.ID);
            Assert.Equal(expected.Uid, activity.Uid);
            Assert.Equal(expected.Name, activity.Name);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByIDFailingTest()
        {
            // Act
            var expected = DefaultAccount.GetActivity(84930248, msgs, dc);

            // Assert
            Assert.Null(expected);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByUIDTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);

            // Act
            var expected = DefaultAccount.GetActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, activity.ID);
            Assert.Equal(expected.Uid, activity.Uid);
            Assert.Equal(expected.Name, activity.Name);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByUIDFailingTest()
        {
            // Act
            var expected = DefaultAccount.GetActivity(Guid.NewGuid(), msgs, dc);

            // Assert
            Assert.Null(expected);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByLocationHostAccountTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expectedById = schoolAccount.GetActivity(activity.ID.Value, msgs, dc);
            var expectedByUid = schoolAccount.GetActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expectedById.ID, activity.ID);
            Assert.Equal(expectedById.Uid, activity.Uid);
            Assert.Equal(expectedById.Name, activity.Name);

            Assert.Equal(expectedByUid.ID, activity.ID);
            Assert.Equal(expectedByUid.Uid, activity.Uid);
            Assert.Equal(expectedByUid.Name, activity.Name);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetActivityByLocationHostAccountFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
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
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expectedById = schoolAccount.GetHostedActivity(activity.ID.Value, msgs, dc);
            var expectedByUid = schoolAccount.GetHostedActivity(activity.Uid, msgs, dc);

            // Assert
            Assert.Equal(expectedById.ID, activity.ID);
            Assert.Equal(expectedById.Uid, activity.Uid);
            Assert.Equal(expectedById.Name, activity.Name);

            Assert.Equal(expectedByUid.ID, activity.ID);
            Assert.Equal(expectedByUid.Uid, activity.Uid);
            Assert.Equal(expectedByUid.Name, activity.Name);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetHostedActivityByIdFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
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
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            // Act
            var expected = schoolAccount.GetHostedActivity(activity, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, activity.ID);
            Assert.Equal(expected.Uid, activity.Uid);
            Assert.Equal(expected.Name, activity.Name);
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
            var expected2 = ActivityFactory.Create(DefaultAccount, location, msgs, dc);

            expected.ProviderAccountId = DefaultAccount.ID.Value;
            msgs.AddRange(expected.Save(dc));

            var actual = DefaultAccount.GetExternallyProvidedActivity(expected.Uid, msgs, dc);
            var actual2 = DefaultAccount.GetExternallyProvidedActivity(expected.ID.Value, msgs, dc);
            var actual3 = DefaultAccount.GetExternallyProvidedActivity(expected2.ID.Value, msgs, dc);

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
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var activity2 = ActivityFactory.Create(DefaultAccount, msgs, dc);

            // Act
            var activities = DefaultAccount.GetActivities(msgs, dc);

            // Assert
            Assert.Equal(2, activities.Count);
            Assert.Equal(activity.ID, activities[0].ID);
            Assert.Equal(activity2.ID, activities[1].ID);
        }

        [Theory(DisplayName = "GetActivitiesByIds Test")]
        [InlineData(5)]
        public void GetActivitiesByIdsTest(int n)
        {
            // Arrange
            var activityIds = new List<int>();
            for (var i = 0; i < n; i++)
            {
                var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
                activityIds.Add(activity.ID.Value);
            }

            // Act
            var expected = DefaultAccount.GetActivities(activityIds, msgs, dc);

            // Assert
            Assert.Equal(expected.Count, n);
        }

        [Fact]
        public void GetActivityGroupTest()
        {
            // Arrange
            var activityGroup = ActivityGroupFactory.Create(DefaultAccount, ActivityGroupType.Session, msgs, dc);

            // Act
            var expected = DefaultAccount.GetActivityGroup(activityGroup.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, activityGroup.ID);
            Assert.Equal(expected.Uid, activityGroup.Uid);
            Assert.Equal(expected.Name, activityGroup.Name);
        }

        [Theory(DisplayName = "GetActivityGroups Test")]
        [InlineData(5)]
        public void GetActivityGroupsTest(int n)
        {
            // Arrange
            var activityGroupIds = new List<int>();
            for (var i = 0; i < n; i++)
            {
                var activityGroup = ActivityGroupFactory.Create(DefaultAccount, ActivityGroupType.Session, msgs, dc);
                activityGroupIds.Add(activityGroup.ID.Value);
            }

            // Act
            var expected = DefaultAccount.GetActivityGroups(activityGroupIds, msgs, dc);

            // Assert
            Assert.Equal(expected.Count, n);
        }

        [Fact]
        public void GetPublishedActivitiesTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            AccountFactory.Create(schoolWebUser, msgs, dc);
            var period = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);

            // Act 
            var activities = DefaultAccount.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(activity.ID.Value, activities.First().ID.Value);
        }

        [Fact]
        public void GetPublishedActivitiesFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(User.ID.Value, dc));

            // Act 
            var activities = DefaultAccount.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Empty(activities);
            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetPublishedOpenActivitiesTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var activity2 = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var activity3 = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var activity4 = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);
            var period2 = RegistrationPeriodFactory.Create(DefaultAccount, DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(12), msgs, dc);
            var period3 = RegistrationPeriodFactory.Create(DefaultAccount, DateTime.UtcNow.AddDays(-12),
                DateTime.UtcNow.AddDays(-1), msgs, dc);
            var period4 = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));
            msgs.AddRange(period2.AddActivity(activity2, true, User.ID.Value, dc));
            msgs.AddRange(period3.AddActivity(activity3, true, User.ID.Value, dc));
            msgs.AddRange(period4.AddActivity(activity4, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);
            activity2.Publish(User, msgs, dc);
            activity3.Publish(User, msgs, dc);

            // Act 
            var activities = DefaultAccount.GetPublishedOpenedActivities(msgs: msgs, dc: dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(activity.ID.Value, activities.First().ID.Value);
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
            activity.ProviderAccountId = DefaultAccount.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);

            // Act 
            var activities = DefaultAccount.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(activity.ID.Value, activities.First().ID.Value);
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
            activity.ProviderAccountId = DefaultAccount.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(User.ID.Value, dc));

            // Act 
            var activities = DefaultAccount.GetPublishedActivities(msgs, dc);

            // Assert
            Assert.Empty(activities);
            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetPublishedActivitiesInLocationsTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);

            // Act 
            var activities = DefaultAccount.GetPublishedActivitiesInLocations(location.ID.Value.AsList(), msgs, dc);

            // Assert
            Assert.Single(activities);
            Assert.True(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(activity.ID.Value, activities.First().ID.Value);
        }

        [Fact]
        public void GetPublishedActivitiesInLocationsFailingTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var schoolWebUser = WebUserFactory.Create(msgs, dc);
            var schoolAccount = AccountFactory.Create(schoolWebUser, msgs, dc);
            var location = LocationFactory.Create(schoolAccount, msgs, dc);
            var period = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);

            activity.LocationId = location.ID.Value;
            msgs.AddRange(activity.Save(dc));

            msgs.AddRange(period.AddActivity(activity, true, User.ID.Value, dc));

            activity.Publish(User, msgs, dc);

            msgs.AddRange(activity.MoveToArchive(User.ID.Value, dc));

            // Act 
            var activities = DefaultAccount.GetPublishedActivitiesInLocations(location.ID.Value.AsList(), msgs, dc);

            // Assert
            Assert.Empty(activities);
            Assert.False(activity.IsPubliclyViewable);
            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void GetRegistrantTest()
        {
            // Arrange
            var registrant = RegistrantFactory.Create(DefaultAccount, false, msgs, dc);

            // Act                
            var expected = DefaultAccount.GetRegistrant(registrant.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, registrant.ID);
            Assert.Equal(expected.Uid, registrant.Uid);
            Assert.Equal(expected.Email, registrant.Email);
        }

        [Fact]
        public void GetPayoutTest()
        {
            // Arrange
            var payout = PayoutFactory.Create(DefaultAccount, msgs, dc);

            // Act                
            var expected = DefaultAccount.GetPayout(payout.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, payout.ID);
            Assert.Equal(expected.Uid, payout.Uid);
            Assert.Equal(expected.AccountId, payout.AccountId);
            Assert.Equal(expected.Amount, payout.Amount);
            Assert.Equal(expected.PayoutStatusId, payout.PayoutStatusId);
        }

        [Fact]
        public void GetCouponTest()
        {
            // Arrange
            var coupon = CouponFactory.Create(DefaultAccount, msgs, dc);

            // Act 
            var expected = DefaultAccount.GetCoupon(coupon.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, coupon.ID);
            Assert.Equal(expected.Uid, coupon.Uid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void GetCouponsTest(int expectedNumberOfCoupons)
        {
            // Arrange
            var expected = new List<Coupon>();

            for (var i = 0; i < expectedNumberOfCoupons; i++)
            {
                var expectedCoupon = CouponFactory.Create(DefaultAccount, msgs, dc);
                expected.Add(expectedCoupon);
            }

            // Act 
            var coupons = DefaultAccount.GetCoupons(msgs, dc);

            // Assert
            Assert.Equal(expected.Count, coupons.Count);
        }

        [Fact]
        public void GetRefundRequestTest()
        {
            // Arrange
            var payingUser = WebUserFactory.Create(msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var transaction = TransactionFactory.Create(DefaultAccount, payingAccount, PaymentType.ACH, msgs, dc);
            var expectedRefundRequest = RefundRequestFactory.Create(transaction.Amount.Value, transaction,
                DefaultAccount, "reason", payingUser, msgs, dc);

            // Act 
            var actualRefundRequest = DefaultAccount.GetRefundRequest(expectedRefundRequest.Uid, msgs, dc);

            // Assert
            Assert.Equal(expectedRefundRequest.ID, actualRefundRequest.ID);
        }

        [Fact]
        public void GetRefundRequestsTest()
        {
            // Arrange
            var payingUser = WebUserFactory.Create(msgs, dc);
            var payingAccount = AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);

            var transaction = TransactionFactory.Create(DefaultAccount, payingAccount, PaymentType.ACH, msgs, dc);
            var expectedRefundRequest = RefundRequestFactory.Create(transaction.Amount.Value, transaction,
                DefaultAccount, "reason", payingUser, msgs, dc);
            var expectedRefundRequest2 = RefundRequestFactory.Create(transaction.Amount.Value, transaction,
                DefaultAccount, "reason", payingUser, msgs, dc);

            // Act 
            var actualRefundRequest = DefaultAccount.GetRefundRequests(msgs, dc);

            // Assert
            Assert.Equal(2, actualRefundRequest.Count);
        }

        [Fact]
        public void GetAccountPackageTest()
        {
            // Arrange
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(DefaultAccount, package, msgs, dc);

            // Act                
            var expected = DefaultAccount.GetAccountPackage(accountPackage.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, accountPackage.ID);
            Assert.Equal(expected.Uid, accountPackage.Uid);
            Assert.Equal(expected.Price, accountPackage.Price);
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
                User,
                msgs,
                dc);

            // Act                
            var expected = hostAccount.GetAccountRelationship(accountRelationship.ID.Value, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, accountRelationship.ID);
        }

        [Fact]
        public void GetFormTest()
        {
            // Arrange                
            var form = FormFactory.Create(DefaultAccount, msgs, dc);

            // Act                
            var expected = DefaultAccount.GetForm(form.ID.Value, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, form.ID);
        }

        [Fact]
        public void GetAccountWebUserTest()
        {
            // Arrange                
            var webUser = AccountWebUserFactory.Create(DefaultAccount, User, UserType.Student, true, msgs, dc);

            // Act                
            var expected = DefaultAccount.GetAccountWebUser(User.ID.Value, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, webUser.ID);
        }

        [Fact(DisplayName = "Get Account Owner test")]
        public void GetAccountOwnerTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            var GetOwner = account.GetAccountOwner(msgs, dc);

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
            var accountWithNoPackage = AccountFactory.Create(User, msgs, dc);

            // Act
            var accountWithFeature = AccountService.HasFeature(null, FeatureType.AcceptACH, msgs, dc);
            var accountWithNoPackageFeature = accountWithNoPackage.HasFeature(FeatureType.AcceptACH, msgs, dc);

            // Assert
            Assert.False(accountWithFeature);
            Assert.False(accountWithNoPackageFeature);
        }

        [Fact(DisplayName = "GetActiveAccountPackage test")]
        public void GetActiveAccountPackageTest()
        {
            // Arrange
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(DefaultAccount, package, msgs, dc);

            // Act
            var expected = DefaultAccount.GetActiveAccountPackage(msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(package.Name, expected.PackageName);
        }

        [Fact(DisplayName = "GetActiveAccountPackage test 2")]
        public void GetActiveAccountPackageEmptyAccountTest()
        {
            // Arrange & Act
            var expected = AccountService.GetActiveAccountPackage(null, msgs, dc);

            // Assert
            Assert.Null(expected);
        }

        [Fact(DisplayName = "GetCheckPaymentAccount test")]
        public void GetCheckPaymentAccountTest()
        {
            // Arrange
            var checkPaymentAccount = CheckPaymentAccountFactory.Create(DefaultAccount, msgs, dc);

            // Act
            var expected = DefaultAccount.GetCheckPaymentAccount(msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(checkPaymentAccount.Name, expected.Name);
            Assert.Equal(checkPaymentAccount.LegalEntityTypeId, expected.LegalEntityTypeId);
        }

        [Fact(DisplayName = "GetCheckPaymentAccount test 2")]
        public void GetCheckPaymentAccountEmptyAccountTest()
        {
            // Arrange & Act
            var expected = AccountService.GetCheckPaymentAccount(null, msgs, dc);

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
            var GetActivityCount = account.GetActivityCount(msgs, dc);

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

            var GetActivityCount = account.GetActiveAndPublishedActivityCount(true, msgs, dc);

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
            for (var i = 1; i <= 5; i++)
            {
                var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
                if (i % 2 == 0) addactivity.Publish(accountOwner, msgs, dc);
            }

            var GetActivities = account.GetAllActiveAndNonPublishedActivities(msgs, dc);

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
            var GetActivityCount = account.GetActiveAndNonPublishedActivityCount(msgs, dc);

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
            var GetActivityCount = account.GetActiveAndNonPublishedActivityCount(msgs, dc);

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
            var HasActivity = account.HasActivities(msgs, dc);

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
            var HasActivity = account.HasActiveActivities(msgs, dc);

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
            var addContact = account.AddContact("Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs,
                dc);

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
            account.AddContact("Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var searchContact = account.SearchContactCount("Test", null, dc);

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
            account.AddContact("Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var GetContactCount = account.GetContactCount(dc);

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
            var activityList = account.LoadByActivitiesbyActivityGroupByAccountId(activityGroup.ID.Value, msgs, dc);

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
            var RegistrationPeriod = account.GetRegistrationPeriod(ResgistrationPeriod.ID.Value, msgs, dc);

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

            for (var i = 0; i < expectedNumberOfResistrationPeriod; i++)
            {
                var regPeriod = RegistrationPeriodFactory.Create(account, msgs, dc);
                registrationPeriods.Add(regPeriod);
            }

            // Act
            var RegistrationPeriodLists = account.GetRegistrationPeriods(null, null, msgs, dc);


            // Assert
            Assert.Equal(registrationPeriods.Count, RegistrationPeriodLists.Count);
        }


        [Fact(DisplayName = "DoesContactExist test")]
        public void DoesContactExistTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            // Act
            account.AddContact("Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var GetContact = account.DoesContactExist("Test", "Lastname", "test@test.com", msgs, dc);

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
            account.AddContact("Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);
            var GetContactsCount = account.GetContacts(null, null, dc);

            // Assert
            Assert.True(GetContactsCount.Count == 1);
        }

        [Fact]
        public void GetAttendanceRegistrantTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);

            // Act
            var expected =
                DefaultAccount.GetAttendanceRegistrant(activity, attendance.ID.Value, registrant.ID.Value, msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(expected.ID, attendanceRegistrant.ID);
        }

        [Fact]
        public void GetAttendanceRegistrantErrorTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);

            // Act
            activity.AccountId = -1;
            msgs.AddRange(activity.Save(dc));
            var expectedError =
                DefaultAccount.GetAttendanceRegistrant(activity, attendance.ID.Value, registrant.ID.Value, msgs, dc);

            // Assert
            Assert.Null(expectedError);
            Assert.True(msgs.HasErrors());
        }

        [Fact]
        public void GetAttendanceRegistrantByDateRangeTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);
            var dateRange = new DateRange(
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(1)
            );

            // Act
            var expected =
                DefaultAccount.GetAttendanceRegistrantByDateRange(activity, registrant.ID.Value, dateRange, msgs, dc);

            // Assert
            Assert.NotNull(expected);
            Assert.Equal(expected.FirstOrDefault().ID.Value, attendanceRegistrant.ID);
        }

        [Fact]
        public void GetAttendanceRegistrantByDateRangeErrorTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var attendance = AttendanceFactory.Create(activity, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var attendanceRegistrant = AttendanceRegistrantFactory.Create(attendance, registrant, msgs, dc);
            var dateRange = new DateRange(
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(1)
            );

            // Act
            activity.AccountId = -1;
            msgs.AddRange(activity.Save(dc));
            var expectedError =
                DefaultAccount.GetAttendanceRegistrantByDateRange(activity, registrant.ID.Value, dateRange, msgs, dc);

            // Assert
            Assert.Null(expectedError);
            Assert.True(msgs.HasErrors());
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripe is activated test")]
        public void UseStripeConnectDuringCheckoutWithStripeTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, true, true, msgs, dc);

            //UseStripeConnectedAccount is true for this package, so this test case will pass since it meets all criteria
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs, dc);

            // Assert
            Assert.True(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripeChargesEnabled test")]
        public void UseStripeConnectDuringCheckoutWithStripeChargesEnabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, true, true, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs, dc);

            // Assert
            Assert.True(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripeCharges is disabled test")]
        public void UseStripeConnectDuringCheckoutWithStripeChargesDisabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, false, true, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs, dc);

            // Assert
            Assert.False(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout StripePayouts Enabled test")]
        public void UseStripeConnectDuringCheckoutWithStripePayoutsEnabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, true, true, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs, dc);

            // Assert
            Assert.True(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout StripePayouts is Disabled test")]
        public void UseStripeConnectDuringCheckoutWithStripePayoutsDisabledTest()
        {
            // Arrange
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, false, false, msgs, dc);
            var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

            // Act
            var expected = account.UseStripeConnectDuringCheckout(msgs, dc);

            // Assert
            Assert.False(expected);
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout Empty account test")]
        public void UseStripeConnectDuringCheckoutEmptyAccountTest()
        {
            // Arrange && Act
            var expected = AccountService.UseStripeConnectDuringCheckout(null, msgs, dc);

            // Assert
            Assert.False(expected);
        }

        [Fact]
        public void GetRegistrationTest()
        {
            // Arrange
            var activity = ActivityFactory.Create(DefaultAccount, msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var registration = RegistrationFactory.Create(DefaultAccount, registrant, msgs, dc);

            // Act
            var expected = DefaultAccount.GetRegistration(registration.Uid, msgs, dc);

            // Assert
            Assert.Equal(expected.ID, registration.ID);
            Assert.Equal(expected.Uid, registration.Uid);
            Assert.Equal(expected.RegistrantId, registration.RegistrantId);
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
                User,
                msgs,
                dc);

            // Act
            var expected = visitorAccount.IsApprovedAsVisitorTo(hostAccount, msgs, dc);

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
                User,
                msgs,
                dc);

            // Act
            var expected = visitorAccount.IsApprovedAsVisitorTo(hostAccount, msgs, dc);

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
            var payerAccount =
                AccountFactory.CreateWithFreePackage(purchasingAccountOwner, AccountType.School, msgs, dc);

            var transaction = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.CreditCard, amount, 0,
                0, billerAccount.ID.Value, billerAccount.ID.Value, convenienceFee, paymentTypeFee, msgs, dc);

            // Act
            var expected = billerAccount.GetMaxAmountAvailableForRefund(null, msgs: msgs, dc: dc);

            // Assert
            Assert.Equal(expected, amount + convenienceFee);
        }

        [Theory(DisplayName = "CreatePurchaseTransactionPayment test", Skip = "Stripe Dependency")]
        [Trait("Category", "Stripe")]
        [InlineData(100, 6, 4, 110, 100, false)] //no pro-rate
        [InlineData(100, 6, 4, 90, 83.33,
            true)] //assume user joins mid-period and so as we have to keep pro-rated amount, its 94.3m(otherwise it would 100m)
        [InlineData(10, 2, 0, 12, 10, false)] //no pro-rate
        [InlineData(10, 2, 0, 9.7, 9.1, true)]
        public void CreatePurchaseTransactionPaymentTest(decimal amount, decimal convenienceFee, decimal paymentTypeFee,
            decimal amountPaidByParent, decimal amountReceivedByProvider, bool isAmountProRated = false)
        {
            // Arrange
            var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
            var purchasingAccountOwner =
                WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
            var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com",
                billingAccountOwner, AccountType.School, msgs, dc);
            var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account",
                "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
            var masterRegistrant = RegistrantFactory.Create(purchasingAccount, true, msgs, dc);

            var activity = ActivityFactory.Create(billingAccount, msgs, dc);
            var activityGroup = ActivityGroupFactory.Create(billingAccount, ActivityGroupType.Session, msgs, dc);
            var registrant = RegistrantFactory.Create(billingAccount, false, msgs, dc);
            var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
            var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc);

            var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity,
                SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);
            purchasingAccount.CreateStripeCustomerPlatform(msgs, dc);

            var purchase = PurchaseFactory.Create(purchasingAccount, amount, registrant.ID.Value, registration.ID.Value,
                msgs, dc); // check: we are inserting row amount here
            var registrantSubscriptionPlan = RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan,
                purchase, amount, convenienceFee, paymentTypeFee, msgs, dc);
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
            var checkoutObject = new CheckoutObject
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
                SubscriptionPlan = subscriptionPlan.PlanTypeId == SubscriptionPlanType.Subscription
                    ? subscriptionPlan
                    : null,
                PaymentPlan = subscriptionPlan.PlanTypeId == SubscriptionPlanType.PaymentPlan ? subscriptionPlan : null,
                MasterRegistrant = masterRegistrant,
                NumberOfSeats = 1,
                Registration = registration,
                RegistrantSubscriptionPlan = registrantSubscriptionPlan,
                ConvenienceFeePayer = convenienceFeePayer,
                CreditCardFeePayer = transactionFeePayer,
                Purchase = purchase,
            };

            // Act
            AccountService.CreatePurchaseTransactionPayment(checkoutObject, User, msgs, dc);

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
            Assert.Equal(subscriptionPlan?.CostPerPeriod,
                amountPaidByParent); //here subscription plan will have amount with all fees
            Assert.Equal(subscriptionPlan?.PlanOwnerAccountId, billingAccount.ID.Value);
            Assert.Equal(BillingPeriodType.Month, subscriptionPlan?.BillingPeriodTypeId);
            Assert.Equal(SubscriptionPlanType.Subscription, subscriptionPlan?.PlanTypeId);

            //transaction
            var transaction = Transaction.LoadByBillerAccountIdByIsDeleted(billingAccount.ID.Value, false, msgs, dc)
                .FirstOrDefault();
            Assert.NotNull(transaction);
            Assert.Equal(transaction?.PurchaseId, purchase.ID.Value);
            Assert.Equal(transaction?.PaymentTypeFeeCharged, paymentTypeFee);
            Assert.Equal(transaction?.ConvenienceFeeCharged, convenienceFee);
            Assert.Equal(transaction?.PaymentTypeFeePayerAccountId, transactionFeePayer.ID.Value);
            Assert.Equal(transaction?.ConvenienceFeePayerAccountId, convenienceFeePayer.ID.Value);
            Assert.Equal(transaction?.BillerAccountId, billingAccount.ID.Value);
            Assert.Equal(transaction?.PayerAccountId, purchasingAccount.ID.Value);

            //activityPayment
            var activityPayment = ActivityPayment
                .LoadByParticipationObjectByRegistrant(participationObject.ObjectID.Value,
                    participationObject.ObjectType, registrant.ID.Value, false, msgs, dc).FirstOrDefault();
            Assert.NotNull(activityPayment);
            Assert.Equal(activityPayment?.Amount, amountReceivedByProvider);
            Assert.Equal(activityPayment?.AmountPaidByParent, amountPaidByParent);
            Assert.Equal(activityPayment?.ParticipationObjectId, participationObject.ObjectID.Value);
            Assert.Equal(activityPayment?.IsProRated, isAmountProRated);
            Assert.Equal(activityPayment?.RegistrantId, registrant.ID.Value);

            //Stripe
            var customer = purchasingAccount.GetStripeCustomerPlatform(msgs);
            Assert.NotNull(customer);

            var product = new StripeHelper().GetProduct(activity.Uid.ToString(), null, msgs);
            Assert.NotNull(product);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void CreateRegistrantTest()
        {
            // Arrange
            var user = WebUserFactory.Create(msgs, dc);
            var webUser = WebUserFactory.Create(true, msgs, dc);
            var canBeAccessedBy = WebUser.LoadByIsActive(true, msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);

            // Act                
            var newRegistrant = account.CreateRegistrant("Allen", "Donald", "test@test.com", user, canBeAccessedBy,
                RegistrantType.Individual, false, false, true, 6067, RegistrantProfileStatus.Completed, webUser, msgs,
                dc);

            // Assert
            Assert.NotNull(newRegistrant.ID);
            Assert.NotNull(newRegistrant.WebUserId);
            Assert.True(newRegistrant.IsDeleted == false);
        }

        [Fact]
        public void GetOpenPublishedSessionsTest()
        {
            var activityGroup1 = ActivityGroupFactory.Create(DefaultAccount, ActivityGroupType.Session, msgs, dc);
            var activityGroup2 = ActivityGroupFactory.Create(DefaultAccount, ActivityGroupType.Session, msgs, dc);
            var activityGroup3 = ActivityGroupFactory.Create(DefaultAccount, ActivityGroupType.Session, msgs, dc);
            var regPeriod1 = RegistrationPeriodFactory.Create(DefaultAccount, activityGroup1, msgs, dc);
            var regPeriod2 = RegistrationPeriodFactory.Create(DefaultAccount, activityGroup2, msgs, dc);
            var regPeriod3 = RegistrationPeriodFactory.Create(DefaultAccount, activityGroup2,
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(4), msgs, dc);

            msgs.AddRange(activityGroup1.SetIsPublished(true, User.ID.Value, dc));
            msgs.AddRange(activityGroup2.SetIsPublished(true, User.ID.Value, dc));
            msgs.AddRange(activityGroup3.SetIsPublished(true, User.ID.Value, dc));

            activityGroup2.SetIsDeleted(User.ID.Value, true, msgs, dc);

            var sessions = DefaultAccount.GetOpenPublishedSessions(msgs, dc);

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

        [Fact(DisplayName = "Test RequestNewPayout with Check")]
        public void RequestNewPayoutCheckTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerUser = WebUserFactory.Create(msgs, dc);
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var billerAccount = AccountFactory.Create(billerUser, msgs, dc);
            var transaction = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.Check, 100,
                0, 0, billerUser.ID.Value, payerAccount.ID.Value,
                7, 4, msgs, dc);

            // Act
            var payout =
                billerAccount.RequestNewPayout("Ahq Payout - Test", transaction.AsList(), loggedInUser, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(payout);
            Assert.NotNull(payout.ID);
            Assert.Equal("Ahq Payout - Test", payout.StatementDescriptor);
            Assert.Equal(PaymentType.Check, payout.PaymentTypeId);
            Assert.Equal(PayoutStatus.Requested, payout.PayoutStatusId);

            var payoutTransactions = payout.GetTransactions(msgs, dc);
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
            var billerAccount = AccountFactory.CreateAccountWithStripeFields(loggedInUser, AccountType.ActivityProvider,
                stripePayoutsEnabled: true, stripeChargesEnabled: true, stripeUserId: "1", msgs: msgs, dc: dc);
            var transaction = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.ACH, 100,
                0, 0, billerUser.ID.Value, payerAccount.ID.Value,
                7, 4, msgs, dc);

            // Act
            var payout =
                billerAccount.RequestNewPayout("Ahq Payout - Test", transaction.AsList(), loggedInUser, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(payout);
            Assert.NotNull(payout.ID);
            Assert.Equal("Ahq Payout - Test", payout.StatementDescriptor);
            Assert.Equal(PaymentType.ACH, payout.PaymentTypeId);
            Assert.Equal(PayoutStatus.Requested, payout.PayoutStatusId);

            var payoutTransactions = payout.GetTransactions(msgs, dc);
            Assert.Single(payoutTransactions);
            Assert.Equal(payoutTransactions.FirstOrDefault().ID, transaction.ID);
        }

        [Fact(DisplayName = "Failing Test case for DisableStripePayoutForConnectAccountFailureTest")]
        public void DisableStripePayoutForConnectAccountFailureTest()
        {
            // Arrange
            var loggedInUser = WebUserFactory.Create(msgs, dc);
            var billerAccount = AccountFactory.CreateAccountWithStripeFields(loggedInUser, AccountType.ActivityProvider,
                stripePayoutsEnabled: true, stripeChargesEnabled: true, stripeUserId: "1", msgs: msgs, dc: dc);

            // Act
            var stripeAccount = new StripeHelper().ToggleStripePayoutForConnectedAccount(billerAccount.StripeUserId,
                billerAccount.Name, PayoutIntervalType.Daily, msgs);

            // Assert
            Assert.True(msgs.HasErrors()); //stripe account will not found
            Assert.Null(stripeAccount);
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
            var accountWebUser =
                AccountWebUserFactory.Create(accountUser, instructorWebUser, UserType.Instructor, true, msgs, dc);
            var arrangeActivities = new List<Activity>();


            for (var i = 0; i < numberOfActivities; i++)
            {
                var activityGroup = ActivityGroupFactory.Create(accountUser, ActivityGroupType.Session, msgs, dc);
                var activity = ActivityFactory.Create(accountUser, ActivityType.Activity, activityGroup, msgs, dc);

                if (i % 2 == 0)
                    ActivityWebUserFactory.Create(activity, instructorWebUser, false, msgs, dc);

                arrangeActivities.Add(activity);
            }

            // Act
            var actActivities = accountUser.GetActivities(accountWebUser.UserTypeId, instructorWebUser, "",
                ActivityType.Activity, false, false, false, start, length, msgs, dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.Equal(length, actActivities.Count);
        }

        [Fact]
        public void UpdateUserTypeTest()
        {
            //Arrage
            var user = WebUserFactory.Create(true, msgs, dc);
            var webUser = WebUserFactory.Create(true, msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);

            var accountWebUser = AccountWebUserFactory.Create(account, user, UserType.Administrator, true, msgs, dc);

            //Act
            accountWebUser.UpdateUserType(webUser, "6", msgs, dc);

            //Assert
            Assert.True(accountWebUser.UserTypeId == UserType.Staff);
            Assert.True(accountWebUser.LastModifiedBy == webUser.ID);
        }

        [Fact(DisplayName = "CanPublishFreeActivityTest")]
        public void CanPublishFreeActivityTest()
        {
            var package = PackageFactory.Create(msgs, dc);
            PackageFeatureFactory.Create(package, FeatureType.FreeActivityLimit, 1, msgs, dc);
            AccountPackageFactory.Create(DefaultAccount, package, msgs, dc);
            var publishedFreeActivity = ActivityFactory.Create(DefaultAccount, 0.ToDecimal(), msgs, dc);
            var registrationPeriod = RegistrationPeriodFactory.Create(DefaultAccount, msgs, dc);
            ActivityRegistrationPeriodFactory.Create(publishedFreeActivity, registrationPeriod, msgs, dc);
            var activitySchedule = ActivityScheduleFactory.Create(DefaultAccount, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1), msgs,
                    dc);
            var scheduleEndedFreeActivity = ActivityFactory.Create(DefaultAccount, activitySchedule, 0, msgs, dc);
            publishedFreeActivity.Publish(User, msgs, dc);

            var publishedFreeActivityResult = DefaultAccount.CanPublishFreeActivity(msgs, dc);
            publishedFreeActivity.Unpublish(User, msgs, dc);
            scheduleEndedFreeActivity.Publish(User, msgs, dc);
            var scheduleEndedFreeActivityResult = DefaultAccount.CanPublishFreeActivity(msgs, dc);

            Assert.False(publishedFreeActivityResult);
            Assert.True(scheduleEndedFreeActivityResult);
        }
    }
}