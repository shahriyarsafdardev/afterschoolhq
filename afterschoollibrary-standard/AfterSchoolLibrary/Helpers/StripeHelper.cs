using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;

namespace AfterSchoolLibrary.Billing
{
    public class StripeHelper
    {
        private static string TestSecretKey { get { return "sk_test_gfeMjl5rgl16L1HHITh3YT5s"; } }
        private static string TestPublishableKey { get { return "pk_test_wuL50O8i1mAzrmV2xDAXuClJ"; } }

        private static string StagingSecretKey { get { return "sk_test_51JIMdPKRCGzrZhdtfWdqperobTbUALbC7jM1hJ6NnKm3C3fR1cFMQxERjehcxzOThlKYw0V3juhXLyAKzcViAC2H00EXqsFG7H"; } }
        private static string StagingPublishableKey { get { return "pk_test_51JIMdPKRCGzrZhdtgviypkNlwXqh1u4nVGshwYPVZCGp1Dk95MedDZaN7jy2cwBzGVFaLe2YafD6vLJTr1L5a8Gk00fkzGdARr"; } }

        private static string LiveSecretKey { get { return ConfigurationManager.AppSettings["Stripe.LiveSecretKey"]; } }
        private static string LivePublishableKey { get { return ConfigurationManager.AppSettings["Stripe.LivePublishableKey"]; } }

        private static string DevPlatformClientId { get { return "ca_7kmNt22jLhXZWtyVV1884Kng6mgs79Jv"; } }
        private static string StagingPlatformClientId { get { return "ca_JwMhWaF2B2i2OntwfomRxhcmhjrJduce"; } }
        private static string LivePlatformClientId { get { return ConfigurationManager.AppSettings["Stripe.LivePlatformClientId"]; } }

        public const decimal StripePercentFee = .029M;
        public const decimal StripeFlatFee = .3M;
        public const decimal StripeBankPercentFee = .008M;

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
                if (Helpers.Utilities.IsProduction)
                    return LiveSecretKey;
                else if (Helpers.Utilities.IsStaging)
                    return StagingSecretKey;
                else
                    return TestSecretKey;
            }
        }

        public static string PublishableKey
        {
            get
            {
                if (Helpers.Utilities.IsProduction)
                    return LivePublishableKey;
                else if (Helpers.Utilities.IsStaging)
                    return StagingPublishableKey;
                else
                    return TestPublishableKey;
            }
        }

        public static string PlatformClientId
        {
            get
            {
                if (Helpers.Utilities.IsProduction)
                    return LivePlatformClientId;
                else if (Helpers.Utilities.IsStaging)
                    return StagingPlatformClientId;
                else
                    return DevPlatformClientId;
            }
        }

        private static string StripeAccessToken { get { return StripeHelper.SecretKey; } }
        private static StripeClient StripeClient { get { return new StripeClient(apiKey: StripeAccessToken); } }

        #region StripeAccounts

        public static AccountService GetAccountService()
        {
            return new AccountService(StripeClient);
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

        public static Stripe.Account CreateCustomAccount(AccountCreateOptions options, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
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

        public static Stripe.Account UpdateCustomAccount(string stripeAccountId, AccountUpdateOptions options, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
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

        //public static Stripe.Account DisableAutomaticPayoutForAccount(string stripeAccountId, AccountUpdateOptions options, SystemMessages msgs)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();
        //    var account = GetCustomAccount(stripeAccountId, msgs);

        //    try
        //    {
        //        account = GetAccountService().Update(stripeAccountId, options);
        //    }
        //    catch (StripeException ex)
        //    {
        //        msgs.Add(ex.Message, MessageType.UserError, ex);
        //        msgs.Add("Error updating payment account", MessageType.UserError);
        //    }

        //    return account;
        //}

        [Obsolete]
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
                RequestedCapabilities = new List<string>() { "card_payments" }
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
            return new ExternalAccountService(StripeClient);
        }

        public static Stripe.BankAccount CreateCustomBankAccount(string stripeAccountId, string routingNumber, string accountNumber, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new ExternalAccountCreateOptions()
            {                                
                ExternalAccount = new AccountBankAccountOptions()
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

        public static Stripe.Account UpdateCustomAccountLegalEntity(string stripeAccountId, LegalEntityType entityType, string firstName, string lastName, string ssnLast4, string fullSSN, DateTime? dateOfBirth, string address1, string address2, string city, string state, string postalCode, string supportPhone, string supportEmail, string businessName, string ein, string businessUrl, string businessPhone, string productDescription, string documentFileId, DateTime tosAcceptanceDate, string tosIPAddress, string tosUserAgent, SystemMessages msgs)
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
                    Url = businessUrl,
                    ProductDescription = productDescription
                },
                BusinessType = entityType.ToString().ToLower(),
                TosAcceptance = new AccountTosAcceptanceOptions()
                {
                    Date = tosAcceptanceDate,
                    Ip = tosIPAddress
                }                
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

            // if (fullSSN.IsNotEmpty())
            //    options.Individual.PersonalIdNumber = fullSSN;

            // if (documentFileId.IsNotEmpty())
            //    options.Individual.Verification.Document.FrontFileId = documentFileId;

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

        #region StripePerson

        private static PersonService GetPersonService()
        {
            return new PersonService(StripeClient);
        }

        public static Stripe.Person CreatePerson(string stripeAccountId, PersonCreateOptions options, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            Stripe.Person person = null;

            try
            {
                person = GetPersonService().Create(stripeAccountId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error creating payment account", MessageType.UserError);
            }

            return person;
        }

        public static Stripe.Person GetPerson(string stripeAccountId, string stripePersonId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            Stripe.Person person = null;

            try
            {
                person = GetPersonService().Get(stripeAccountId, stripePersonId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error retrieving person from stripe", MessageType.Error);
            }

            return person;
        }

        #endregion

        #region StripeTransfer

        private static TransferService GetTransferService()
        {
            return new TransferService(StripeClient);
        }

        private static TransferReversalService GetTranferReversalService()
        {
            return new TransferReversalService(StripeClient);
        }

        public static Transfer CreateTransfer(decimal amount, string description, string transferIdentifier, string destinationId, string chargeId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new TransferCreateOptions()
            {
                Amount = (long)(amount * 100),
                Currency = "usd",
                Destination = destinationId,
                Metadata = metaData,
                Description = description,
                TransferGroup = transferIdentifier
            };

            if (chargeId.IsNotEmpty())
                options.SourceTransaction = chargeId;

            Transfer transfer = null;

            try
            {
                transfer = GetTransferService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Transfer was not created", MessageType.UserError);
            }

            return transfer;
        }


        internal static TransferReversal CreateTransferReversal(string stripeTransferId, decimal amountToReverse, SystemMessages msgs)
        {

            var options = new TransferReversalCreateOptions
            {
                Amount = (long)(amountToReverse * 100),
            };

            TransferReversal transferReversal = null;

            try
            {
                transferReversal = GetTranferReversalService().Create(stripeTransferId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Transfer Reversal was not created", MessageType.UserError);
            }

            return transferReversal;
        }

        #endregion

        #region StripePayouts

        private static PayoutService GetPayoutService()
        {
            return new PayoutService(StripeClient);
        }

        public static Payout CreatePayout(decimal amount, string externalAccountId, string connectedAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new PayoutCreateOptions()
            {
                Amount = (long)(amount * 100),
                Currency = "usd",
                Destination = externalAccountId,
                Metadata = metaData,
                Method = "standard",
                StatementDescriptor = "AHQ Payout"
            };

            var requestOptions = new RequestOptions()
            {
                StripeAccount = connectedAccountId
            };

            Payout stripePayout = null;

            try
            {
                stripePayout = GetPayoutService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Payout was not created", MessageType.UserError);
            }

            return stripePayout;
        }

        //public static Payout UpdatePayout(Payout payout, Dictionary<string, string> metaData, SystemMessages msgs)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();

        //    var options = new PayoutUpdateOptions()
        //    {                
        //        Expand = 
        //    };

        //    var requestOptions = new RequestOptions()
        //    {
        //        StripeAccount = connectedAccountId
        //    };

        //    Payout stripePayout = null;

        //    try
        //    {
        //        stripePayout = GetPayoutService().Update(payoutId: payout.Id, options: options, requestOptions: requestOptions);
        //    }
        //    catch (StripeException ex)
        //    {
        //        msgs.Add(ex.Message, MessageType.UserError, ex);
        //        msgs.Add($"Payout was not created", MessageType.UserError);
        //    }

        //    return stripePayout;
        //}

        #endregion

        #region StripeFiles

        private static FileService GetFileUploadService()
        {
            return new FileService(StripeClient);
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
            return new ProductService(StripeClient);
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
                requestOptions.StripeAccount = stripeAccountId;

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
                requestOptions.StripeAccount = stripeAccountId;

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

        public static SystemMessages DeleteProduct(string productId)
        {
            var msgs = new SystemMessages();

            try
            {
                var result = GetProductService().Delete(productId);

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
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Product product = null;

            try
            {
                product = GetProductService().Get(productId, null, requestOptions);
            }
            catch (StripeException ex)
            {
                // msgs.Add($"No stripe product found with ID: {productId}", MessageType.Notice, ex);
            }

            return product;
        }

        #endregion

        #region StripePrices
        private static PriceService GetPriceService()
        {
            return new PriceService(StripeClient);
        }

        public static Stripe.Price CreatePrice(string productId, decimal amount, string nickname, BillingPeriodType billingPeriodType, string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Price price = null;

            var options = new PriceCreateOptions
            {
                Product = productId,
                UnitAmount = (long)amount,
                Currency = "usd",
                Nickname = nickname,
                Recurring = new PriceRecurringOptions
                {
                    Interval = billingPeriodType.ToString().ToLower(),
                },
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            if (metaData.IsNotEmpty())
                options.Metadata = metaData;

            try
            {
                price = GetPriceService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Price was not created", MessageType.ApiError);
            }

            return price;
        }

        public static Stripe.Price GetPrice(string priceId, SystemMessages msgs)
        {
            Stripe.Price price = null;

            try
            {
                price = GetPriceService().Get(priceId);
            } 
            catch (StripeException ex)
            {
                //msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Unable to retrieve price", MessageType.Notice);
            }

            return price;
        }

        public static StripeList<Price> GetProductPrices(string productId, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var priceListOptions = new PriceListOptions()
            {
                Product = productId
            };

            var requestOptions = new RequestOptions();

            StripeList<Price> prices = null;

            try
            {
                prices = GetPriceService().List(priceListOptions, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe product price found with product ID: {productId}", MessageType.Notice, ex);
            }

            return prices;
        }   

        public static SystemMessages DeletePrice(string priceId, SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var options = new PriceUpdateOptions
            {
                   Active = false,
            };

            try
            {
                GetPriceService().Update(priceId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Unable to delete price", MessageType.ApiError);
            }

            return msgs;
        }

        #endregion

        #region StripePlans

        private static PlanService GetPlanService()
        {
            return new PlanService(StripeClient);
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

            var intervalCount = 1;
            var interval = billingPeriodType.ToString().ToLower();

            if (billingPeriodType == BillingPeriodType.TwoWeeks)
            {
                intervalCount = 2;
                interval = BillingPeriodType.Week.ToString().ToLower();
            }

            Stripe.Plan plan = null;
            var options = new Stripe.PlanCreateOptions
            {
                Amount = Convert.ToInt32(price * 100),
                Currency = "usd",
                IntervalCount = intervalCount,
                Interval = interval,
                Nickname = planNickName,
                Product = stripeProduct.Id
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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
                requestOptions.StripeAccount = stripeAccountId;

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

            try
            {
                var result = GetPlanService().Delete(planId);

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
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Plan plan = null;

            try
            {
                plan = GetPlanService().Get(planId, null, requestOptions);
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
            return new Stripe.CustomerService(StripeClient);
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
                requestOptions.StripeAccount = stripeAccountId;

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
                Source = token
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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
                requestOptions.StripeAccount = stripeAccountId;

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
                requestOptions.StripeAccount = stripeAccountId;

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

            try
            {
                var result = GetCustomerService().Delete(customerId);

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
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Customer customer = null;

            try
            {
                customer = GetCustomerService().Get(customerId, null, requestOptions);
            }
            catch (StripeException ex)
            {
                // msgs.Add($"No stripe customer found with ID: {customerId}", MessageType.Notice, ex);
            }

            return customer;
        }

        public static Stripe.Customer AddDefaultSourceToCustomer(string customerId, string sourceToken, string stripeAccountId, SystemMessages msgs)
        {
            var options = new Stripe.CustomerUpdateOptions
            {
                Source = sourceToken
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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

        public static Stripe.Source AttachSourceToCustomer(string customerId, string sourceToken, string stripeAccountId, SystemMessages msgs)
        {
            var options = new Stripe.SourceAttachOptions
            {
                Source = sourceToken
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Source source = null;

            try
            {
                source = GetSourceService().Attach(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Source was not attached", MessageType.UserError);
            }

            return source;
        }

        public static Stripe.Card AddNewCardToCustomer(string customerId, string token, string stripeAccountId, SystemMessages msgs)
        {
            var options = new Stripe.CardCreateOptions()
            {
                Source = token
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Card card = null;

            try
            {
                card = GetCardService().Create(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }

            return card;
        }

        public static Stripe.Card AddNewCardToCustomer(string customerId, Stripe.Source source, string stripeAccountId, SystemMessages msgs)
        {
            var options = new Stripe.CardCreateOptions()
            {
                 Source = source.Id
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Card card = null;

            try
            {
                card = GetCardService().Create(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }

            return card;
        }

        public static Stripe.BankAccount AddNewBankAccountToCustomer(string customerId, Stripe.Source source, string stripeAccountId, SystemMessages msgs)
        {
            var options = new Stripe.BankAccountCreateOptions()
            {
                Source = source.Id
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.BankAccount bankAccount = null;

            try
            {
                bankAccount = GetBankAccountService().Create(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }

            return bankAccount;
        }

        #endregion

        #region StripeCreditNotes

        private static Stripe.CreditNoteService GetCreditNoteService()
        {
            return new Stripe.CreditNoteService(StripeClient);
        }

        public static Stripe.CreditNote CreateCreditNote(string invoiceId, decimal amount, string memo, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            CreditNote creditNote = null;

            var options = new CreditNoteCreateOptions
            {
                Invoice = invoiceId,
                CreditAmount = Convert.ToInt64(amount * 100),
                Memo = memo,
                Lines = new List<CreditNoteLineOptions>()
                {
                    new CreditNoteLineOptions()
                    {
                        Type = "custom_line_item",
                        UnitAmount = Convert.ToInt64(amount * 100),
                        Quantity = 1,
                        Description = memo,
                    }
                }
            };

            try
            {
                creditNote = GetCreditNoteService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription was not created", MessageType.UserError);
            }

            return creditNote;
        }

        #endregion

        #region CustomerBalanceTransaction

        private static Stripe.CustomerBalanceTransactionService GetCustomerBalanceTransactionService()
        {
            return new Stripe.CustomerBalanceTransactionService(StripeClient);
        }

        public static Stripe.CustomerBalanceTransaction CreateCustomerBalanceTransaction(string customerId, decimal amount, string description, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            CustomerBalanceTransaction transaction = null;

            var options = new CustomerBalanceTransactionCreateOptions
            {
                Amount = Convert.ToInt64(amount * 100),
                Currency = "usd",
                Description = description,
                Metadata = metaData
            };

            try
            {
                transaction = GetCustomerBalanceTransactionService().Create(customerId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Balance transaction was not created", MessageType.UserError);
            }

            return transaction;
        }

        #endregion

        #region StripeSubscriptions

        private static Stripe.SubscriptionService GetSubscriptionService()
        {
            return new Stripe.SubscriptionService(StripeClient);
        }

        public static Stripe.Subscription CreateRegistrantSubscription(string customerId, string planId, string sourceId, decimal? platformFeePercent, 
            DateTime? startDate, DateTime? endDate, DateTime? billingCycleAnchor, string stripeAccountId, string stripeCouponId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Subscription subscription = null;

            var options = new Stripe.SubscriptionCreateOptions
            {
                Customer = customerId,
                CollectionMethod = "charge_automatically",
                DefaultSource = sourceId,
                Coupon = stripeCouponId,
                Items = new List<SubscriptionItemOptions>() { new SubscriptionItemOptions() { Plan = planId, Quantity = 1 } },
                Metadata = metaData,
                Prorate = billingCycleAnchor.HasValue, //prorate only if ongoing activity, else we will handle this manually
            };

            if (startDate.Value.Date.AddDays(-1).IsAfter(DateTime.UtcNow.Date))
                options.TrialEnd = startDate.Value.AddDays(-1);
            else if (billingCycleAnchor.HasValue)
            {
                //.. If registrant joins mid-period then we can set the billing cycle anchor as the last date of current cycle. for example if monthly and month is of 30th. set 30th.
                //.. Same way for weekly, only change would be weekly cycle would end on sunday
                //.. it'll get a prorated invoice for the shorter amount of time on the first month.
                //.. Then the customer will be invoiced normally after that https://stripe.com/docs/billing/subscriptions/billing-cycle#new-subscriptions            
                options.BillingCycleAnchor = billingCycleAnchor.Value;
            }

            // please make sure that billing ends when the program ends.
            if (endDate.HasValue && endDate.IsNotEmpty() && endDate.Value.Date.AddDays(-1).IsAfter(DateTime.UtcNow.Date))
                options.CancelAt = endDate.Value;        

            if (stripeAccountId.IsNotEmpty())
                options.TransferData = new SubscriptionTransferDataOptions()
                {
                    Destination = stripeAccountId,
                    AmountPercent = 100 - (platformFeePercent * 100)
                };

            try
            {
                subscription = GetSubscriptionService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription was not created", MessageType.UserError);
            }

            return subscription;
        }

        public static Stripe.Subscription CreatePackageSubscription(string customerId, string priceId, string sourceId, decimal? platformFeePercent, DateTime? startDate, string stripeAccountId, string stripeCouponId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Subscription subscription = null;

            var options = new Stripe.SubscriptionCreateOptions
            {
                Customer = customerId,
                CollectionMethod = "charge_automatically",
                Metadata = metaData,
                Prorate = false,
            };

            if (sourceId.IsNotEmpty())
                options.DefaultSource = sourceId;

            if (priceId.IsNotBlank())
            {
                options.Items = new List<Stripe.SubscriptionItemOptions>()
                    {
                        new Stripe.SubscriptionItemOptions()
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    };
            }

            if (stripeCouponId.IsNotEmpty())
                options.Coupon = stripeCouponId;

            if (startDate.Value.AddDays(-1).IsAfter(DateTime.UtcNow))
                options.TrialEnd = startDate.Value.AddDays(-1);

            if (stripeAccountId.IsNotEmpty())
                options.TransferData = new SubscriptionTransferDataOptions()
                {
                    Destination = stripeAccountId,
                    AmountPercent = 100 - (platformFeePercent * 100)
                };

            try
            {
                subscription = GetSubscriptionService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription was not created", MessageType.UserError);
            }

            return subscription;
        }

        public static Stripe.Subscription UpdatePackageSubscription(string subscriptionId, string priceId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Subscription subscription = GetSubscriptionService().Get(subscriptionId);

            var options = new Stripe.SubscriptionUpdateOptions() 
            { 
                ProrationBehavior = "always_invoice",
                BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now
            };

            if (priceId.IsNotBlank())
            {
                options.Items = new List<Stripe.SubscriptionItemOptions>()
                    {
                        new Stripe.SubscriptionItemOptions()
                        {
                            Id = subscription.Items.Data[0].Id,
                            Price = priceId,
                        }
                    };
            } 

            try
            {
                subscription = GetSubscriptionService().Update(subscriptionId, options);
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
        public static Stripe.Subscription CreateSubscription(Stripe.Customer customer, Stripe.Plan plan, string sourceId, decimal? platformFeePercent, DateTime? startDate, string stripeAccountId, string stripeCouponId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            return CreateRegistrantSubscription(customer.Id, plan.Id, sourceId, platformFeePercent, startDate, null, null, stripeAccountId, stripeCouponId, metaData, msgs);
        }

        public static Stripe.Subscription UpdateSubscription(string subscriptionId, string priceId, bool? billingCycleAnchorNow, bool? billingCycleAnchorUnchanged, bool? endTrialNow, bool? prorate, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.SubscriptionUpdateOptions();

            if (priceId.IsNotBlank())
            {
                options.Items = new List<Stripe.SubscriptionItemOptions>()
                    {
                        new Stripe.SubscriptionItemOptions()
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    };
            }

            if (billingCycleAnchorNow.HasValue)
                options.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now;

            if (billingCycleAnchorUnchanged.HasValue)
                options.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Unchanged;

            if (endTrialNow.HasValue)
                options.TrialEnd = DateTime.UtcNow;

            if (prorate.HasValue)
                options.Prorate = prorate.Value;

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Subscription subscription = null;

            try
            {
                var service = GetSubscriptionService();
                if (service.IsNotEmpty())
                    subscription = service.Update(subscriptionId, options, requestOptions);
                
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription ({subscriptionId}) was not updated", MessageType.UserError);
            }

            return subscription;
        }

        public static SystemMessages CancelSubscription(string subscriptionId, string stripeAccountId, bool cancelAtPeriodEnd = true)
        {
            var msgs = new SystemMessages();

            Stripe.SubscriptionUpdateOptions options;

            if(cancelAtPeriodEnd)
            {
                options = new Stripe.SubscriptionUpdateOptions()
                {
                    CancelAtPeriodEnd = true
                };
            }
            else
            {
                options = new Stripe.SubscriptionUpdateOptions()
                {
                    CancelAt = DateTime.Now.AddSeconds(15),
                    ProrationDate = DateTime.Now
                };
            }


            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.Subscription subscription = null;

            try
            {
                subscription = GetSubscriptionService().Get(subscriptionId, null, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe subscription found with ID: {subscriptionId}", MessageType.Notice, ex);
            }

            return subscription;
        }

        #endregion

        #region StripeCard

        private static Stripe.CardService GetCardService()
        {
            return new Stripe.CardService(StripeClient);
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
                    Source = token
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
                var options = new Stripe.CardListOptions
                {                    
                    Limit = 10,
                };

                cards.AddRange(GetCardService().List(customerId, options).ToList());
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

        #region StripeBankAccount

        private static Stripe.BankAccountService GetBankAccountService()
        {
            return new Stripe.BankAccountService(StripeClient);
        }

        public static List<Stripe.BankAccount> GetBankAccounts(string customerId, SystemMessages msgs)
        {
            var bankAccounts = new List<Stripe.BankAccount>();

            try
            {
                bankAccounts = GetBankAccountService().List(customerId).ToList();
            }
            catch (StripeException ex)
            {
                msgs.Add($"Cannot get list of bank accounts", MessageType.Error, ex);
            }

            return bankAccounts.Where(bankAccount => bankAccount.Object == "bank_account").ToList();
        }

        #endregion

        #region StripeSource

        public const string TestSourceToken = "tok_visa";

        private static Stripe.SourceService GetSourceService()
        {
            return new Stripe.SourceService(StripeClient);
        }

        public static Stripe.Source CreateCreditCardSource(string token, SystemMessages msgs)
        {
            return CreateCreditCardSource(token, null, msgs);
        }

        public static Stripe.Source CreateCreditCardSource(string token, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.SourceCreateOptions()
            {
                Type = "card",
                Token = token,
            };

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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
                requestOptions.StripeAccount = stripeAccountId;

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
            var sources = new List<Stripe.Source>();

            try
            {
                sources = GetSourceService().List(customerId).ToList();
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Cannot get list of payment sources", MessageType.UserError);
            }

            return sources;
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
            return new Stripe.ChargeService(StripeClient);
        }        

        public static Stripe.Charge CreateCharge(string fromCustomerId, string tokenOrSourceId, decimal totalAmount, string description, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            return CreateCharge(fromCustomerId, null, tokenOrSourceId, totalAmount, null, description, statementDescriptor, metaData, msgs);
        }

        public static Stripe.Charge CreateCharge(string fromCustomerId, string destinationAccountId, string tokenOrSourceId, decimal totalAmount, decimal? platformFee, string description, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.ChargeCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Currency = "usd",
                Description = description,
                Source = tokenOrSourceId,
                StatementDescriptorSuffix = statementDescriptor.Replace("'", "").MaxChars(22),
                Customer = fromCustomerId,
                Metadata = metaData,             
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

        public static Stripe.Charge GetCharge(string stripeChargeId, SystemMessages msgs)
        {
            Stripe.Charge charge = null;

            try
            {
                charge = GetChargeService().Get(stripeChargeId);
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
                Customer = customerId,
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

        public static List<Stripe.Charge> GetCharges(string startingAfter, int limit, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<Stripe.Charge>();
            var options = new Stripe.ChargeListOptions
            {
                StartingAfter = startingAfter,
                Limit = limit,               
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

        /// <summary>
        /// This method will be used to charge connected account for refund
        /// Right now, while refund we refund all the amount from platform to parent
        /// and then platform will collec the actiivty amount from provider
        /// </summary>
        /// <param name="totalAmount"></param>
        /// <param name="connectedAccountId"></param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public static Stripe.Charge CreateRefundChargeOnConnectedAccount(decimal totalAmount, string connectedAccountId, Dictionary<string,string> metaData, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new ChargeCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Currency = "usd",
                Source = connectedAccountId,
                Metadata = metaData,
            };

            Stripe.Charge charge = null;

            try
            {
                charge = GetChargeService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Error charging connected account while refund", MessageType.UserError);
            }

            return charge;
        }
        #endregion

        #region Stripe Payment Intent

        private static Stripe.PaymentIntentService GetPaymentIntentService()
        {
            return new Stripe.PaymentIntentService(StripeClient);
        }

        public static Stripe.PaymentIntent GetPaymentIntent(string paymentIntentId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Stripe.PaymentIntent paymentIntent = null;

            try
            {
                paymentIntent = GetPaymentIntentService().Get(paymentIntentId, null, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Error retrieving payment source.", MessageType.UserError);
            }

            return paymentIntent;
        }

        //..TODO: 
        //..Intialized payment intent to check if refund works properly for below task.
        //..I've kept this code as this will be usefull in future.
        //..Review Refund Process(https://app.clickup.com/t/k50ay4)

        public static Stripe.PaymentIntent CreatePaymentIntent(string fromCustomerId, string destinationAccountId, string tokenOrSourceId, decimal totalAmount, decimal? platformFee, string description, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            var createOptions = new PaymentIntentCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                ApplicationFeeAmount = (platformFee.Value * 100).ToInteger(),
                Currency = "usd",
                Customer = fromCustomerId,
                Description = description,
                Source = tokenOrSourceId,
                StatementDescriptorSuffix = statementDescriptor.Replace("'", "").MaxChars(22),
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = destinationAccountId,
                },
            };

            Stripe.PaymentIntent charge = null;

            try
            {
                charge = GetPaymentIntentService().Create(createOptions);
                GetPaymentIntentService().Confirm(charge.Id);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Error charging payment source", MessageType.UserError);
            }

            return charge;
        }

        #endregion

        #region StripeTokens

        private static Stripe.TokenService GetTokenService()
        {
            return new Stripe.TokenService(StripeClient);
        }

        public static Stripe.Token CreateCustomerToken(string customerId, string stripeAccountId, string sourceId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.TokenCreateOptions()
            {
                Customer = customerId,
                Card = new AnyOf<string, CreditCardOptions>(sourceId)
            };

            var requestOptions = new Stripe.RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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
            return new Stripe.RefundService(StripeClient);
        }

        public static Stripe.Refund CreateRefund(string chargeId, decimal totalAmount, string reason, bool reverseTransfer = true, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.RefundCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Reason = reason,
                ReverseTransfer = reverseTransfer,
                // RefundApplicationFee = true,
                Charge = chargeId,                
            };

            Refund refund = null;

            try
            {
                refund = GetRefundService().Create(options);
            }
            catch (StripeException ex)
            {
                if (ex.StripeError.Code == "charge_already_refunded")
                {
                    var refunds = GetRefunds(chargeId, msgs);
                    var amount = Convert.ToInt64(totalAmount * 100);
                    refund = refunds.FirstOrDefault(x => x.Amount == amount);
                }
                else
                {
                    msgs.Add(ex.Message, MessageType.UserError, ex);
                    msgs.Add($"Error refunding", MessageType.UserError);
                }
            }

            return refund;
        }

        public static List<Stripe.Refund> GetRefunds(string chargeId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<Stripe.Refund>();
            var options = new Stripe.RefundListOptions
            {
                 Charge = chargeId
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
            return new Stripe.CouponService(StripeClient);
        }

        public static Stripe.Coupon CreateCoupon(decimal discountValue, string stripeAccountId, CouponAmountType amountType, StripeCouponDuration couponDuration, int? durationInMonths, SystemMessages msgs)
        {
            return CreateCoupon(discountValue, couponDuration, durationInMonths, amountType, stripeAccountId, msgs);
        }

        public static Stripe.Coupon CreateCoupon(decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths, CouponAmountType couponAmountType, string stripeAccountId, SystemMessages msgs)
        {
            return CreateCoupon(null, discountValue, couponDuration, durationInMonths, couponAmountType, stripeAccountId, msgs);
        }

        public static Stripe.Coupon CreateCoupon(string couponName, decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths, CouponAmountType couponAmountType, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new Stripe.CouponCreateOptions();

            if (couponDuration == StripeCouponDuration.Repeating)
            {
                options.DurationInMonths = durationInMonths;
                options.Duration = StripeCouponDuration.Repeating.ToString().ToLower();
            }
            else if (couponDuration == StripeCouponDuration.Once)
                options.Duration = StripeCouponDuration.Once.ToString().ToLower();
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

            if (couponName.IsNotEmpty())
                options.Name = couponName.MaxChars(40);

            var requestOptions = new Stripe.RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

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

        #region StripeInvoices
        private static Stripe.InvoiceService GetInvoiceService()
        {
            return new Stripe.InvoiceService(StripeClient);
        }

        public static Stripe.Invoice GetInvoice(string invoiceId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Invoice invoice = null;

            try
            {
                invoice = GetInvoiceService().Get(invoiceId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Invoice doesn't exist", MessageType.UserError);
            }

            return invoice;
        }

        #endregion

        #region StripeOAuth

        private static Stripe.OAuthTokenService GetOAuthTokenService()
        {
            return new Stripe.OAuthTokenService(StripeClient);
        }

        public static Stripe.OAuthToken CompleteOAuthConnection(string code, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new OAuthTokenCreateOptions
            {
                GrantType = "authorization_code",
                Code = code,
            };

            OAuthToken token = null;

            try
            {
                token = GetOAuthTokenService().Create(options);
            }
            catch (StripeException ex)
            {
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error connecting stripe account - {ex.Message}", MessageType.UserError);
            }
            catch(Exception ex)
            {
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add($"Error connecting stripe account - {ex.Message}", MessageType.UserError);
            }

            return token;
        }

        public static Stripe.OAuthDeauthorize DeauthorizeOAuthConnection(string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new OAuthDeauthorizeOptions
            {
                ClientId = PlatformClientId,
                StripeUserId = stripeAccountId,
            };

            OAuthDeauthorize oAuthDeauthorize = null;

            try
            {
                oAuthDeauthorize = GetOAuthTokenService().Deauthorize(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error deauthorizing account - {ex.Message}", MessageType.UserError);
            }
            catch (Exception ex)
            {
                msgs.Add($"Error deauthorizing account - {ex.Message}", MessageType.UserError);
            }

            return oAuthDeauthorize;
        }



        #endregion

        #region StripeLoginLink

        private static Stripe.LoginLinkService GetLoginLinkService()
        {
            return new Stripe.LoginLinkService(StripeClient);
        }

        public static Stripe.LoginLink CreateLoginLink(string stripeAccountId, string redirectUrl, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.LoginLink loginLink = null;
            var options = new Stripe.LoginLinkCreateOptions() { RedirectUrl = redirectUrl };

            try
            {
                loginLink = GetLoginLinkService().Create(stripeAccountId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating login link", MessageType.UserError);
            }

            return loginLink;
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
