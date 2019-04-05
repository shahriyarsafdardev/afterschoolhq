using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Billing
{
    public class StripeHelper
    {
        private const string TestSecretKey = "sk_test_gfeMjl5rgl16L1HHITh3YT5s";
        private const string TestPublishableKey = "pk_test_wuL50O8i1mAzrmV2xDAXuClJ";
        private const string LiveSecretKey = "sk_live_SDa559erkpDCJ9BkrPS58GrC";
        private const string LivePublishableKey = "pk_live_4q5JuU2JIxsjWfQauJ7bNISK";

        private const string DevPlatformClientId = "ca_7kmNt22jLhXZWtyVV1884Kng6mgs79Jv";
        private const string LivePlatformClientId = "ca_7kmNqL4oMYfH5z7tY2N9riALjFpkMJAQ";

        public const decimal StripePercentFee = .029M;
        public const decimal StripeFlatFee = .3M;

        public static decimal GetStripeFee(decimal cost, bool feeTakenOutOfCost)
        {
            if (feeTakenOutOfCost)
                return (cost * StripePercentFee) + StripeFlatFee;

            return ((cost + StripeFlatFee) / (1 - StripePercentFee)) - cost;
        }

        public static string SecretKey
        {
            get
            {
                if(Helpers.Utilities.IsProduction)
                    return LiveSecretKey;

                return TestSecretKey;
            }
        }
        public static string PublishableKey
        {
            get
            {
                if (Helpers.Utilities.IsProduction)
                    return LivePublishableKey;

                return TestPublishableKey;
            }
        }
        public static string PlatformClientId
        {
            get
            {
                if (Helpers.Utilities.IsProduction)
                    return LivePlatformClientId;

                return DevPlatformClientId;
            }
        }

        private static string StripeAccessToken { get { return StripeHelper.SecretKey; } }

        #region StripeAccounts

        public static AccountService GetAccountService()
        {
            return new AccountService(StripeAccessToken);
        }

        public static Stripe.Account GetCustomAccount(string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();            

            Stripe.Account account = null;

            try
            {
                account = GetAccountService().Get(stripeAccountId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error getting payment account", MessageType.UserError);
            }

            return account;
        }

        public static Stripe.Account CreateCustomAccount(string businessName, string email, LegalEntityType entityType, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new AccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                DefaultCurrency = "usd",
                Company = new AccountCompanyOptions()
                {
                    Name = businessName,
                },
                Email = email,
                Settings = new AccountSettingsOptions()
                {
                    Payouts = new AccountSettingsPayoutsOptions()
                    {
                        DebitNegativeBalances = true,
                        StatementDescriptor = "AHQ transfer".MaxChars(22),
                        Schedule = new AccountSettingsPayoutsScheduleOptions()
                        {
                            Interval = "manual",
                        }
                    }
                },
                BusinessType = entityType.ToString().ToLower(),
                RequestedCapabilities = new List<string>() { "platform_payments" }
            };

            Stripe.Account account = null;

            try
            {
                account = GetAccountService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error creating payment account", MessageType.UserError);
            }

            return account;
        }

        public static ExternalAccountService GetExternalAccountService()
        {
            return new ExternalAccountService(StripeAccessToken);
        }

        public static Stripe.BankAccount CreateCustomBankAccount(string stripeAccountId, string routingNumber, string accountNumber, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new ExternalAccountCreateOptions()
            {                
                ExternalAccountBankAccount = new AccountBankAccountOptions()
                {
                    RoutingNumber = routingNumber,
                    AccountNumber = accountNumber,
                    Country = "US",
                    Currency = "usd"
                }
            };

            BankAccount bankAccount = null;

            try
            {
                bankAccount = (BankAccount)GetExternalAccountService().Create(stripeAccountId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error creating bank account", MessageType.UserError);
            }

            return bankAccount;
        }

        public static Stripe.Account UpdateCustomAccountLegalEntity(string stripeAccountId, LegalEntityType entityType, string firstName, string lastName, string ssnLast4, string fullSSN, DateTime? dateOfBirth, string address1, string address2, string city, string state, string postalCode, string supportPhone, string supportEmail, string businessName, string ein, string businessUrl, string businessPhone, string documentFileId, DateTime tosAcceptanceDate, string tosIPAddress, string tosUserAgent, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var address = new AddressOptions()
            {
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = "US",
                Line1 = address1,
                Line2 = address2,
            };

            var options = new AccountUpdateOptions()
            {
                BusinessProfile = new AccountBusinessProfileOptions()
                {
                    SupportEmail = supportEmail,
                    SupportPhone = supportPhone,
                    Url = businessUrl
                },
                BusinessType = entityType.ToString().ToLower(),
                TosAcceptance = new AccountTosAcceptanceOptions()
                {
                    Date = tosAcceptanceDate,
                    Ip = tosIPAddress
                },
            };

            if (entityType == LegalEntityType.Company)
            {
                options.BusinessProfile.Name = businessName;
                options.Company = new AccountCompanyOptions()
                {
                    Address = address,
                    TaxId = ein,
                    Phone = businessPhone,     
                };
            }
            else
                options.Individual = new PersonUpdateOptions()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Dob = new DobOptions()
                    {
                        Day = dateOfBirth.Value.Day,
                        Month = dateOfBirth.Value.Month,
                        Year = dateOfBirth.Value.Year,
                    },
                    SSNLast4 = ssnLast4,
                    Address = address
                };

            if (fullSSN.IsNotEmpty())
                options.Individual.PersonalIdNumber = fullSSN;

            if (documentFileId.IsNotEmpty())
                options.Individual.Verification.Document.FrontFileId = documentFileId;

            Stripe.Account account = null;

            try
            {
                account = GetAccountService().Update(stripeAccountId, options);               
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error updating payment account", MessageType.UserError);
            }

            return account;
        }

        #endregion

        #region StripeFiles

        private static FileService GetFileUploadService()
        {
            return new FileService(StripeAccessToken);
        }

        public static Stripe.File UploadFile(StripeUploadFile stripeUploadFile, SystemMessages msgs)
        {
            return UploadFile(stripeUploadFile.Document, stripeUploadFile.FileName, stripeUploadFile.Purpose, msgs);
        }

        public static Stripe.File UploadFile(Stream file, string filename, StripeFilePurpose purpose, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var fileCreateOptions = new FileCreateOptions()
            {
                File = file,
                Purpose = purpose.ToString().SplitByUpperCase().Replace(" ", "_").ToLower()               
            };

            Stripe.File stripeFile = null;
            
            try
            {
                stripeFile = GetFileUploadService().Create(fileCreateOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"payment account file was not uploaded", MessageType.UserError);
            }

            return stripeFile;
        }

        #endregion

        #region StripeProducts

        private static ProductService GetProductService()
        {
            return new ProductService(StripeAccessToken);
        }        

        public static Stripe.Product CreateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Product product = null;

            var options = new ProductCreateOptions
            {
                Id = productId,
                Name = productName,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                Active = active,
                Type = "service"
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            if (metaData.IsNotEmpty())
                options.Metadata = metaData;

            try
            {
                product = GetProductService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Product was not created", MessageType.UserError);
            }

            return product;
        }

        public static Stripe.Product UpdateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();


            var options = new ProductUpdateOptions
            {
                Name = productName,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                Active = active
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Product product = null;

            try
            {
                product = GetProductService().Update(productId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Product ({productId}) was not updated", MessageType.UserError);
            }

            return product;
        }

        public static SystemMessages DeleteProduct(string productId, string stripeAccountId)
        {
            var msgs = new SystemMessages();

            var requestOptions = new RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var result = GetProductService().Delete(productId, requestOptions);

                if (result.Deleted.HasValue && !result.Deleted.Value)
                    msgs.Add($"Product ({productId}) was not deleted", MessageType.UserError);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Product ({productId}) was not deleted", MessageType.UserError);
            }

            return msgs;
        }

        public static Stripe.Product GetProduct(string productId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var requestOptions = new RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Product product = null;

            try
            {
                product = GetProductService().Get(productId, requestOptions);
            }
            catch (StripeException ex)
            {
                // msgs.Add($"No stripe product found with ID: {productId}", MessageType.Notice, ex);
            }

            return product;
        }

        #endregion

        #region StripePlans

        private static PlanService GetPlanService()
        {
            return new PlanService(StripeAccessToken);
        }

        /// <summary>
        /// Create a stripe plan
        /// </summary>
        /// <param name="stripeProduct"></param>
        /// <param name="planId">optional, you can set your own id or just save it and get the plan id back</param>
        /// <param name="price"></param>
        /// <param name="billingPeriodType"></param>
        /// <param name="planNickName"></param>
        /// <param name="stripeAccountId">optional, the account id of the connect account, if platform should be null</param>
        /// <param name="metaData">A dictionary to send whatever information we want to store about the plan, for easier referencing between systems</param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public static Plan CreatePlan(Stripe.Product stripeProduct, string planId, decimal price, BillingPeriodType billingPeriodType, string planNickName, string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Plan plan = null;
            var options = new Stripe.PlanCreateOptions
            {
                Amount = Convert.ToInt32(price * 100),
                Currency = "usd",
                IntervalCount = 1,
                Interval = billingPeriodType.ToString().ToLower(),
                Nickname = planNickName,
                ProductId = stripeProduct.Id
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            if (metaData.IsNotEmpty())
                options.Metadata = metaData;

            // you can set your own id or just save it and get the plan id back
            if (planId.IsNotEmpty())
                options.Id = planId;

            try
            {
                plan = GetPlanService().Create(options, requestOptions);
            }
            catch(StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Plan was not created", MessageType.UserError);
            }

            return plan;
        }

        public static Stripe.Plan UpdatePlan(string planId, string planNickName, bool active, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.PlanUpdateOptions
            {
                Active = active,
                Nickname = planNickName
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Plan plan = null;

            try
            {
                plan = GetPlanService().Update(planId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Plan ({planId}) was not updated", MessageType.UserError);
            }

            return plan;
        }

        public static SystemMessages DeletePlan(string planId, string stripeAccountId)
        {
            var msgs = new SystemMessages();
            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var result = GetPlanService().Delete(planId, requestOptions);

                if (result.Deleted.HasValue && !result.Deleted.Value)
                    msgs.Add($"Plan ({planId}) was not deleted", MessageType.UserError);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Plan ({planId}) was not deleted", MessageType.UserError);
            }

            return msgs;
        }

        public static Stripe.Plan GetPlan(string planId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Plan plan = null;

            try
            {
                plan = GetPlanService().Get(planId, requestOptions);
            }
            catch (StripeException ex)
            {
                // msgs.Add($"No stripe plan found with ID: {planId}", MessageType.Notice, ex);
            }

            return plan;
        }

        #endregion

        #region StripeCustomers

        private static Stripe.CustomerService GetCustomerService()
        {
            return new Stripe.CustomerService(StripeAccessToken);
        }

        public static Stripe.Customer CreateCustomer(string description, string email, string stripeAccountId, SystemMessages msgs)
        {
            return CreateCustomer(description, email, null, null, msgs);
        }

        public static Stripe.Customer CreateCustomer(string description, string email, string sourceId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Customer customer = null;
            var options = new Stripe.CustomerCreateOptions
            {
                Description = description,
                Email = email
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                customer = GetCustomerService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer was not created", MessageType.UserError);
            }

            return customer;
        }

        public static Stripe.Customer CreateCustomerFromToken(string token, string description, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Customer customer = null;
            var options = new Stripe.CustomerCreateOptions
            {
                Description = description,
                SourceToken = token                
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                customer = GetCustomerService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer was not created", MessageType.UserError);
            }

            return customer;
        }

        public static SystemMessages UpdateCustomerDefaultCard(string customerId, string stripeAccountId, string cardId)
        {
            var msgs = new SystemMessages();
            var options = new Stripe.CustomerUpdateOptions
            {
                DefaultSource = cardId
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                GetCustomerService().Update(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }

            return msgs;
        }
        
        public static Stripe.Customer UpdateCustomer(string customerId, string description, string email, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            var options = new Stripe.CustomerUpdateOptions
            {
                Description = description,
                Email = email
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Customer customer = null;

            try
            {
                customer = GetCustomerService().Update(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }

            return customer;
        }

        public static SystemMessages DeleteCustomer(string customerId, string stripeAccountId)
        {
            var msgs = new SystemMessages();

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var result = GetCustomerService().Delete(customerId, requestOptions);

                if (result.Deleted.HasValue && !result.Deleted.Value)
                    msgs.Add($"Customer ({customerId}) was not deleted", MessageType.UserError);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not deleted", MessageType.UserError);
            }

            return msgs;
        }

        public static Stripe.Customer GetCustomer(string customerId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Customer customer = null;

            try
            {
                customer = GetCustomerService().Get(customerId, requestOptions);
            }
            catch (StripeException ex)
            {
                // msgs.Add($"No stripe customer found with ID: {customerId}", MessageType.Notice, ex);
            }

            return customer;
        }

        public static SystemMessages AddSourceToCustomer(string customerId, string sourceToken, string stripeAccountId)
        {
            var msgs = new SystemMessages();

            var options = new Stripe.CustomerUpdateOptions
            {
                SourceToken = sourceToken
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                GetCustomerService().Update(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }

            return msgs;
        }

        #endregion

        #region StripeSubscriptions

        private static Stripe.SubscriptionService GetSubscriptionService()
        {
            return new Stripe.SubscriptionService(StripeAccessToken);
        }

        public static Stripe.Subscription CreateSubscription(string customerId, string planId, string sourceId, decimal? platformFeePercent, DateTime? trialEndDate, string stripeAccountId, string stripeCouponId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Subscription subscription = null;

            var options = new Stripe.SubscriptionCreateOptions
            {
                CustomerId = customerId,
                Billing = Stripe.Billing.ChargeAutomatically,
                TrialEnd = trialEndDate,
                DefaultSource = sourceId,
                CouponId = stripeCouponId      ,
                Items = new List<SubscriptionItemOption>() { new SubscriptionItemOption() { PlanId = planId, Quantity = 1 } }
                
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
            {
                requestOptions.StripeConnectAccountId = stripeAccountId;
                options.ApplicationFeePercent = platformFeePercent * 100;
            }

            try
            {
                subscription = GetSubscriptionService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription was not created", MessageType.UserError);
            }

            return subscription;
        }

        /// <summary>
        /// will create a recurring subscription with no end date, and automatic charging
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="plan"></param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public static Stripe.Subscription CreateSubscription(Stripe.Customer customer, Stripe.Plan plan, string sourceId, decimal? platformFeePercent, DateTime? trialEndDate, string stripeAccountId, string stripeCouponId, SystemMessages msgs)
        {
            return CreateSubscription(customer.Id, plan.Id, sourceId, platformFeePercent, trialEndDate, stripeAccountId, stripeCouponId, msgs);
        }

        public static Stripe.Subscription UpdateSubscription(string subscriptionId, string newPlanId, bool? billingCycleAnchorNow, bool? billingCycleAnchorUnchanged, bool? endTrialNow, bool? prorate, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.SubscriptionUpdateOptions();

            if (newPlanId.IsNotBlank())
            {
                options.Items = new List<Stripe.SubscriptionItemUpdateOption>()
                    {
                        new Stripe.SubscriptionItemUpdateOption()
                        {
                            PlanId = newPlanId,
                            Quantity = 1
                        }
                    };
            }

            if (billingCycleAnchorNow.HasValue)
                options.BillingCycleAnchorNow = billingCycleAnchorNow.Value;

            if (billingCycleAnchorUnchanged.HasValue)
                options.BillingCycleAnchorUnchanged = billingCycleAnchorUnchanged.Value;

            if (endTrialNow.HasValue)
                options.EndTrialNow = endTrialNow.Value;

            if (prorate.HasValue)
                options.Prorate = prorate.Value;

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Subscription subscription = null;

            try
            {
                subscription = GetSubscriptionService().Update(subscription.Id, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription ({subscriptionId}) was not updated", MessageType.UserError);
            }

            return subscription;
        }

        public static SystemMessages CancelSubscription(string subscriptionId, string stripeAccountId)
        {
            var msgs = new SystemMessages();
            
            var options = new Stripe.SubscriptionUpdateOptions()
            {
                CancelAtPeriodEnd = true
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var subscription = GetSubscriptionService().Update(subscriptionId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription ({subscriptionId}) was not cancelled", MessageType.UserError);
            }

            return msgs;
        }

        public static Stripe.Subscription GetSubscription(string subscriptionId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Subscription subscription = null;

            try
            {
                subscription = GetSubscriptionService().Get(subscriptionId, requestOptions);
            }
            catch (StripeException ex)
            {
                // msgs.Add($"No stripe subscription found with ID: {subscriptionId}", MessageType.Notice, ex);
            }

            return subscription;
        }

        #endregion

        #region StripeCard

        private static Stripe.CardService GetCardService()
        {
            return new Stripe.CardService(StripeAccessToken);
        }

        public static Stripe.Card CreateCard(Stripe.Customer customer, string token, SystemMessages msgs)
        {
            return CreateCard(customer.Id, token, msgs);
        }

        public static Stripe.Card CreateCard(string customerId, string token, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Card card = null;

            try
            {
                var options = new Stripe.CardCreateOptions()
                {
                    SourceToken = token
                };

                card = GetCardService().Create(customerId, options);
            }
            catch(StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Stripe source card was not created", MessageType.Error);
            }

            return card;
        }

        public static List<Stripe.Card> GetCards(string customerId, SystemMessages msgs)
        {
            var cards = new List<Stripe.Card>();

            try
            {
                cards.AddRange(GetCardService().List(customerId).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add($"Cannot get list of cards", MessageType.Error, ex);
            }

            return cards;
        }

        public static Stripe.Card GetCard(string customerId, string cardId, SystemMessages msgs)
        {
            var card = new Stripe.Card();

            try
            {
                card = GetCardService().Get(customerId, cardId);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Card id ({cardId}) not found", MessageType.Error, ex);
            }

            return card;
        }

        public static SystemMessages DeleteCard(string customerId, string cardId)
        {
            var msgs = new SystemMessages();

            try
            {
                var result = GetCardService().Delete(customerId, cardId);

                if (result.Deleted.HasValue && !result.Deleted.Value)
                    msgs.Add($"Stripe card ({cardId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe card ({cardId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        //public static SourceCard GetTestCard(StripeTestCardType stripeTestCardType)
        //{
        //    return new SourceCard()
        //    {
        //        Name = "Bill Payer",
                
        //        Number = GetTestCardNumber(stripeTestCardType),
        //        ExpirationMonth = 1,
        //        ExpirationYear = DateTime.UtcNow.AddYears(2).Year,
        //        Cvc = "123"
        //    };
        //}

        private static string GetTestCardNumber(StripeTestCardType stripeTestCardType)
        {
            switch(stripeTestCardType)
            {
                case StripeTestCardType.AmericanExpress:
                    return "378282246310005";
                case StripeTestCardType.AmericanExpress2:
                    return "371449635398431";
                case StripeTestCardType.DinersClub:
                    return "30569309025904";
                case StripeTestCardType.DinersClub2:
                    return "38520000023237";
                case StripeTestCardType.Discover:
                    return "6011111111111117";
                case StripeTestCardType.Discover2:
                    return "6011000990139424";
                case StripeTestCardType.JCB:
                    return "3566002020360505";
                case StripeTestCardType.MasterCard:
                    return "5555555555554444";
                case StripeTestCardType.MasterCard2Series:
                    return "2223003122003222";
                case StripeTestCardType.MasterCardDebit:
                    return "5200828282828210";
                case StripeTestCardType.MasterCardPrepaid:
                    return "5105105105105100";
                case StripeTestCardType.UnionPay:
                    return "6200000000000005";
                case StripeTestCardType.VisaDebit:
                    return "4000056655665556";
                case StripeTestCardType.Visa:
                default:
                    return "4242424242424242";

            }
        }

        #endregion

        #region StripeSource

        public const string TestSourceToken = "tok_visa";

        private static Stripe.SourceService GetSourceService()
        {
            return new Stripe.SourceService(StripeAccessToken);
        }

        public static Stripe.Source CreateSource(Stripe.Customer customer, string token, SystemMessages msgs)
        {
            return CreateSource(token, msgs);
        }

        public static Stripe.Source CreateSource(string token, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Source source = null;

            try
            {
                var options = new Stripe.SourceCreateOptions()
                {
                    Type = "card",
                    Token = token
                };

                source = GetSourceService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Payment source was not created", MessageType.UserError);
            }

            return source;
        }

        public static Stripe.Source CreateSourceFromOriginal(string customerId, string originalSourceId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.SourceCreateOptions()
            {
                Customer = customerId,
                OriginalSource = originalSourceId,
                Usage = "reusable"
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Source source = null;

            try
            {
                source = GetSourceService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Payment source was not created", MessageType.UserError);
            }

            return source;
        }

        public static List<Stripe.Source> GetSources(string customerId, SystemMessages msgs)
        {
            var cards = new List<Stripe.Source>();

            try
            {
                cards.AddRange(GetSourceService().List(customerId).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Cannot get list of payment sources", MessageType.UserError);
            }

            return cards;
        }

        public static Stripe.Source GetSource(string sourceId, SystemMessages msgs)
        {
            var source = new Stripe.Source();

            try
            {
                source = GetSourceService().Get(sourceId);
            }
            catch (StripeException ex)
            {

            }

            return source;
        }

        #endregion

        #region StripeCharge

        private static Stripe.ChargeService GetChargeService()
        {
            return new Stripe.ChargeService(StripeAccessToken);
        }        

        public static Stripe.Charge CreateCharge(string fromCustomerId, string tokenOrSourceId, decimal totalAmount, string description, string statementDescriptor, SystemMessages msgs)
        {
            return CreateCharge(fromCustomerId, null, tokenOrSourceId, totalAmount, null, description, statementDescriptor, msgs);
        }

        public static Stripe.Charge CreateCharge(string fromCustomerId, string destinationAccountId, string tokenOrSourceId, decimal totalAmount, decimal? platformFee, string description, string statementDescriptor, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.ChargeCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Currency = "usd",
                Description = description,
                SourceId = tokenOrSourceId,                
                // SourceTokenOrExistingSourceId = tokenOrSourceId,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                CustomerId = fromCustomerId
            };

            if (platformFee.HasValue)
            {
                options.Destination = new ChargeDestinationCreateOptions()
                {
                    Account = destinationAccountId,
                    Amount = (totalAmount * 100).ToInteger() - (platformFee.Value * 100).ToInteger()
                };
            }

            Stripe.Charge charge = null;

            try
            {
                charge = GetChargeService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Error charging payment source", MessageType.UserError);
            }

            return charge;
        }

        public static List<Stripe.Charge> GetCharges(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<Stripe.Charge>();
            var options = new Stripe.ChargeListOptions
            {
                CustomerId = customerId,
                Limit = 10
            };

            try
            {
                charges.AddRange(GetChargeService().List(options).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Error retrieving charges", MessageType.UserError);
            }

            return charges;
        }

        #endregion

        #region StripeTokens

        private static Stripe.TokenService GetTokenService()
        {
            return new Stripe.TokenService(StripeAccessToken);
        }

        public static Stripe.Token CreateCustomerToken(string customerId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.TokenCreateOptions()
            {
                CustomerId = customerId               
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Token token = null;

            try
            {
                token = GetTokenService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add("Error charging payment source", MessageType.Error, ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
            }

            return token;
        }

        #endregion

        #region StripeRefunds

        public static Stripe.RefundService GetRefundService()
        {
            return new Stripe.RefundService(StripeAccessToken);
        }

        public static Stripe.Refund CreateRefund(string chargeId, decimal totalAmount, string reason, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.RefundCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Reason = reason,
                ReverseTransfer = true,
                ChargeId = chargeId
            };

            Stripe.Refund refund = null;

            try
            {
                refund = GetRefundService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Error refunding", MessageType.UserError);
            }

            return refund;
        }

        public static List<Stripe.Refund> GetRefunds(string chargeId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<Stripe.Refund>();
            var options = new Stripe.RefundListOptions
            {
                 ChargeId = chargeId
            };

            try
            {
                refunds.AddRange(GetRefundService().List(options).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add("Error retrieving refunds", MessageType.Error, ex);
            }

            return refunds;
        }

        #endregion

        #region StripeCoupons

        private static Stripe.CouponService GetCouponService()
        {
            return new Stripe.CouponService(StripeAccessToken);
        }

        public static Stripe.Coupon CreateCoupon(decimal discountValue, string stripeAccountId, CouponAmountType amountType, StripeCouponDuration couponDuration, int? durationInMonths, SystemMessages msgs)
        {
            return CreateCoupon(discountValue, couponDuration, durationInMonths, amountType, stripeAccountId, msgs);
        }

        public static Stripe.Coupon CreateCoupon(decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths, CouponAmountType couponAmountType, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.CouponCreateOptions();

            if (couponDuration == StripeCouponDuration.Repeating)
            {
                options.DurationInMonths = durationInMonths;
                options.Duration = StripeCouponDuration.Repeating.ToString().ToLower();
            }
            else
                options.Duration = StripeCouponDuration.Forever.ToString().ToLower();

            var value = (discountValue * 100).ToInteger();

            if (couponAmountType == CouponAmountType.FlatAmount)
            {
                options.AmountOff = value;
                options.Currency = "USD";
            }
            else
                options.PercentOff = value;

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            Stripe.Coupon coupon = null;

            try
            {
                coupon = GetCouponService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Coupon was not created", MessageType.UserError);
            }

            return coupon;
        }

        #endregion
    }

    public enum StripeTestCardType
    {
        Visa,
        VisaDebit,
        MasterCard,
        MasterCard2Series,
        MasterCardDebit,
        MasterCardPrepaid,
        AmericanExpress,
        AmericanExpress2,
        Discover,
        Discover2,
        DinersClub,
        DinersClub2,
        JCB,
        UnionPay
    }

    public enum StripeFilePurpose
    {
        BusinesLogo,
        CustomerSignature,
        DisputeEvidence,
        IdentityDocument,
        PciDocument,
        TaxDocumentUserUpload
    }

    enum StripeAccountType
    {
        Custom,
        Standard,
    }

    enum StripeAccountHolderType
    {
        Individual,
        Company
    }

    // Specifies how long the discount will be in effect
    public enum StripeCouponDuration
    {
        Forever,
        Once,
        Repeating
    }

    enum StripeRefundReason
    {
        Duplicate,
        Fraudulent,
        RequestedByCustomer
    }

    public enum StripeRefundStatus
    {
        Succeeded,
        Failed,
        Pending,
        Canceled
    }
    
}
