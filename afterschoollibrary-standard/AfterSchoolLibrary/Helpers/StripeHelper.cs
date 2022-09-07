using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;
using Stripe.BillingPortal;
using Stripe.Checkout;
using Twilio.Types;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Billing
{
    public class StripeHelper
    {
        private static string TestSecretKey => "sk_test_gfeMjl5rgl16L1HHITh3YT5s";
        private static string TestPublishableKey => "pk_test_wuL50O8i1mAzrmV2xDAXuClJ";

        private static string LiveSecretKey => ConfigurationManager.AppSettings["Stripe.LiveSecretKey"];
        private static string LivePublishableKey => ConfigurationManager.AppSettings["Stripe.LivePublishableKey"];

        private static string DevPlatformClientId => "ca_7kmNt22jLhXZWtyVV1884Kng6mgs79Jv";
        private static string LivePlatformClientId => ConfigurationManager.AppSettings["Stripe.LivePlatformClientId"];

        private static string DevSubscriptionsSecretKey => "sk_test_51L86SRKqvjOx1xht5HLUSdCXXvePMWwsmWyqjJpwH5vBWPJaBcwrRM3HqfNHAsjn8cDolgGx2NQr7P7tombgd1Kw000Ej19ATs";
        private static string DevSubscriptionsPublishableKey => "pk_test_51L86SRKqvjOx1xhtySfmOFLymwZjBhtoUweSkImAJbF52trFklUdsEmDewGcxs7JFmZddTvSmkAhj3f8UfkMJOpV00aFruefGr";

        public const decimal StripePercentFee = .029M;
        public const decimal StripeFlatFee = .3M;
        public const decimal StripeBankPercentFee = .008M;

        public static decimal CalculateStripeFee(decimal cost, bool feeTakenOutOfCost)
        {
            var fee = feeTakenOutOfCost
                ? cost * StripePercentFee + StripeFlatFee
                : (cost + StripeFlatFee) / (1 - StripePercentFee) - cost;
            return Math.Round(fee, 2, MidpointRounding.AwayFromZero);
        }
        public static decimal CalculateStripeAchFee(decimal cost, bool feeTakenOutOfCost)
        {
            var fee = feeTakenOutOfCost
                ? cost * StripeBankPercentFee
                : (cost / (1 - StripeBankPercentFee)) - cost;
            return Math.Round(fee, 2, MidpointRounding.AwayFromZero);
        }

        public static string SecretKey => Helpers.Utilities.IsLocalHost ? TestSecretKey : LiveSecretKey;
        public static string PublishableKey  => Helpers.Utilities.IsLocalHost ? TestPublishableKey : LivePublishableKey;
        public static string PlatformClientId => Helpers.Utilities.IsLocalHost ? DevPlatformClientId : LivePlatformClientId;
        public static string SubscriptionsSecretKey => Helpers.Utilities.IsLocalHost ? DevSubscriptionsSecretKey : ConfigurationManager.AppSettings["Stripe.SubscriptionsSecretKey"];
        public static string SubscriptionsPublishableKey => Helpers.Utilities.IsLocalHost ? DevSubscriptionsPublishableKey : ConfigurationManager.AppSettings["Stripe.SubscriptionsPublishableKey"];
        
        public static StripeHelper GetStripeHelperSubscriberAccount() => new StripeHelper(StripeHelper.SubscriptionsSecretKey);
        public static StripeHelper GetStripeHelperConnectAccount() => new StripeHelper(StripeHelper.SecretKey);
        public static StripeHelper GetStripeHelper(StripeAccountType stripeAccountType) => stripeAccountType == StripeAccountType.SubscriberAccount ?  GetStripeHelperSubscriberAccount() : GetStripeHelperConnectAccount();

        private string StripeAccessToken { get; set; }
        private StripeClient StripeClient => new StripeClient(apiKey: StripeAccessToken);

        public StripeHelper()
        {
            StripeAccessToken = SecretKey;
        }

        public StripeHelper(string secretKey)
        {
            StripeAccessToken = secretKey;
        }
        
        #region StripeAccounts

        public AccountService GetAccountService(string accessToken = null)
        {
            return new AccountService(StripeClient);
        }

        public Stripe.Account GetConnectedAccount(string stripeAccountId, SystemMessages msgs)
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

        public Stripe.Account UpdateCustomAccount(string stripeAccountId, AccountUpdateOptions options, SystemMessages msgs)
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

        public Stripe.Account ToggleStripePayoutForConnectedAccount(string customAccountsStripeUserId, string accountName, PayoutIntervalType payoutIntervalType, SystemMessages msgs)
        {
            var options = new AccountUpdateOptions()
            {
                Settings = new AccountSettingsOptions()
                {
                    Payouts = new AccountSettingsPayoutsOptions()
                    {
                        Schedule = new AccountSettingsPayoutsScheduleOptions()
                        {
                            Interval = payoutIntervalType.ToString().ToLower()
                        }
                    }
                }
            };

            var stripeAccount = UpdateCustomAccount(customAccountsStripeUserId, options, msgs);

            if (msgs.HasErrors())
                return null;

            if (stripeAccount.IsNotEmpty())
                msgs.Add($"Account: ({accountName}) updated with { (payoutIntervalType == PayoutIntervalType.Manual ? "manual AHQ" : "daily Stripe" )} payout option.", MessageType.Success);

            return stripeAccount;
        }

        #endregion

        #region StripePerson

        private PersonService GetPersonService()
        {
            return new PersonService(StripeClient);
        }

        public Stripe.Person CreatePerson(string stripeAccountId, PersonCreateOptions options, SystemMessages msgs)
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

        public Stripe.Person GetPerson(string stripeAccountId, string stripePersonId, SystemMessages msgs)
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

        private TransferService GetTransferService()
        {
            return new TransferService(StripeClient);
        }

        private TransferReversalService GetTranferReversalService()
        {
            return new TransferReversalService(StripeClient);
        }

        public Transfer CreateTransfer(decimal amount, string description, string transferIdentifier, string destinationId, string chargeId, Dictionary<string, string> metaData, SystemMessages msgs)
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


        internal TransferReversal CreateTransferReversal(string stripeTransferId, decimal amountToReverse, SystemMessages msgs)
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

        private PayoutService GetPayoutService()
        {
            return new PayoutService(StripeClient);
        }

        public Payout CreatePayout(decimal amount, string externalAccountId, string connectedAccountId, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new PayoutCreateOptions()
            {
                Amount = (long)(amount * 100),
                Currency = "usd",
                Destination = externalAccountId,
                Metadata = metaData,
                Method = "standard",
                StatementDescriptor = statementDescriptor
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

        public Stripe.Payout GetPayout(string payoutId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var requestOptions = new RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Payout stripePayout = null;

            try
            {
                stripePayout = GetPayoutService().Get(payoutId, null, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No Stripe Payout found with ID: {payoutId}", MessageType.Notice, ex);
            }

            return stripePayout;
        }

        #endregion

        #region StripeFiles

        private FileService GetFileUploadService()
        {
            return new FileService(StripeClient);
        }

        public Stripe.File UploadFile(Stream file, string filename, StripeFilePurpose purpose, SystemMessages msgs)
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

        private ProductService GetProductService()
        {
            return new ProductService(StripeClient);
        }        

        public Stripe.Product CreateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
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

        public Stripe.Product UpdateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, SystemMessages msgs)
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

        public SystemMessages DeleteProduct(string productId)
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

        public Stripe.Product GetProduct(string productId, string stripeAccountId, SystemMessages msgs)
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
            catch (StripeException)
            {
                // msgs.Add($"No stripe product found with ID: {productId}", MessageType.Notice, ex);
            }

            return product;
        }

        #endregion

        #region StripePrices
        private PriceService GetPriceService()
        {
            return new PriceService(StripeClient);
        }

        public Stripe.Price CreatePrice(string productId, decimal amount, string nickname, BillingPeriodType billingPeriodType, string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
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

        public Stripe.Price GetPrice(string priceId, SystemMessages msgs)
        {
            Stripe.Price price = null;

            try
            {
                price = GetPriceService().Get(priceId);
            } 
            catch (StripeException)
            {
                //msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Unable to retrieve price", MessageType.Notice);
            }

            return price;
        }

        public StripeList<Price> GetProductPrices(string productId, SystemMessages msgs = null)
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

        public SystemMessages DeletePrice(string priceId, SystemMessages msgs)
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

        private PlanService GetPlanService()
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
        public Plan CreatePlan(Stripe.Product stripeProduct, string planId, decimal price, BillingPeriodType billingPeriodType, string planNickName, string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
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

        public Stripe.Plan UpdatePlan(string planId, string planNickName, bool active, string stripeAccountId, SystemMessages msgs)
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

        public SystemMessages DeletePlan(string planId, string stripeAccountId)
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

        public Stripe.Plan GetPlan(string planId, string stripeAccountId, SystemMessages msgs)
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
            catch (StripeException)
            {
                // msgs.Add($"No stripe plan found with ID: {planId}", MessageType.Notice, ex);
            }

            return plan;
        }

        #endregion

        #region StripeCustomers

        private Stripe.CustomerService GetCustomerService()
        {
            return new Stripe.CustomerService(StripeClient);
        }

        public void SetDefaultPaymentMethod(string customerId, string paymentMethodId, SystemMessages msgs)
        {
            try
            {
                var options = new Stripe.CustomerUpdateOptions()
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions()
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                GetCustomerService().Update(customerId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Cannot set default payment option", MessageType.Error, ex);
            }
        }

        public Stripe.Customer CreateCustomer(string accountName, string email, string phone, string description, string sourceId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Customer customer = null;
            var options = new Stripe.CustomerCreateOptions
            {
                Description = description,
                Email = email,
                Name = accountName,
                Phone = phone,
                Metadata = metaData
            };

            try
            {
                customer = GetCustomerService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer was not created", MessageType.UserError);
            }

            return customer;
        }

        public Stripe.Customer CreateCustomerFromToken(string token, string description, string stripeAccountId, SystemMessages msgs)
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

        public SystemMessages UpdateCustomerDefaultCard(string customerId, string stripeAccountId, string cardId)
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
        
        public Stripe.Customer UpdateCustomer(string customerId, string description, string email, string stripeAccountId, SystemMessages msgs)
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

        public SystemMessages DeleteCustomer(string customerId, string stripeAccountId)
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

        public Stripe.Customer GetCustomer(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Customer customer = null;

            try
            {
                customer = GetCustomerService().Get(customerId);
            }
            catch (StripeException)
            {
                // msgs.Add($"No stripe customer found with ID: {customerId}", MessageType.Notice, ex);
            }

            return customer;
        }

        public Stripe.Customer AddDefaultSourceToCustomer(string customerId, string sourceToken, string stripeAccountId, SystemMessages msgs)
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

        public Stripe.Source AttachSourceToCustomer(string customerId, string sourceToken, string stripeAccountId, SystemMessages msgs)
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

        public Stripe.Card AddNewCardToCustomer(string customerId, string token, string stripeAccountId, SystemMessages msgs)
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

        public Stripe.Card AddNewCardToCustomer(string customerId, Stripe.Source source, string stripeAccountId, SystemMessages msgs)
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

        public Stripe.BankAccount AddNewBankAccountToCustomer(string customerId, Stripe.Source source, string stripeAccountId, SystemMessages msgs)
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

        #region StripePaymentMethods
        
        private Stripe.PaymentMethodService GetPaymentMethodService()
        {
            return new Stripe.PaymentMethodService(StripeClient);
        }
        
        public List<Stripe.PaymentMethod> GetPaymentMethods(string customerId, StripePaymentMethodType paymentMethodType, SystemMessages msgs)
        {
            var paymentMethods = new List<Stripe.PaymentMethod>();

            try
            {
                var options = new Stripe.PaymentMethodListOptions()
                {
                    Customer = customerId,
                    Type = paymentMethodType.ToString()
                };

                paymentMethods.AddRange(GetPaymentMethodService().List(options).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add($"Cannot get list of payment methods", MessageType.Error, ex);
            }

            return paymentMethods;
        }
        
        #endregion
        
        #region StripeCreditNotes

        private Stripe.CreditNoteService GetCreditNoteService()
        {
            return new Stripe.CreditNoteService(StripeClient);
        }

        public Stripe.CreditNote CreateCreditNote(string invoiceId, decimal amount, string memo, SystemMessages msgs)
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

        private Stripe.CustomerBalanceTransactionService GetCustomerBalanceTransactionService()
        {
            return new Stripe.CustomerBalanceTransactionService(StripeClient);
        }

        public Stripe.CustomerBalanceTransaction CreateCustomerBalanceTransaction(string customerId, decimal amount, string description, Dictionary<string, string> metaData, SystemMessages msgs)
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

        private Stripe.SubscriptionService GetSubscriptionService()
        {
            return new Stripe.SubscriptionService(StripeClient);
        }

        public Stripe.Subscription CreateRegistrantSubscription(string customerId, string planId, bool isProrated, string sourceId, decimal? platformFeePercent, 
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
                ProrationBehavior = isProrated ? "create_prorations" : "none",
                BillingCycleAnchor = billingCycleAnchor
            };

            if (startDate.Value.Date.IsAfter(DateTime.UtcNow.Date.AddDays(1)))
                options.TrialEnd = startDate.Value; // if start date is at least one day in the future, start on start date

            // please make sure that billing ends when the program ends.
            if (endDate.HasValue && endDate.Value.Date.IsOnOrAfter(DateTime.UtcNow.Date.AddDays(1)))
                options.CancelAt = endDate.Value;

            // options.ProrationBehavior = "none";

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

        public Stripe.Subscription CreatePackageSubscription(string customerId, string priceId, string sourceId, DateTime? trialEndDate, string stripeCouponId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Subscription subscription = null;

            var options = new Stripe.SubscriptionCreateOptions
            {
                Customer = customerId,
                CollectionMethod = "charge_automatically",
                Metadata = metaData,
                ProrationBehavior = "none",
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

            if (trialEndDate.HasValue)
                options.TrialEnd = trialEndDate.Value;

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

        public Stripe.Subscription UpdatePackageSubscription(string subscriptionId, string priceId, SystemMessages msgs)
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
        public Stripe.Subscription CreateSubscription(Stripe.Customer customer, Stripe.Plan plan, bool isProrated, string sourceId, decimal? platformFeePercent, DateTime? startDate, string stripeAccountId, string stripeCouponId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            return CreateRegistrantSubscription(customer.Id, plan.Id, isProrated, sourceId, platformFeePercent, startDate, null, null, stripeAccountId, stripeCouponId, metaData, msgs);
        }

        public Stripe.Subscription UpdateSubscription(string subscriptionId, string priceId, bool? billingCycleAnchorNow, bool? billingCycleAnchorUnchanged, DateTime? trialEndDate, bool? prorate, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            var options = new Stripe.SubscriptionUpdateOptions();
            Stripe.Subscription subscription = null;

            if (priceId.IsNotBlank())
                options.Items = new List<Stripe.SubscriptionItemOptions>()
                {
                    new Stripe.SubscriptionItemOptions()
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                };

            if (billingCycleAnchorNow.HasValue)
                options.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now;

            if (billingCycleAnchorUnchanged.HasValue)
                options.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Unchanged;

            if (trialEndDate.HasValue)
                options.TrialEnd = trialEndDate;

            if (prorate.HasValue)
                options.ProrationBehavior = prorate.Value ? "create_prorations" : "none";

            try
            {
                subscription = GetSubscriptionService().Update(subscriptionId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription ({subscriptionId}) was not updated", MessageType.UserError);
            }

            return subscription;
        }

        public void CancelSubscription(string subscriptionId, bool cancelAtPeriodEnd = true, SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

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
                    CancelAt = DateTime.UtcNow.AddSeconds(15),
                };
            }

            try
            {
                var subscription = GetSubscriptionService().Update(subscriptionId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription ({subscriptionId}) was not canceled", MessageType.UserError);
            }
        }

        public Stripe.Subscription GetSubscription(string subscriptionId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            Stripe.Subscription subscription = null;

            try
            {
                subscription = GetSubscriptionService().Get(subscriptionId);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe subscription found with ID: {subscriptionId}", MessageType.Notice, ex);
            }

            return subscription;
        }
        
        public List<Stripe.Subscription> GetSubscriptions(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var subscriptions = new List<Stripe.Subscription>();

            var options = new SubscriptionListOptions()
            {
                Customer = customerId,
            };

            try
            {
                subscriptions = GetSubscriptionService().List(options).ToList();
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe subscriptions found with customer ID: {customerId}", MessageType.Notice, ex);
            }

            return subscriptions;
        }

        #endregion
        
        #region SetupIntent

        private Stripe.SetupIntentService GetSetupIntentService()
        {
            return new Stripe.SetupIntentService(StripeClient);
        }

        public Stripe.SetupIntent CreateSetupIntent(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            Stripe.SetupIntent setupIntent = null;

            var options = new SetupIntentCreateOptions()
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string>() {"card"}
            };

            try
            {
                setupIntent = GetSetupIntentService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Error creating setup intent", MessageType.Notice, ex);
            }

            return setupIntent;
        }
        
        #endregion

        #region StripeCard

        private Stripe.CardService GetCardService()
        {
            return new Stripe.CardService(StripeClient);
        }

        public Stripe.Card CreateCard(Stripe.Customer customer, string token, SystemMessages msgs)
        {
            return CreateCard(customer.Id, token, msgs);
        }

        public Stripe.Card CreateCard(string customerId, string token, SystemMessages msgs)
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

        public List<Stripe.Card> GetCards(string customerId, SystemMessages msgs)
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

        public Stripe.Card GetCard(string customerId, string cardId, SystemMessages msgs)
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

        public SystemMessages DeleteCard(string customerId, string cardId)
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

        //public SourceCard GetTestCard(StripeTestCardType stripeTestCardType)
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

        private Stripe.BankAccountService GetBankAccountService()
        {
            return new Stripe.BankAccountService(StripeClient);
        }

        public List<Stripe.BankAccount> GetBankAccounts(string customerId, SystemMessages msgs)
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

        private Stripe.SourceService GetSourceService()
        {
            return new Stripe.SourceService(StripeClient);
        }

        public Stripe.Source CreateCreditCardSource(string token, SystemMessages msgs)
        {
            return CreateCreditCardSource(token, null, msgs);
        }

        public Stripe.Source CreateCreditCardSource(string token, string stripeAccountId, SystemMessages msgs)
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

        public Stripe.Source CreateSourceFromOriginal(string customerId, string originalSourceId, string stripeAccountId, SystemMessages msgs)
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

        public List<Stripe.Source> GetSources(string customerId, SystemMessages msgs)
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

        public Stripe.Source GetSource(string sourceId, SystemMessages msgs)
        {
            var source = new Stripe.Source();

            try
            {
                source = GetSourceService().Get(sourceId);
            }
            catch (StripeException)
            {

            }

            return source;
        }

        #endregion

        #region StripeCharge

        private Stripe.ChargeService GetChargeService()
        {
            return new Stripe.ChargeService(StripeClient);
        }        

        public Stripe.Charge CreateCharge(string fromCustomerId, string tokenOrSourceId, decimal totalAmount, string description, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            return CreateCharge(fromCustomerId, null, tokenOrSourceId, totalAmount, null, description, statementDescriptor, metaData, msgs);
        }

        public Stripe.Charge CreateCharge(string fromCustomerId, string destinationAccountId, string tokenOrSourceId, decimal totalAmount, decimal? platformFee, string description, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
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
                options.Destination = new ChargeDestinationOptions()
                {
                    Account = destinationAccountId,
                    Amount = (totalAmount * 100).ToInteger() - (platformFee.Value * 100).ToInteger()
                };

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

        public Stripe.Charge GetCharge(string stripeChargeId, SystemMessages msgs)
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

        public List<Stripe.Charge> GetCharges(string customerId, SystemMessages msgs)
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

        public List<Stripe.Charge> GetCharges(string startingAfter, int limit, SystemMessages msgs)
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
        public Stripe.Charge CreateRefundChargeOnConnectedAccount(decimal totalAmount, string connectedAccountId, Dictionary<string,string> metaData, SystemMessages msgs = null)
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

        private Stripe.PaymentIntentService GetPaymentIntentService()
        {
            return new Stripe.PaymentIntentService(StripeClient);
        }

        public Stripe.PaymentIntent GetPaymentIntent(string paymentIntentId, string stripeAccountId, SystemMessages msgs)
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

        #endregion

        #region StripeRefunds

        public Stripe.RefundService GetRefundService()
        {
            return new Stripe.RefundService(StripeClient);
        }

        public Stripe.Refund CreateRefund(string chargeId, decimal totalAmount, string reason, bool reverseTransfer = true, SystemMessages msgs = null)
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

        public List<Stripe.Refund> GetRefunds(string chargeId, SystemMessages msgs)
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

        private Stripe.CouponService GetCouponService()
        {
            return new Stripe.CouponService(StripeClient);
        }

        public Stripe.Coupon CreateCoupon(decimal discountValue, string stripeAccountId, CouponAmountType amountType, StripeCouponDuration couponDuration, int? durationInMonths, SystemMessages msgs)
        {
            return CreateCoupon(discountValue, couponDuration, durationInMonths, amountType, stripeAccountId, msgs);
        }

        public Stripe.Coupon CreateCoupon(decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths, CouponAmountType couponAmountType, string stripeAccountId, SystemMessages msgs)
        {
            return CreateCoupon(null, discountValue, couponDuration, durationInMonths, couponAmountType, stripeAccountId, msgs);
        }

        public Stripe.Coupon CreateCoupon(string couponName, decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths, CouponAmountType couponAmountType, string stripeAccountId, SystemMessages msgs)
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
        private Stripe.InvoiceService GetInvoiceService()
        {
            return new Stripe.InvoiceService(StripeClient);
        }

        public Stripe.Invoice GetInvoice(string invoiceId, SystemMessages msgs)
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

        public Stripe.Invoice GetUpcomingInvoice(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Invoice invoice = null;

            var options = new UpcomingInvoiceOptions
            {
                Customer = customerId,
            };

            try
            {
                invoice = GetInvoiceService().Upcoming(options);
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

        private Stripe.OAuthTokenService GetOAuthTokenService()
        {
            return new Stripe.OAuthTokenService(StripeClient);
        }

        public Stripe.OAuthToken CompleteOAuthConnection(string code, SystemMessages msgs)
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

        public Stripe.OAuthDeauthorize DeauthorizeOAuthConnection(string stripeAccountId, SystemMessages msgs)
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

        private Stripe.LoginLinkService GetLoginLinkService()
        {
            return new Stripe.LoginLinkService(StripeClient);
        }

        public Stripe.LoginLink CreateLoginLink(string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.LoginLink loginLink = null;

            try
            {
                loginLink = GetLoginLinkService().Create(stripeAccountId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating login link", MessageType.UserError);
            }

            return loginLink;
        }

        #endregion
        
        #region StripeBillingPortalSession

        private Stripe.BillingPortal.SessionService GetBillingPortalSessionService()
        {
            return new Stripe.BillingPortal.SessionService(StripeClient);
        }

        public Stripe.BillingPortal.Session GetBillingPortalSession(string customerId, string billingPortalConfigId, string redirectUrl, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.BillingPortal.Session session = null;
            var options = new Stripe.BillingPortal.SessionCreateOptions()
            {
                Customer = customerId,
                ReturnUrl = redirectUrl,
                Configuration = billingPortalConfigId,
            };

            try
            {
                session = GetBillingPortalSessionService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating session", MessageType.UserError);
            }

            return session;
        }
        
        #endregion
        
        #region BillingPortalConfiguration
        
        private Stripe.BillingPortal.ConfigurationService GetBillingPortalConfigurationService()
        {
            return new Stripe.BillingPortal.ConfigurationService(StripeClient);
        }
        
        public Stripe.BillingPortal.Configuration CreateBillingPortalConfigurationPaymentMethodUpdate(SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.BillingPortal.Configuration configuration = null;
            var options = new Stripe.BillingPortal.ConfigurationCreateOptions()
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions()
                {
                    PaymentMethodUpdate = new ConfigurationFeaturesPaymentMethodUpdateOptions()
                    {
                        Enabled = true
                    }
                },
            };

            try
            {
                configuration = GetBillingPortalConfigurationService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }

        private ConfigurationBusinessProfileOptions GetConfigurationBusinessProfileOptions()
        {
            return new ConfigurationBusinessProfileOptions()
            {
                Headline = "AfterSchool HQ partners with Stripe for simplified billing",
                PrivacyPolicyUrl = $"{Utilities.BaseUrl()}/privacy",
                TermsOfServiceUrl = $"{Utilities.BaseUrl()}/tos",
            };
        }
        
        public Stripe.BillingPortal.Configuration CreateBillingPortalConfigurationSubscriptionUpdate(List<ConfigurationFeaturesSubscriptionUpdateProductOptions> productOptionsList, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.BillingPortal.Configuration configuration = null;
            var options = new Stripe.BillingPortal.ConfigurationCreateOptions()
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions()
                {
                    SubscriptionUpdate = new ConfigurationFeaturesSubscriptionUpdateOptions()
                    {
                        Enabled = true,
                        DefaultAllowedUpdates = new List<string>() { "price", "promotion_code" },
                        Products = productOptionsList,
                    },
                    PaymentMethodUpdate = new ConfigurationFeaturesPaymentMethodUpdateOptions()
                    {
                        Enabled = true
                    },
                },
            };

            try
            {
                configuration = GetBillingPortalConfigurationService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }
        
        public Stripe.BillingPortal.Configuration UpdateBillingPortalConfigurationPaymentMethodUpdate(string billingPortalConfigId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.BillingPortal.Configuration configuration = null;
            var options = new Stripe.BillingPortal.ConfigurationUpdateOptions()
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions()
                {
                    PaymentMethodUpdate = new ConfigurationFeaturesPaymentMethodUpdateOptions()
                    {
                        Enabled = true
                    }
                },
            };

            try
            {
                configuration = GetBillingPortalConfigurationService().Update(billingPortalConfigId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }
        
        public Stripe.BillingPortal.Configuration UpdateBillingPortalConfigurationSubscriptionUpdate(string billingPortalConfigId, List<ConfigurationFeaturesSubscriptionUpdateProductOptions> productOptionsList, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.BillingPortal.Configuration configuration = null;
            var options = new Stripe.BillingPortal.ConfigurationUpdateOptions()
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions()
                {
                    SubscriptionUpdate = new ConfigurationFeaturesSubscriptionUpdateOptions()
                    {
                        Enabled = true,
                        DefaultAllowedUpdates = new List<string>() { "price", "promotion_code" },
                        Products = productOptionsList,
                    }
                },
            };

            try
            {
                configuration = GetBillingPortalConfigurationService().Update(billingPortalConfigId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }
        
        #endregion
        
        #region StripeCheckoutSession
        
        private Stripe.Checkout.SessionService GetCheckoutSessionService()
        {
            return new Stripe.Checkout.SessionService(StripeClient);
        }

        public Stripe.Checkout.Session GetCheckoutSession(string customerId, string priceId, string successUrl, string cancelUrl, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Checkout.Session session = null;
            var options = new Stripe.Checkout.SessionCreateOptions()
            {
                Customer = customerId,
                AllowPromotionCodes = true,
                AutomaticTax = new SessionAutomaticTaxOptions()
                {
                    Enabled = true,
                },
                CancelUrl = cancelUrl,
                SuccessUrl = successUrl,
                Mode = "subscription",
                BillingAddressCollection = "required",
                LineItems = new List<SessionLineItemOptions>()
                {
                    new SessionLineItemOptions()
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                CustomerUpdate = new SessionCustomerUpdateOptions()
                {
                    Address = "auto",
                },
            };

            try
            {
                session = GetCheckoutSessionService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error creating session", MessageType.UserError);
            }

            return session;
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

    public enum StripePaymentMethodType
    {
        card,
        us_bank_account
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

    public enum StripeAccountType
    {
        SubscriberAccount,
        ConnectAccount,
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
