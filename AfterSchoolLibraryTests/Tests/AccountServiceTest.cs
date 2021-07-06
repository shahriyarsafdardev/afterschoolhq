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

            Assert.Single(Account.LoadByAccountTypeIds(accountTypes, false, null, null, msgs, dc));
        }

        //[Theory(DisplayName = "Get Platform Flat Fee")]
        //[InlineData(.99, 1, .99)]
        //[InlineData(3, 1, 3)]
        //[InlineData(.99, 0, .99)]
        //[InlineData(3, 0, .99)]
        //public void GetPlatformFlatFeeTest(decimal platformFlatFee, decimal stripeConnectedAccountValue, decimal expectedResult)
        //{
        //    var accountOwner = WebUserFactory.Create(msgs, dc);
        //    var account = AccountFactory.Create(accountOwner, msgs, dc);
        //    var package = PackageFactory.Create(msgs, dc);
        //    var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
        //    var accountPackageFeature = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);
        //    var accountPackageFeature2 = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, stripeConnectedAccountValue, msgs, dc);

        //    Assert.Equal(expectedResult, account.GetPlatformFlatFee(msgs, dc));
        //}

        //[Theory(DisplayName = "Get Platform Percent Fee")]
        //[InlineData(.05, 1, .05)]
        //[InlineData(.34, 1, .34)]
        //[InlineData(.05, 0, 0.05)]
        //[InlineData(.34, 0, 0.05)]
        //public void GetPlatformPercentFeeTest(decimal platformPercentFee, decimal stripeConnectedAccountValue, decimal expectedResult)
        //{
        //    var accountOwner = WebUserFactory.Create(msgs, dc);
        //    var account = AccountFactory.Create(accountOwner, msgs, dc);
        //    var package = PackageFactory.Create(msgs, dc);
        //    var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
        //    var accountPackageFeature = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentFee, msgs, dc);
        //    var accountPackageFeature2 = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, stripeConnectedAccountValue, msgs, dc);

        //    Assert.Equal(expectedResult, account.GetPlatformPercentFee(msgs, dc));
        //}

        //[Theory(DisplayName = "Calculate Platform Flat Fee Amount")]
        //[InlineData(20, 1, 1.99)]
        //[InlineData(100, 1, 5.99)]
        //[InlineData(20, 0, 2.49)]
        //[InlineData(100, 0, 6.49)]
        //public void CalculatePlatformFeeAmountTest(decimal tuitionFee, decimal expectedPlatformFee)
        //{
        //    var accountOwner = WebUserFactory.Create(msgs, dc);
        //    var account = AccountFactory.Create(accountOwner, msgs, dc);
        //    var package = PackageFactory.Create(msgs, dc);
        //    var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
        //    //var apfPlatformFeePercentage = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFee, platformPercentageFee, msgs, dc);
        //    //var apfPlatformFeeFlat = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.PaymentsPercentageFlatFee, platformFlatFee, msgs, dc);
        //    //var apfStripeConnect = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.UseStripeConnectedAccount, stripeConnectedAccountValue, msgs, dc);

        //    Assert.Equal(expectedPlatformFee, account.CalculatePlatformFeeAmount(tuitionFee, msgs, dc));
        //}        

        /// <summary>
        /// should calculate the percentage that AHQ gets when processing a subscription payment
        /// Should be equal to 
        /// (((cost per period * AHQ percent fee) +  AHQ flat fee) - ((cost per period * stripe percent fee) + stripe flat fee) / cost per period
        /// to get the final percentage
        /// </summary>
        /// <param name="platformFlatFee"></param>
        /// <param name="platformPercentageFee"></param>
        /// <param name="costPerPeriod"></param>
        /// <param name="expectedPlatformPercentage"></param>
        [Theory(DisplayName = "GetSubscriptionPlatformFeePercentTest")]
        [InlineData(.99, .05, 2, 1, 0.366)]
        [InlineData(.99, .05, 20, 1, 0.0555)]
        [InlineData(.99, .05, 100, 1, 0.0279)]
        [InlineData(.99, .05, 2, 0, 0.4975)]
        [InlineData(.99, .05, 20, 0, 0.1482)]
        [InlineData(.99, .05, 100, 0, 0.0907)]
        public void GetSubscriptionPlatformFeePercentTest(decimal platformFlatFee, decimal platformPercentageFee, decimal costPerPeriod, int stripeConnectedAccountValue, decimal expectedPlatformPercentage)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
            var apfPlatformFeePercentage = AccountPackageFeatureFactory.Create(
                accountPackage, 
                FeatureType.PaymentsPercentageFee, 
                platformPercentageFee, 
                msgs, 
                dc);
            var apfPlatformFeeFlat = AccountPackageFeatureFactory.Create(
                accountPackage, 
                FeatureType.PaymentsPercentageFlatFee, 
                platformFlatFee, 
                msgs, 
                dc);
            var apfStripeConnect = AccountPackageFeatureFactory.Create(
                accountPackage, 
                FeatureType.UseStripeConnectedAccount, 
                stripeConnectedAccountValue, 
                msgs, 
                dc);
            var activity = ActivityFactory.Create(account, msgs, dc);
            var subscriptionPlan = SubscriptionPlanFactory.Create(
                account, 
                activity, 
                SubscriptionPlanType.Subscription, 
                costPerPeriod, 
                msgs, 
                dc);
            
            Assert.Equal(expectedPlatformPercentage, account.GetSubscriptionPlatformFeePercent(subscriptionPlan, PaymentType.CreditCard, msgs, dc));
        }

        [Theory]
        [InlineData(StripeVerificationStatus.NotSet, true, false)]
        [InlineData(StripeVerificationStatus.Unverified, true, false)]
        [InlineData(StripeVerificationStatus.Pending, true, true)]
        [InlineData(StripeVerificationStatus.Verified, true, true)]
        [InlineData(StripeVerificationStatus.Pending, false, false)]
        [InlineData(StripeVerificationStatus.Verified, false, false)]
        public void CanProcessPaymentsTest(StripeVerificationStatus stripeVerificationStatus, bool addPaymentFeatureToAccount, bool expectedResult)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var stripeAccount = StripeAccountFactory.Create(account, stripeVerificationStatus, msgs, dc);

            if (addPaymentFeatureToAccount)
            {
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);
                var accountPackageFeature = AccountPackageFeatureFactory.Create(accountPackage, FeatureType.AcceptPayments, 1, msgs, dc);
            }

            Assert.Equal(account.CanProcessPayments(msgs, dc), expectedResult);

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

            Assert.Equal(expectedResult, account.HasAccountPackageFeature(featureType, msgs, dc));
        }

        [Fact]
        public void CreateStripeCustomAccountTestIndividual()
        {
            // school mcc code: 8211
            // educational services: 8299

            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            var dob = DateTime.UtcNow.AddYears(-25);

            var createOptions = new Stripe.AccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                DefaultCurrency = "usd",
                Email = "test@test.com",
                BusinessProfile = new Stripe.AccountBusinessProfileOptions()
                {
                    Url = "www.thebluehouse.co",
                    //SupportEmail = "support@email.com",
                    //SupportPhone = "317-316-0596",
                    //SupportUrl = "www.support.com",
                    Mcc = "8299",
                    Name = "The Blue House"
                },
                BusinessType = LegalEntityType.Individual.ToString().ToLower(),
                Individual = new Stripe.PersonCreateOptions()
                {
                    Address = new Stripe.AddressOptions()
                    {
                        City = "Fishers",
                        Line1 = "10864 Knightsbridge Lane",
                        Line2 = "",
                        Country = "US",
                        PostalCode = "46037",
                        State = "IN"
                    },
                    FirstName = "Darye",
                    LastName = "Henry",
                    Phone = "317-809-2345",
                    SSNLast4 = "0000",
                    Email = "darye@darye.com",
                    Dob = new Stripe.DobOptions()
                    {
                        Day = dob.Day,
                        Month = dob.Month,
                        Year = dob.Year
                    },
                },                
                Settings = new Stripe.AccountSettingsOptions()
                {
                    Payouts = new Stripe.AccountSettingsPayoutsOptions()
                    {
                        DebitNegativeBalances = true,
                        StatementDescriptor = "AHQ transfer".MaxChars(22),
                        Schedule = new Stripe.AccountSettingsPayoutsScheduleOptions()
                        {
                            Interval = "manual",
                        }
                    }
                },
                RequestedCapabilities = new List<string>() { "card_payments" },
                TosAcceptance = new Stripe.AccountTosAcceptanceOptions()
                {
                    Date = DateTime.UtcNow,
                    Ip = "1.1.1.1",
                    UserAgent = "Chrome"
                },
                ExternalAccount = new Stripe.AccountBankAccountOptions()
                {
                    RoutingNumber = "110000000",
                    AccountNumber = "000123456789",
                    Country = "US"
                }
            };

            var stripeAccount = account.CreateStripeCustomAccount(createOptions, msgs, dc);

            stripeAccount = account.GetStripeCustomAccount(msgs);

            msgs.Add($"Status: {stripeAccount.Individual.Verification.Status}", MessageType.Warning);

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
            // school mcc code: 8211
            // educational services: 8299

            var createOptions = new Stripe.AccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                DefaultCurrency = "usd",
                Email = "test@test.com",
                BusinessProfile = new Stripe.AccountBusinessProfileOptions()
                {
                    Url = "www.thebluehouse.co",
                    //SupportEmail = "support@email.com",
                    //SupportPhone = "317-316-0596",
                    //SupportUrl = "www.support.com",
                    Mcc = "8299"
                },
                BusinessType = LegalEntityType.Company.ToString().ToLower(),
                Company = new Stripe.AccountCompanyOptions()
                {
                    Address = new Stripe.AddressOptions()
                    {
                        City = "Fishers",
                        Line1 = "12175 Visionary Way",
                        Line2 = "#710",
                        Country = "US",
                        PostalCode = "46038",
                        State = "IN"
                    },
                    Phone = "317-316-0596",
                    TaxId = "23-1231233",
                    Name = "The Blue House"
                },
                Settings = new Stripe.AccountSettingsOptions()
                {
                    Payouts = new Stripe.AccountSettingsPayoutsOptions()
                    {
                        DebitNegativeBalances = true,
                        StatementDescriptor = "AHQ transfer".MaxChars(22),
                        Schedule = new Stripe.AccountSettingsPayoutsScheduleOptions()
                        {
                            Interval = "manual",
                        }
                    }
                },
                RequestedCapabilities = new List<string>() { "card_payments" },
                TosAcceptance = new Stripe.AccountTosAcceptanceOptions()
                {
                    Date = DateTime.UtcNow,
                    Ip = "1.1.1.1",
                    UserAgent = "Chrome"
                },
                ExternalAccount = new Stripe.AccountBankAccountOptions()
                {
                    RoutingNumber = "110000000",
                    AccountNumber = "000123456789",
                    Country = "US"
                }
            };

            var stripeAccount = StripeHelper.CreateCustomAccount(createOptions, msgs);

            var dob = DateTime.UtcNow.AddYears(-25);
            var personCreateOptions = new Stripe.PersonCreateOptions()
            {
                FirstName = "Darye",
                LastName = "Henry",
                Phone = "317-809-2345",
                SSNLast4 = "0000",
                Email = "darye@darye.com",
                Dob = new Stripe.DobOptions()
                {
                    Day = dob.Day,
                    Month = dob.Month,
                    Year = dob.Year
                },
                Relationship = new Stripe.PersonRelationshipOptions()
                {
                    // AccountOpener = true,
                    Title = "President",
                    Owner = true,
                    PercentOwnership = 100
                },
                Address = new Stripe.AddressOptions()
                {
                    City = "Fishers",
                    Line1 = "12175 Visionary Way",
                    Line2 = "#710",
                    Country = "US",
                    PostalCode = "46038",
                    State = "IN"
                },
            };

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

        [Theory(DisplayName= "GetBillerAccountTransactionsTest")]
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

        [Theory(DisplayName ="GetAllActivitiesForAccountTest")]
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
            Assert.Equal(expected: expected.ID, actual: activity.ID);
            Assert.Equal(expected: expected.Uid, actual: activity.Uid);
            Assert.Equal(expected: expected.Name, actual: activity.Name);
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

        [Fact]
        public void GetActivityGroupTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetRegistrantTest()
        {
            try
            {
                // Arrange
                var registrant = RegistrantFactory.Create(account, msgs, dc);

                // Act                
                var expected = account.GetRegistrant(Uid: registrant.Uid, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: registrant.ID);
                Assert.Equal(expected: expected.Uid, actual: registrant.Uid);
                Assert.Equal(expected: expected.Email, actual: registrant.Email);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetPayoutTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetCouponTest()
        {
            try
            {
                // Arrange
                var coupon = CouponFactory.Create(account, msgs, dc);

                // Act 
                var expected = account.GetCoupon(coupon.Uid, msgs, dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: coupon.ID);
                Assert.Equal(expected: expected.Uid, actual: coupon.Uid);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void GetCouponsTest(int expectedNumberOfCoupons)
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetRefundRequestTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetRefundRequestsTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void LogActivityTest()
        {
            try
            {
                // Act 
                SessionVariables.WebSession.LogActivity(
                    account: account,
                    actor: user,
                    actionType: WebSessionActionType.Navigated,
                    objectType: ObjectType.Account,
                    indirectObjectType: ObjectType.DirectoryAccount,
                    objectId: 1,
                    objectText: "Test",
                    indirectObjectId: 1,
                    indirectObjectText: "ISYP",
                    msgs: msgs, 
                    dc: dc);

                var isSaved = msgs.HasNoErrors();

                // Assert
                Assert.True(isSaved);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetAccountPackageTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetAccountRelationshipTest()
        {
            try
            {
                // Arrange
                var visitorOwner = WebUserFactory.Create(msgs, dc);
                var visitorAccount = AccountFactory.Create(visitorOwner, msgs, dc);
                var hostOwner = WebUserFactory.Create(msgs, dc);
                var hostAccount = AccountFactory.Create(hostOwner, msgs, dc);
                var accountRelationship = AccountRelationshipService.CreateNew(hostAccount, visitorAccount, ApprovalStatus.Approved, HostPermissionType.Edit, user, msgs, dc);
                
                // Act                
                var expected = hostAccount.GetAccountRelationship(Id: accountRelationship.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: accountRelationship.ID);                
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetFormTest()
        {
            try
            {
                // Arrange                
                var form = FormFactory.Create(account, msgs, dc);

                // Act                
                var expected = account.GetForm(Id: form.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: form.ID);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetAccountWebUserTest()
        {
            try
            {
                // Arrange                
                var webUser = AccountWebUserFactory.Create(account, user, UserType.Student, true, msgs, dc);

                // Act                
                var expected = account.GetAccountWebUser(webUserId: user.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: webUser.ID);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "Get Account Owner test")]
        public void GetAccountOwnerTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);

                // Act
                var GetOwner = AccountService.GetAccountOwner(account, msgs, dc);

                // Assert
                Assert.NotNull(GetOwner);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "HasFeature test")]
        public void HasFeatureTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.CreateWithFreePackage(accountOwner, AccountType.Church, msgs, dc);

                // Act
                var AccountWithFeature = account.HasFeature(FeatureType.AcceptACH, msgs, dc);

                // Assert
                Assert.True(AccountWithFeature);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "HasFeature False Case Test")]
        public void HasFeatureEmptyAccountTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetActiveAccountPackage test")]
        public void GetActiveAccountPackageTest()
        {
            try
            {
                // Arrange
                var package = PackageFactory.Create(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

                // Act
                var expected = account.GetActiveAccountPackage(msgs: msgs, dc: dc);

                // Assert
                Assert.NotNull(expected);
                Assert.Equal(accountPackage.PackageName, expected.PackageName);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetActiveAccountPackage test 2")]
        public void GetActiveAccountPackageEmptyAccountTest()
        {
            try
            {
                // Arrange & Act
                var expected = AccountService.GetActiveAccountPackage(account: null, msgs: msgs, dc: dc);

                // Assert
                Assert.Null(expected);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetCheckPaymentAccount test")]
        public void GetCheckPaymentAccountTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetCheckPaymentAccount test 2")]
        public void GetCheckPaymentAccountEmptyAccountTest()
        {
            try
            {
                // Arrange & Act
                var expected = AccountService.GetCheckPaymentAccount(account: null, msgs: msgs, dc: dc);

                // Assert
                Assert.Null(expected);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetActivityCount test")]
        public void GetActivityCountTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetActiveAndPublishedActivityCount test")]
        public void GetActiveAndPublishedActivityCountTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);
                var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);

                // Act
                var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
                addactivity.Publish(accountOwner, msgs, dc);
                var GetActivityCount = AccountService.GetActiveAndPublishedActivityCount(account, msgs, dc);

                // Assert
                Assert.True(GetActivityCount > 0);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetAllActiveAndNonPublishedActivities test")]
        public void GetAllActiveAndNonPublishedActivitiesTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetActiveAndNonPublishedActivityCount test")]
        public void GetActiveAndNonPublishedActivityCountTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetUnpublishActivityCount test")]
        public void GetUnpublishActivityCountTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "HasActivities test")]
        public void HasActivitiesTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "HasActiveActivities test")]
        public void HasActiveActivitiesTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "AddContact test")]
        public void AddContactTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);

                // Act
                var addContact = AccountService.AddContact(account, "Test", "Lastname", "test@test.com", "123-123-1234", accountOwner, msgs, dc);

                // Assert
                Assert.NotNull(addContact);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "SearchContactCount test")]
        public void SearchContactTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetContactCount test")]
        public void GetContactCountTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "LoadByActivitiesbyActivityGroupByAccountId test")]
        public void LoadByActivitiesbyActivityGroupByAccountIdTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);
                var activityGroup = ActivityGroupFactory.Create(account, ActivityGroupType.Session, msgs, dc);
                var addactivity = ActivityFactory.Create(account, activityGroup, msgs, dc);
                addactivity.Publish(accountOwner, msgs, dc);

                // Act
                var activityList = AccountService.LoadByActivitiesbyActivityGroupByAccountId(account, addactivity.ID.Value, msgs, dc);              

                // Assert
                Assert.True(activityList.Count() == 1);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }


        [Fact(DisplayName = "GetRegistrationPeriod test")]
        public void GetRegistrationPeriodTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Theory(DisplayName = "GetRegistrationPeriodsByGroupId Test")]
        [InlineData(1)]
        [InlineData(5)]
        public void GetRegistrationPeriodsByGroupIdTest(int expectedNumberOfResistrationPeriod)
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }


        [Fact(DisplayName = "DoesContactExist test")]
        public void DoesContactExistTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "GetContacts test")]
        public void GetContactsTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
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
            Assert.Equal(expected: expected.FirstOrDefault().ID.Value , actual: attendanceRegistrant.ID);
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
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout test")]
        public void UseStripeConnectDuringCheckoutTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: true, msgs, dc);
                var package = PackageFactory.CreateFreePackage(msgs, dc); //UseStripeConnectedAccount is false for this package, so this test case will fail
                var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

                // Act
                var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

                // Assert
                Assert.False(expected);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripeChargesEnabled test")]
        public void UseStripeConnectDuringCheckoutWithStripeChargesEnabledTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout stripeCharges is disabled test")]
        public void UseStripeConnectDuringCheckoutWithStripeChargesDisabledTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout StripePayouts Enabled test")]
        public void UseStripeConnectDuringCheckoutWithStripePayoutsEnabledTest()
        {
            try
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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout StripePayouts is Disabled test")]
        public void UseStripeConnectDuringCheckoutWithStripePayoutsDisabledTest()
        {
            try
            {
                // Arrange
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.CreateAccount(accountOwner, AccountType.School, stripeChargesEnabled: true, stripePayoutsEnabled: false, msgs, dc);
                var package = PackageFactory.CreateFreePackageWithUseStripeConnectedAccount(msgs, dc);
                var accountPackage = AccountPackageFactory.Create(account, package, msgs, dc);

                // Act
                var expected = account.UseStripeConnectDuringCheckout(msgs: msgs, dc: dc);

                // Assert
                Assert.False(expected);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "UseStripeConnectDuringCheckout Empty account test")]
        public void UseStripeConnectDuringCheckoutEmptyAccountTest()
        {
            try
            {
                // Arrange && Act
                var expected = AccountService.UseStripeConnectDuringCheckout(account: null, msgs: msgs, dc: dc);

                // Assert
                Assert.False(expected);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact(DisplayName = "CreatePurchaseTransactionPayment test")]
        public void CreatePurchaseTransactionPaymentTest()
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.Create("Don", "John", "DonJohn@test.com", true, msgs, dc);
                var purchasingAccountOwner = WebUserFactory.Create("Cristine", "Trump", "CristineTrump@test.com", true, msgs, dc);
                var billingAccount = AccountFactory.CreateWithFreePackage("Billing account", "BillingAccount@test.com", billingAccountOwner, AccountType.School, msgs, dc);
                var purchasingAccount = AccountFactory.CreateWithFreePackage("Purchasing account", "PurchasingAccount@test.com", purchasingAccountOwner, AccountType.School, msgs, dc);
                var masterRegistrant = RegistrantFactory.Create(purchasingAccount, msgs, dc);

                var activity = ActivityFactory.Create(billingAccount, msgs, dc: dc);
                var activityGroup = ActivityGroupFactory.Create(billingAccount, ActivityGroupType.Session, msgs, dc);
                var registrant = RegistrantFactory.Create(billingAccount, msgs, dc: dc);
                var registration = RegistrationFactory.Create(billingAccount, registrant, msgs, dc);
                var registrantActivity = RegistrantActivityFactory.Create(activity, registrant, msgs, dc: dc);

                decimal amount = 100m, 
                    convenienceFee = 6m, 
                    paymentTypeFee = 4m,
                    amountPaidByParent = 110m, // with convenience fee + cc fee
                    amountReceivedByProvider = 100m; //assume user joins mid-period and so as we have to keep pro-rated amount, its 94.3m(otherwise it would 100m)
                bool isAmountProRated = false;

                var subscriptionPlan = SubscriptionPlanFactory.Create(billingAccount, activity, SubscriptionPlanType.Subscription, amountPaidByParent, msgs, dc);
                purchasingAccount.CreateStripeCustomer(msgs, dc);

                //var subscriptionPlan = SubscriptionPlanFactory.Create(account, activity, SubscriptionPlanType.Subscription, msgs, dc);
                //var transaction = TransactionFactory.Create(account, account, PaymentType.CreditCard, msgs, dc);
                //var masterRegistrant = registrant.GetMasterRegistrant(msgs, dc);
                //var purchasingAccount = masterRegistrant.GetAccount(msgs, dc);
                //var billingAccount = subscriptionPlan.GetAccount(msgs, dc);

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
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
