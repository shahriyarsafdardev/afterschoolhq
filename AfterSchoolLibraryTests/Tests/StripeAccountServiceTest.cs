using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using AfterSchoolLibrary.Billing;

namespace AfterSchoolLibraryTests.Tests
{
    public class StripeAccountServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account Account { get; set; }
        // private StripeAccount StripeAccount { get; set; }

        public StripeAccountServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            Account = Factories.AccountFactory.Create(AccountOwner, msgs, dc);
            // StripeAccount = Factories.StripeAccountFactory.Create(Account, msgs, dc);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void CanReceivePaymentDepositsTest(bool chargesEnabled, bool payoutsEnabled, bool expectedResult)
        {
            //StripeAccount.ChargesEnabled = chargesEnabled;
            //StripeAccount.PayoutsEnabled = payoutsEnabled;

            //Assert.Equal(expectedResult, StripeAccount.CanReceivePaymentDeposits());
        }

        [Fact]
        public void SaveAndSendBankInfoToStripeTest()
        {
            Assert.True(false);
        }

        [Fact]
        public void GetLatestStripeAccountDetailsTest()
        {
            Assert.True(false);
        }

        [Fact]
        public void SaveAndSendLegalEntityToStripeTest()
        {
            //var package = PackageFactory.Create(msgs, dc);
            //var accountPackage = AccountPackageFactory.Create(Account, package, msgs, dc);

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
                ExternalBankAccount = new Stripe.AccountBankAccountOptions()
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
                    AccountOpener = true,
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

        [Fact]
        public void ParseAndUpdateStripeCustomAccountTest()
        {

            Assert.True(false);
        }

        [Fact]
        public void GetAccount()
        {
            Assert.True(false);
        }

        [Fact]
        public void SendPayout()
        {
            Assert.True(false);
        }
    }
}
