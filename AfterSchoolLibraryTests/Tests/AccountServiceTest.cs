using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using System;
using AfterSchoolLibrary.Billing;
using BasicLibrary;

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


        [Fact(DisplayName = "GetPayoutTest")]
        public void GetPayoutTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);

            var expectedPayout = Factories.PayoutFactory.Create(account, msgs, dc);
            var actualPayout = account.GetPayout(expectedPayout.Uid, msgs, dc);

            Assert.Equal(expectedPayout.ID, actualPayout.ID);
        }


        [Fact(DisplayName = "GetRegistrantTest")]
        public void GetRegistrantTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            
            var expectedRegistrant = Factories.RegistrantFactory.Create(account, msgs, dc);
            var actualRegistrant = account.GetRegistrant(expectedRegistrant.Uid, msgs, dc);

            Assert.Equal(expectedRegistrant.ID, actualRegistrant.ID);
        }

        [Fact(DisplayName = "GetRefundRequestTest")]
        public void GetRefundRequestTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var payingUser = Factories.WebUserFactory.Create(msgs, dc);
            var billingAccount = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var payingAccount = Factories.AccountFactory.CreateWithFreePackage(payingUser, AccountType.Registrant, msgs, dc);
            var transaction = Factories.TransactionFactory.Create(billingAccount, payingAccount, PaymentType.ACH, msgs, dc);
           
            var expectedRefundRequest = Factories.RefundRequestFactory.Create(transaction.Amount.Value, transaction, billingAccount, "reason", payingUser, msgs, dc);
            var actualRefundRequest = billingAccount.GetRefundRequest(expectedRefundRequest.Uid, msgs, dc);

            Assert.Equal(expectedRefundRequest.ID, actualRefundRequest.ID);
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

        [Fact(DisplayName = "GetCouponTest")]
        public void GetCouponTest()
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);

            var expectedCoupon = Factories.CouponFactory.Create(account, msgs, dc);
            var actualCoupon = account.GetCoupon(expectedCoupon.Uid, msgs, dc);

            Assert.Equal(expectedCoupon.ID, actualCoupon.ID);
        }

        [Theory(DisplayName = "GetCouponsTest")]
        [InlineData(1)]
        [InlineData(5)]
        public void GetCouponsTest(int expectedNumberOfCoupons)
        {
            var loggedInUser = Factories.WebUserFactory.Create(msgs, dc);
            var account = Factories.AccountFactory.CreateWithFreePackage(loggedInUser, AccountType.ActivityProvider, msgs, dc);
            var expectedCoupons = new List<Coupon>();

            for (int i = 0; i < expectedNumberOfCoupons; i++)
            {
                var expectedCoupon = Factories.CouponFactory.Create(account, msgs, dc);
                expectedCoupons.Add(expectedCoupon);
            }

            var actualNumberOfCoupons = account.GetCoupons(msgs, dc);

            Assert.Equal(expectedCoupons.Count, actualNumberOfCoupons.Count);
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

    }
}
