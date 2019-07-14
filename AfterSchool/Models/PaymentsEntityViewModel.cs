using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Models
{
    public class PaymentsEntityViewModel
    {
        public LegalEntityType BusinessType { get; set; }
        public string BusinessName { get; set; }
        public string BusinessUrl { get; set; }
        public string BusinessPhone { get; set; }
        public string EIN { get; set; }

        public string ProductDescription { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string SupportPhone { get; set; }
        public string SupportEmail { get; set; }
        public string StatementDescriptor { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string PersonalEmail { get; set; }
        public string PersonalPhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string SSNLast4 { get; set; }
        public string SSN { get; set; }
        public StripeVerificationStatus VerificationStatus { get; set; }
        public string DocumentFrontId { get; set; }
        public string DocumentBackId { get; set; }

        public bool TOSAccept { get; set; }
        public DateTime? TOSAcceptanceDate { get; set; }
        public string TOSAcceptanceIP { get; set; }
        public string TOSAcceptanceUserAgent { get; set; }

        public bool? RequiresFullSSN { get; set; }
        public bool? RequiresDocument { get; set; }

        public bool PayoutsEnabled { get; set; }
        public bool ChargesEnabled { get; set; }
        public bool Disabled { get; set; }
        public string DisabledReason { get; set; }
        public StripeVerificationFailedCode FailedDetailsCode { get; set; }

        public string BankAccountId { get; set; }
        public string BankName { get; set; }
        public string BankLast4 { get; set; }
        public string BankRoutingNumber1 { get; set; }
        public string BankRoutingNumber2 { get; set; }
        public string BankAccountNumber1 { get; set; }
        public string BankAccountNumber2 { get; set; }

        public PaymentsEntityViewModel()
        {
            FailedDetailsCode = StripeVerificationFailedCode.NotSet;
            BusinessType = LegalEntityType.NotSet;
        }

        public Stripe.AccountCreateOptions GetStripeAccountCreateOptions(Account account)
        {
            if (BusinessType == LegalEntityType.Company)
                return GetStripeAccountCreateOptionsCompany(account);

            return GetStripeAccountCreateOptionsIndividual(account);
        }

        public Stripe.AccountUpdateOptions GetStripeAccountUpdateOptions(Account account)
        {
            if (BusinessType == LegalEntityType.Company)
                return GetStripeAccountUpdateOptionsCompany(account);

            return GetStripeAccountUpdateOptionsIndividual(account);
        }

        public static PaymentsEntityViewModel LoadFromStripeAccount(Stripe.Account stripeAccount)
        {
            if (stripeAccount.IsEmpty())
                return new PaymentsEntityViewModel();

            if (stripeAccount.BusinessType == LegalEntityType.Individual.ToString().ToLower())
                return LoadFromStripeAccountIndividual(stripeAccount);

            return new PaymentsEntityViewModel();
        }

        private static StripeVerificationStatus GetStripeVerificationStatus(Stripe.Account stripeAccount)
        {
            if (stripeAccount.Individual.Verification.Status.IsNotEmpty())
                return stripeAccount.Individual.Verification.Status.Replace("_", " ")
                                                                    .ToProperCase()
                                                                    .Replace(" ", "")
                                                                    .ParseEnum<StripeVerificationStatus>();
            return StripeVerificationStatus.NotSet;
        }

        private static StripeVerificationFailedCode GetStripeVerificationFailedCode(Stripe.Account stripeAccount)
        {
            if (stripeAccount.Individual.Verification.Status.IsNotEmpty())
                return stripeAccount.Individual.Verification.DetailsCode.Replace("_", " ")
                                                                    .ToProperCase()
                                                                    .Replace(" ", "")
                                                                    .ParseEnum<StripeVerificationFailedCode>();
            return StripeVerificationFailedCode.NotSet;
        }

        private static PaymentsEntityViewModel LoadFromStripeAccountIndividual(Stripe.Account stripeAccount)
        {
            if (stripeAccount.IsNotEmpty())
                return new PaymentsEntityViewModel()
                {
                    FirstName = stripeAccount.Individual.FirstName,
                    LastName = stripeAccount.Individual.LastName,
                    PersonalEmail = stripeAccount.Individual.Email,
                    PersonalPhone = stripeAccount.Individual.Phone,
                    Address1 = stripeAccount.Individual.Address.Line1,
                    Address2 = stripeAccount.Individual.Address.Line2,
                    City = stripeAccount.Individual.Address.City,
                    State = BasicLibrary.Utilities.GetStates().Find(state => state.Abbreviation == stripeAccount.Individual.Address.State).Name,
                    PostalCode = stripeAccount.Individual.Address.PostalCode,
                    BusinessName = stripeAccount.BusinessProfile.Name,
                    BusinessUrl = stripeAccount.BusinessProfile.Url,
                    TOSAcceptanceDate = stripeAccount.TosAcceptance.Date,
                    ProductDescription = stripeAccount.BusinessProfile.ProductDescription,
                    DateOfBirth = $"{stripeAccount.Individual.Dob.Month}/{stripeAccount.Individual.Dob.Day}/{stripeAccount.Individual.Dob.Year}".ToDateTime(),
                    BusinessType = LegalEntityType.Individual,
                    StatementDescriptor = stripeAccount.Settings.Payments.StatementDescriptor,
                    RequiresFullSSN = stripeAccount.Requirements.CurrentlyDue?.Contains("individual.id_number"),
                    RequiresDocument = stripeAccount.Requirements.CurrentlyDue?.Contains("individual.verification.document"),
                    ChargesEnabled = stripeAccount.ChargesEnabled,
                    PayoutsEnabled = stripeAccount.PayoutsEnabled,
                    DisabledReason = stripeAccount.Requirements.DisabledReason,
                    Disabled = stripeAccount.Requirements.DisabledReason.IsNotEmpty(),
                    BankAccountId = stripeAccount.ExternalAccounts.FirstOrDefault()?.Id,
                    BankName = ((Stripe.BankAccount)stripeAccount.ExternalAccounts.FirstOrDefault())?.BankName,
                    BankLast4 = ((Stripe.BankAccount)stripeAccount.ExternalAccounts.FirstOrDefault())?.Last4,
                    FailedDetailsCode = GetStripeVerificationFailedCode(stripeAccount),
                    VerificationStatus = GetStripeVerificationStatus(stripeAccount),
                };

            return new PaymentsEntityViewModel();
        }

        private Stripe.AccountCreateOptions GetStripeAccountCreateOptionsIndividual(Account account)
        {            
            return new Stripe.AccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                DefaultCurrency = "usd",
                Email = PersonalEmail,
                BusinessType = BusinessType.ToString().ToLower(),
                BusinessProfile = GetAccountBusinessProfileOptions(account),
                Individual = GetPersonCreateOptions(account),
                Settings = GetAccountSettingsOptions(),
                RequestedCapabilities = GetRequestedCapabilities(),
                TosAcceptance = GetAccountTosAcceptanceOptions(),
                ExternalAccount = GetAccountBankAccountOptions()
            };
        }

        private Stripe.AccountUpdateOptions GetStripeAccountUpdateOptionsIndividual(Account account)
        {
            if (BankAccountNumber1.IsNotEmpty())
                return new Stripe.AccountUpdateOptions()
                {
                    ExternalAccount = GetAccountBankAccountOptions()
                };

            return new Stripe.AccountUpdateOptions()
            {
                DefaultCurrency = "usd",
                Email = PersonalEmail,
                BusinessType = BusinessType.ToString().ToLower(),
                BusinessProfile = GetAccountBusinessProfileOptions(account),
                Individual = GetPersonUpdateOptions(account),
                Settings = GetAccountSettingsOptions(),
                RequestedCapabilities = GetRequestedCapabilities(),
                TosAcceptance = GetAccountTosAcceptanceOptions(),
                ExternalAccount = GetAccountBankAccountOptions()
            };
        }

        private Stripe.AccountCreateOptions GetStripeAccountCreateOptionsCompany(Account account)
        {
            return null;
        }

        private Stripe.AccountUpdateOptions GetStripeAccountUpdateOptionsCompany(Account account)
        {
            return null;
        }

        private string GetMcc(Account account)
        {
            switch (account.AccountTypeId)
            {
                case AccountType.School:
                case AccountType.SchoolDistrict:
                    return "8211";
            }

            return "8299";
        }

        private Stripe.PersonUpdateOptions GetPersonUpdateOptions(Account account)
        {
            return new Stripe.PersonUpdateOptions()
            {
                FirstName = FirstName,
                LastName = LastName,
                Phone = PersonalPhone,
                SSNLast4 = SSNLast4,
                IdNumber = SSN,
                Email = PersonalEmail,
                Dob = new Stripe.DobOptions()
                {
                    Day = DateOfBirth.Value.Day,
                    Month = DateOfBirth.Value.Month,
                    Year = DateOfBirth.Value.Year
                },
                Address = GetAddressOptions(),
                Metadata = GetMetadata(account),
                Verification = new Stripe.PersonVerificationOptions()
                {
                    Document = new Stripe.PersonVerificationDocumentOptions()
                    {
                        FrontFileId = DocumentFrontId,
                        BackFileId = DocumentBackId
                    }
                }
            };
        }

        private Stripe.PersonCreateOptions GetPersonCreateOptions(Account account)
        {
            return new Stripe.PersonCreateOptions()
            {
                FirstName = FirstName,
                LastName = LastName,
                Phone = PersonalPhone,
                SSNLast4 = SSNLast4,
                IdNumber = SSN,
                Email = PersonalEmail,
                Dob = new Stripe.DobOptions()
                {
                    Day = DateOfBirth.Value.Day,
                    Month = DateOfBirth.Value.Month,
                    Year = DateOfBirth.Value.Year
                },
                Address = GetAddressOptions(),
                Metadata = GetMetadata(account),
            };
        }

        private Stripe.AccountBankAccountOptions GetAccountBankAccountOptions()
        {
            if (BankAccountNumber1.IsNotEmpty())
                return new Stripe.AccountBankAccountOptions()
                {
                    RoutingNumber = BankRoutingNumber1,
                    AccountNumber = BankAccountNumber1,
                    Country = "US"
                };

            return null;
        }

        private Stripe.AccountBusinessProfileOptions GetAccountBusinessProfileOptions(Account account)
        {
            return new Stripe.AccountBusinessProfileOptions()
            {
                Mcc = GetMcc(account),
                Url = BusinessUrl,
                SupportEmail = SupportEmail,
                SupportPhone = SupportPhone,
                Name = account.Name,
                ProductDescription = ProductDescription
            };
        }

        private Dictionary<string, string> GetMetadata(Account account)
        {
            return new Dictionary<string, string>()
                {
                    { "AccountUid", account.Uid.ToString() }
                };
        }

        private List<string> GetRequestedCapabilities()
        {
            return new List<string>() { "card_payments" };
        }

        private Stripe.AccountTosAcceptanceOptions GetAccountTosAcceptanceOptions()
        {
            return new Stripe.AccountTosAcceptanceOptions()
            {
                Date = TOSAcceptanceDate,
                Ip = TOSAcceptanceIP,
                UserAgent = TOSAcceptanceUserAgent
            };
        }

        private Stripe.AddressOptions GetAddressOptions()
        {
            return new Stripe.AddressOptions()
            {
                City = City,
                Line1 = Address1,
                Line2 = Address2,
                Country = "US",
                PostalCode = PostalCode,
                State = BasicLibrary.Utilities.GetStates().Find(state => state.Name == State).Abbreviation
            };
        }

        private Stripe.AccountSettingsOptions GetAccountSettingsOptions()
        {
            return new Stripe.AccountSettingsOptions()
            {
                Payouts = new Stripe.AccountSettingsPayoutsOptions()
                {
                    DebitNegativeBalances = true,
                    StatementDescriptor = "AHQ transfer".MaxChars(22),
                    Schedule = new Stripe.AccountSettingsPayoutsScheduleOptions()
                    {
                        Interval = "manual",
                    }
                },
                Payments = new Stripe.AccountSettingsPaymentsOptions()
                {
                    StatementDescriptor = StatementDescriptor.MaxChars(25)
                }
            };
        }
    }
}