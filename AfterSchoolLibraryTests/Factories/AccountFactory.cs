using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System;
using static AfterSchoolLibrary.Services.AccountService;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Factories
{
    public class AccountFactory
{
        public static Account Create(WebUser user, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Account Create(WebUser user, AccountType accountType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com", AccountTypeId = accountType };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        
        public static Account CreateAccount(WebUser user, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com", AccountTypeId = AccountType.Business, ConvenienceFeesPaidBy = FeesPaidByType.Parent, CreditCardFeesPaidBy = FeesPaidByType.Parent };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Account CreateAccount(WebUser user, AccountType accountType, bool stripeChargesEnabled, bool stripePayoutsEnabled, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com", AccountTypeId = accountType, StripeChargesEnabled = stripeChargesEnabled, StripePayoutsEnabled = stripePayoutsEnabled };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Account CreateAccountWithFreePackage(string accountName, string accountEmail, WebUser user, AccountType accountType, bool stripeChargesEnabled, bool stripePayoutsEnabled, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = accountName, Email = accountEmail, IsDeleted = false,
                AccountTypeId = accountType, StripeChargesEnabled = stripeChargesEnabled, StripePayoutsEnabled = stripePayoutsEnabled
            };
            msgs.AddRange(obj.Save(dc: dc));

            var package = PackageFactory.CreateFreePackage(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(obj, package, msgs, dc);

            return obj;
        }

        public static Account CreateAccount(WebUser user, AccountType accountType, bool stripeChargesEnabled, bool stripePayoutsEnabled, string stripeUserId, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com", AccountTypeId = accountType, 
                StripeChargesEnabled = stripeChargesEnabled, StripePayoutsEnabled = stripePayoutsEnabled, StripeUserId = stripeUserId };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Account CreateWithFreePackage(WebUser user, AccountType accountType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = "Test Account", Email = "test@test.com", AccountTypeId = accountType };
            msgs.AddRange(obj.Save(dc: dc));

            var package = PackageFactory.CreateFreePackage(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(obj, package, msgs, dc);

            return obj;
        }

        public static Account CreateWithFreePackage(string name, string email, WebUser user, AccountType accountType,SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Account() { OwnerId = user.ID, Name = name, Email = email, AccountTypeId = accountType };
            msgs.AddRange(obj.Save(dc: dc));

            var package = PackageFactory.CreateFreePackageWithFlatFee(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(obj, package, msgs, dc);

            return obj;
        }

        public static AccountPackage AddPackageWithoutDisablingPreviousPackage(Account account, int packageId, BillingPeriodType billingPeriodType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Package package = null;
            AccountPackage currentPackage = null;
            var newap = new AccountPackage();
            var loggedInUserId = loggedInUser.IsNotEmpty() ? loggedInUser.ID.Value : 0;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    package = Package.Load(packageId, amsgs, adc);
                    currentPackage = AfterSchoolLibrary.Services.AccountService.GetActiveAccountPackage(account, amsgs, adc);

                    var trialDayCount = 0;

                    // is user:
                    //      paying for their Trial account
                    //      upgrading to a different package
                    //      paying for a new package with no trial account
                    var signupType = SignUpType.NotSet;
                    if (package.IsNotEmpty())
                        signupType = AfterSchoolLibrary.Services.AccountService.GetSignUpType(account, package, currentPackage, billingPeriodType, ref trialDayCount, amsgs, adc);

                    if (signupType != SignUpType.NotSet)
                    {
                        if (signupType == SignUpType.ChangePackage ||
                            signupType == SignUpType.ChangePackageDuringTrial)
                        {
                            //cancel existing package                            
                            //amsgs.AddRange(currentPackage.CancelPackageWithStripe(account.StripeCustomerId, false, adc));
                        }

                        if (package.StripePricePerMonthId.IsEmpty() || package.StripePricePerYearId.IsEmpty())
                            package.SaveWithStripe(adc);

                        var priceId = billingPeriodType == BillingPeriodType.Month ? package.StripePricePerMonthId : package.StripePricePerYearId;


                        if (msgs.HasNoErrors())
                        {
                            if (signupType != SignUpType.ConvertingFromTrial)
                            {
                                //create new package
                                newap.AccountId = account.ID;
                                newap.PackageId = packageId;
                                newap.BillingPeriodTypeId = billingPeriodType;
                                newap.CreateDate = DateTime.UtcNow;
                                newap.CreatedBy = loggedInUserId;
                                newap.LastModifiedDate = DateTime.UtcNow;
                                newap.LastModifiedBy = loggedInUserId;
                                newap.IsActive = true;
                                newap.Price = billingPeriodType == BillingPeriodType.Month ? package.Price : package.PricePerYear;

                                newap.StripeSubscriptionId = null;

                                if (currentPackage.IsNotEmpty() &&
                                    currentPackage.StripeSubscriptionId.IsNotEmpty() &&
                                    signupType != SignUpType.ChangePackage &&
                                    signupType != SignUpType.ChangePackageDuringTrial)
                                {
                                    var subscription = AfterSchoolLibrary.Billing.StripeHelper.GetSubscription(currentPackage?.StripeSubscriptionId, "", msgs);
                                    if (subscription.IsNotEmpty() && subscription.Status == "active")
                                        newap.StripeSubscriptionId = subscription.Id;
                                }

                                // set trial end date if there are still trial days
                                if (trialDayCount > 0)
                                {
                                    newap.TrialStartDate = DateTime.UtcNow;
                                    newap.TrialEndDate = DateTime.UtcNow.AddDays(trialDayCount);
                                }
                                else
                                    newap.StartDate = DateTime.UtcNow;

                                amsgs.AddRange(newap.SaveWithStripe(account.StripeCustomerId, false, priceId, adc));

                                //if (amsgs.HasNoErrors() && currentPackage.IsNotEmpty())
                                //    amsgs.AddRange(currentPackage.CancelPackage(adc));
                            }
                            // if it has a trial date end in the future, then end it now if they are converting from trial
                            else if (currentPackage.TrialEndDate.HasValue && currentPackage.TrialEndDate.Value >= DateTime.Now)
                            {
                                currentPackage.TrialEndDate = DateTime.UtcNow;
                                currentPackage.LastModifiedDate = DateTime.UtcNow;
                                currentPackage.LastModifiedBy = loggedInUserId;

                                amsgs.AddRange(currentPackage.SaveWithStripe(account.StripeCustomerId, true, priceId, adc));

                                newap = currentPackage;
                            }
                        }

                        if (amsgs.HasNoErrors() && newap.ID.HasValue)
                            package.CopyFeaturesToAccountPackage(newap, loggedInUserId, amsgs, adc);

                        // ensure package features are added
                        AfterSchoolLibrary.Services.AccountService.EnsureDefaultPackageFeatures(account, newap, loggedInUser, msgs, dc);

                        // make sure the account package is set to the real trial days passed in
                        if (amsgs.HasNoErrors())
                        {
                            var apf = newap.GetFeature(FeatureType.TrialDays, amsgs, adc);
                            if (apf.IsNotEmpty())
                            {
                                apf.Max = trialDayCount;
                                amsgs.AddRange(apf.Save(adc));
                            }
                        }
                    }
                    else
                        amsgs.Add($"Credit Card is required to sign up for {package.Name} Package", MessageType.UserError);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return newap;
        }

        public static (Stripe.Account, Stripe.Customer) CreateStripeAccountAndCustomer(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
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
                        Line1 = "12175",
                        Line2 = "Visionary Way",
                        Country = "US",
                        PostalCode = "46038",
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
                    },                    
                },                
                RequestedCapabilities = new List<string>() { "transfers", "card_payments", "legacy_payments" },
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

            var stripeAccount = account.CreateStripeCustomAccount(createOptions, updateChargesAndPayouts: false, msgs, dc);
            var customer = account.CreateStripeCustomer(msgs, dc);

            return (stripeAccount, customer);
        }
    }
}
