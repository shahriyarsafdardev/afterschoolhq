using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;
using Stripe.BillingPortal;
using Stripe.Checkout;
using Configuration = Stripe.BillingPortal.Configuration;
using Session = Stripe.BillingPortal.Session;
using SessionCreateOptions = Stripe.BillingPortal.SessionCreateOptions;
using SessionService = Stripe.BillingPortal.SessionService;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Billing
{
    public class StripeHelper
    {
        public const decimal StripePercentFee = .029M;
        public const decimal StripeFlatFee = .3M;
        public const decimal StripeBankPercentFee = .008M;

        public StripeHelper()
        {
            StripeAccessToken = SecretKey;
        }

        private StripeHelper(string secretKey)
        {
            StripeAccessToken = secretKey;
        }
        private static string TestSecretKey => "sk_test_517VG1oCEgXQRZLhk2178KIZ4q6KFHMrjhlF3UX5bJ60zJcq9R9RKw85ua0ycmzTaeQkxew6VXSEunXRU7ffhqnIF00KMbqHheO";
        private static string TestPublishableKey => "pk_test_517VG1oCEgXQRZLhkAI207PAZC7tECHQteJ5pCE2tVZUXwj396PAyJuMxWjPjPfSVmGgj2evpPLBGZPWRkC9T6C7h00K9a3RwWP";

        private static string LiveSecretKey => ConfigurationManager.AppSettings["Stripe.LiveSecretKey"];
        private static string LivePublishableKey => ConfigurationManager.AppSettings["Stripe.LivePublishableKey"];

        private static string DevPlatformClientId => "ca_7kmNt22jLhXZWtyVV1884Kng6mgs79Jv";
        private static string LivePlatformClientId => ConfigurationManager.AppSettings["Stripe.LivePlatformClientId"];

        private static string DevSubscriptionsSecretKey => "sk_test_51L86SRKqvjOx1xht5HLUSdCXXvePMWwsmWyqjJpwH5vBWPJaBcwrRM3HqfNHAsjn8cDolgGx2NQr7P7tombgd1Kw000Ej19ATs";
        private static string DevSubscriptionsPublishableKey => "pk_test_51L86SRKqvjOx1xhtySfmOFLymwZjBhtoUweSkImAJbF52trFklUdsEmDewGcxs7JFmZddTvSmkAhj3f8UfkMJOpV00aFruefGr";

        public static string ReferralCouponId => Utilities.IsProduction ? "4BOLxUYy" : "5zMx3Z7l";

        public static string SecretKey => Utilities.IsLocalHost ? TestSecretKey : LiveSecretKey;
        public static string PublishableKey => Utilities.IsLocalHost ? TestPublishableKey : LivePublishableKey;
        public static string PlatformClientId => Utilities.IsLocalHost ? DevPlatformClientId : LivePlatformClientId;

        private static string SubscriptionsSecretKey =>
            Utilities.IsLocalHost ? DevSubscriptionsSecretKey : ConfigurationManager.AppSettings["Stripe.SubscriptionsSecretKey"];

        public static string SubscriptionsPublishableKey =>
            Utilities.IsLocalHost
                ? DevSubscriptionsPublishableKey
                : ConfigurationManager.AppSettings["Stripe.SubscriptionsPublishableKey"];

        private string StripeAccessToken { get; }
        private StripeClient StripeClient => new StripeClient(StripeAccessToken);

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

        public static StripeHelper GetStripeHelperSubscriberAccount()
        {
            return new StripeHelper(SubscriptionsSecretKey);
        }

        public static StripeHelper GetStripeHelperConnectAccount()
        {
            return new StripeHelper(SecretKey);
        }

        public static StripeHelper GetStripeHelper(StripeAccountType stripeAccountType)
        {
            return stripeAccountType == StripeAccountType.SubscriberAccount ? GetStripeHelperSubscriberAccount() : GetStripeHelperConnectAccount();
        }

        #region StripeAccounts

        private AccountService GetAccountService()
        {
            return new AccountService(StripeClient);
        }

        public Account GetConnectedAccount(string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Account account = null;

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

        private Account UpdateCustomAccount(string stripeAccountId, AccountUpdateOptions options, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();
            Account account = null;

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

        public Account ToggleStripePayoutForConnectedAccount(string customAccountsStripeUserId, string accountName, PayoutIntervalType payoutIntervalType,
            SystemMessages msgs)
        {
            var options = new AccountUpdateOptions
            {
                Settings = new AccountSettingsOptions
                {
                    Payouts = new AccountSettingsPayoutsOptions
                    {
                        Schedule = new AccountSettingsPayoutsScheduleOptions
                        {
                            Interval = payoutIntervalType.ToString().ToLower(),
                        },
                    },
                },
            };

            var stripeAccount = UpdateCustomAccount(customAccountsStripeUserId, options, msgs);

            if (msgs.HasErrors())
                return null;

            if (stripeAccount.IsNotEmpty())
                msgs.Add(
                    $"Account: ({accountName}) updated with {(payoutIntervalType == PayoutIntervalType.Manual ? "manual AHQ" : "daily Stripe")} payout option.",
                    MessageType.Success);

            return stripeAccount;
        }

        #endregion

        #region StripeTransfer

        private TransferService GetTransferService()
        {
            return new TransferService(StripeClient);
        }

        private TransferReversalService GetTransferReversalService()
        {
            return new TransferReversalService(StripeClient);
        }

        public Transfer CreateTransfer(decimal amount, string description, string transferIdentifier, string destinationId, string chargeId,
            Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new TransferCreateOptions
            {
                Amount = (long) (amount * 100),
                Currency = "usd",
                Destination = destinationId,
                Metadata = metaData,
                Description = description,
                TransferGroup = transferIdentifier,
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
                msgs.Add("Transfer was not created", MessageType.UserError);
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

        public Payout CreatePayout(decimal amount, string externalAccountId, string connectedAccountId, string statementDescriptor,
            Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new PayoutCreateOptions
            {
                Amount = (long) (amount * 100),
                Currency = "usd",
                Destination = externalAccountId,
                Metadata = metaData,
                Method = "standard",
                StatementDescriptor = statementDescriptor,
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = connectedAccountId,
            };

            Payout stripePayout = null;

            try
            {
                stripePayout = GetPayoutService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Payout was not created", MessageType.UserError);
            }

            return stripePayout;
        }

        #endregion

        #region StripeProducts

        private ProductService GetProductService()
        {
            return new ProductService(StripeClient);
        }

        public Product CreateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId,
            Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Product product = null;

            var options = new ProductCreateOptions
            {
                Id = productId,
                Name = productName,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                Active = active,
                Type = "service",
            };

            var requestOptions = new RequestOptions();

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
                msgs.Add("Product was not created", MessageType.UserError);
            }

            return product;
        }

        public Product UpdateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();


            var options = new ProductUpdateOptions
            {
                Name = productName,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                Active = active,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Product product = null;

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

        public Product GetProduct(string productId, string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var requestOptions = new RequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Product product = null;

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

        public Price CreatePrice(string productId, decimal amount, string nickname, BillingPeriodType billingPeriodType, string stripeAccountId,
            Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Price price = null;

            var options = new PriceCreateOptions
            {
                Product = productId,
                UnitAmount = (long) amount,
                Currency = "usd",
                Nickname = nickname,
                Recurring = new PriceRecurringOptions
                {
                    Interval = billingPeriodType.ToString().ToLower(),
                },
            };

            var requestOptions = new RequestOptions();

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
                msgs.Add("Price was not created", MessageType.ApiError);
            }

            return price;
        }

        public Price GetPrice(string priceId, SystemMessages msgs)
        {
            Price price = null;

            try
            {
                price = GetPriceService().Get(priceId);
            }
            catch (StripeException)
            {
                //msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Unable to retrieve price", MessageType.Notice);
            }

            return price;
        }

        #endregion

        #region StripePlans

        private PlanService GetPlanService()
        {
            return new PlanService(StripeClient);
        }

        /// <summary>
        ///     Create a stripe plan
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
        public Plan CreatePlan(Product stripeProduct, string planId, decimal price, BillingPeriodType billingPeriodType, string planNickName,
            string stripeAccountId, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var intervalCount = 1;
            var interval = billingPeriodType.ToString().ToLower();

            if (billingPeriodType == BillingPeriodType.TwoWeeks)
            {
                intervalCount = 2;
                interval = BillingPeriodType.Week.ToString().ToLower();
            }

            Plan plan = null;
            var options = new PlanCreateOptions
            {
                Amount = Convert.ToInt32(price * 100),
                Currency = "usd",
                IntervalCount = intervalCount,
                Interval = interval,
                Nickname = planNickName,
                Product = stripeProduct.Id,
            };

            var requestOptions = new RequestOptions();

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
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Plan was not created", MessageType.UserError);
            }

            return plan;
        }

        public Plan UpdatePlan(string planId, string planNickName, bool active, string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new PlanUpdateOptions
            {
                Active = active,
                Nickname = planNickName,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Plan plan = null;

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

        public SystemMessages DeletePlan(string planId)
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

        public Plan GetPlan(string planId, string stripeAccountId)
        {
            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Plan plan = null;

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

        private CustomerService GetCustomerService()
        {
            return new CustomerService(StripeClient);
        }

        public void SetDefaultPaymentMethod(string customerId, string paymentMethodId, SystemMessages msgs)
        {
            try
            {
                var options = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId,
                    },
                };

                GetCustomerService().Update(customerId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add("Cannot set default payment option", MessageType.Error, ex);
            }
        }

        public Customer CreateCustomer(string accountName, string email, string phone, string description, Dictionary<string, string> metaData,
            SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Customer customer = null;
            var options = new CustomerCreateOptions
            {
                Description = description,
                Email = email,
                Name = accountName,
                Phone = phone,
                Metadata = metaData,
            };

            try
            {
                customer = GetCustomerService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Customer was not created", MessageType.UserError);
            }

            return customer;
        }

        public SystemMessages UpdateCustomerDefaultCard(string customerId, string stripeAccountId, string cardId)
        {
            var msgs = new SystemMessages();
            var options = new CustomerUpdateOptions
            {
                DefaultSource = cardId,
            };

            var requestOptions = new RequestOptions();
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

        public Customer UpdateCustomer(string customerId, string description, string email, string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new CustomerUpdateOptions
            {
                Description = description,
                Email = email,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Customer customer = null;

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

        public SystemMessages DeleteCustomer(string customerId)
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

        public Customer GetCustomer(string customerId)
        {
            Customer customer = null;

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

        public Source AttachSourceToCustomer(string customerId, string sourceToken, string stripeAccountId, SystemMessages msgs)
        {
            var options = new SourceAttachOptions
            {
                Source = sourceToken,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Source source = null;

            try
            {
                source = GetSourceService().Attach(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Source was not attached", MessageType.UserError);
            }

            return source;
        }

        public Card AddNewCardToCustomer(string customerId, string token, string stripeAccountId, SystemMessages msgs)
        {
            var options = new CardCreateOptions
            {
                Source = token,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Card card = null;

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

        public void AddNewCardToCustomer(string customerId, Source source, string stripeAccountId, SystemMessages msgs)
        {
            var options = new CardCreateOptions
            {
                Source = source.Id,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            try
            {
                GetCardService().Create(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Customer ({customerId}) was not updated", MessageType.UserError);
            }
        }

        #endregion

        #region StripePaymentMethods

        private PaymentMethodService GetPaymentMethodService()
        {
            return new PaymentMethodService(StripeClient);
        }

        public List<PaymentMethod> GetPaymentMethods(string customerId, StripePaymentMethodType paymentMethodType, SystemMessages msgs)
        {
            var paymentMethods = new List<PaymentMethod>();

            try
            {
                var options = new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = paymentMethodType.ToString(),
                };

                paymentMethods.AddRange(GetPaymentMethodService().List(options).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add("Cannot get list of payment methods", MessageType.Error, ex);
            }

            return paymentMethods;
        }

        #endregion

        #region CustomerBalanceTransaction

        private CustomerBalanceTransactionService GetCustomerBalanceTransactionService()
        {
            return new CustomerBalanceTransactionService(StripeClient);
        }

        public void CreateCustomerBalanceTransaction(string customerId, decimal amount, string description,
            Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new CustomerBalanceTransactionCreateOptions
            {
                Amount = Convert.ToInt64(amount * 100),
                Currency = "usd",
                Description = description,
                Metadata = metaData,
            };

            try
            {
                GetCustomerBalanceTransactionService().Create(customerId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Balance transaction was not created", MessageType.UserError);
            }
        }

        #endregion

        #region StripeSubscriptions

        private const string CollectionMethodChargeAutomatically = "charge_automatically";
        private const string ProrationBehaviorAlwaysInvoice = "always_invoice";
        private const string ProrationBehaviorCreateProrations = "create_prorations";
        private const string ProrationBehaviorNone = "none";

        private SubscriptionService GetSubscriptionService()
        {
            return new SubscriptionService(StripeClient);
        }

        public Subscription CreateRegistrantSubscription(string customerId, string planId, bool isProrated, string sourceId,
            decimal? platformFeePercent, DateTime? startDate, DateTime? endDate, DateTime? billingCycleAnchor, string stripeAccountId, string stripeCouponId,
            Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                CollectionMethod = CollectionMethodChargeAutomatically,
                DefaultSource = sourceId,
                Coupon = stripeCouponId,
                Items = new List<SubscriptionItemOptions> {new SubscriptionItemOptions {Plan = planId, Quantity = 1}},
                Metadata = metaData,
                ProrationBehavior = isProrated ? ProrationBehaviorCreateProrations : ProrationBehaviorNone, // pass in from applesauce
                BillingCycleAnchor = billingCycleAnchor, // pass in from applesauce
            };
            var uIOptions = new UpcomingInvoiceOptions
            {
                SubscriptionBillingCycleAnchor = new DateTime(2022, 11, 1),
                SubscriptionProrationBehavior = ProrationBehaviorCreateProrations,
                SubscriptionStartDate = DateTime.UtcNow.Date,
            };

            // if the start date is after today, then end the trial (start the subscription) on the start date of the subscription
            if (startDate.Value.Date.IsAfter(DateTime.UtcNow.Date))
                options.TrialEnd = startDate.Value; 

            // please make sure that billing ends when the program ends.
            if (endDate.HasValue && endDate.Value.Date.IsOnOrAfter(DateTime.UtcNow.AddDays(1).Date))
                options.CancelAt = endDate.Value;

            if (stripeAccountId.IsNotEmpty())
                options.TransferData = new SubscriptionTransferDataOptions
                {
                    Destination = stripeAccountId,
                    AmountPercent = 100 - platformFeePercent * 100,
                };

            try
            {
                var subscription = GetSubscriptionService().Create(options);
                return subscription;
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Subscription was not created", MessageType.UserError);
                return null;
            }
        }

        public Subscription CreatePackageSubscription(string customerId, string priceId, string sourceId, DateTime? trialEndDate, string stripeCouponId,
            string defaultPaymentMethod, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Subscription subscription = null;

            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                CollectionMethod = CollectionMethodChargeAutomatically,
                Metadata = metaData,
                ProrationBehavior = ProrationBehaviorNone,
            };

            if (defaultPaymentMethod.IsNotEmpty())
                options.DefaultPaymentMethod = defaultPaymentMethod;

            if (sourceId.IsNotEmpty())
                options.DefaultSource = sourceId;

            if (priceId.IsNotBlank())
                options.Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                };

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
                msgs.Add("Subscription was not created", MessageType.UserError);
            }

            return subscription;
        }

        public void UpdatePackageSubscription(string subscriptionId, string priceId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var subscription = GetSubscriptionService().Get(subscriptionId);

            var options = new SubscriptionUpdateOptions
            {
                ProrationBehavior = ProrationBehaviorAlwaysInvoice,
                BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now,
            };

            if (priceId.IsNotBlank())
                options.Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items.Data[0].Id,
                        Price = priceId,
                    },
                };

            try
            {
                GetSubscriptionService().Update(subscriptionId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Subscription was not created", MessageType.UserError);
            }
        }

        public void UpdateSubscription(string subscriptionId, string priceId, bool? billingCycleAnchorNow, bool? billingCycleAnchorUnchanged,
            DateTime? trialEndDate, bool? prorate, string couponId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new SubscriptionUpdateOptions();

            if (priceId.IsNotBlank())
                options.Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                };

            if (couponId.IsNotEmpty())
                options.Coupon = couponId;

            if (billingCycleAnchorNow.HasValue)
                options.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now;

            if (billingCycleAnchorUnchanged.HasValue)
                options.BillingCycleAnchor = SubscriptionBillingCycleAnchor.Unchanged;

            if (trialEndDate.HasValue)
                options.TrialEnd = trialEndDate;

            if (prorate.HasValue)
                options.ProrationBehavior = prorate.Value ? ProrationBehaviorCreateProrations : ProrationBehaviorNone;

            try
            {
                GetSubscriptionService().Update(subscriptionId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add($"Subscription ({subscriptionId}) was not updated", MessageType.UserError);
            }
        }

        public void CancelSubscription(string subscriptionId, bool cancelAtPeriodEnd = true, SystemMessages msgs = null)
        {
            msgs ??= new SystemMessages();

            SubscriptionUpdateOptions options;

            if (cancelAtPeriodEnd)
                options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true,
                };
            else
                options = new SubscriptionUpdateOptions
                {
                    CancelAt = DateTime.UtcNow.AddSeconds(5),
                };

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

        public Subscription GetSubscription(string subscriptionId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();
            Subscription subscription = null;

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

        public List<Subscription> GetSubscriptions(string customerId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();
            var subscriptions = new List<Subscription>();

            var options = new SubscriptionListOptions
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

        private SetupIntentService GetSetupIntentService()
        {
            return new SetupIntentService(StripeClient);
        }

        public SetupIntent CreateSetupIntent(string customerId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();
            SetupIntent setupIntent = null;

            var options = new SetupIntentCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> {"card"},
            };

            try
            {
                setupIntent = GetSetupIntentService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add("Error creating setup intent", MessageType.Notice, ex);
            }

            return setupIntent;
        }

        #endregion

        #region StripeCard

        private CardService GetCardService()
        {
            return new CardService(StripeClient);
        }

        public List<Card> GetCards(string customerId, SystemMessages msgs)
        {
            var cards = new List<Card>();

            try
            {
                var options = new CardListOptions
                {
                    Limit = 10,
                };

                cards.AddRange(GetCardService().List(customerId, options).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add("Cannot get list of cards", MessageType.Error, ex);
            }

            return cards;
        }

        #endregion

        #region StripeBankAccount

        private BankAccountService GetBankAccountService()
        {
            return new BankAccountService(StripeClient);
        }

        public List<BankAccount> GetBankAccounts(string customerId, SystemMessages msgs)
        {
            var bankAccounts = new List<BankAccount>();

            try
            {
                bankAccounts = GetBankAccountService().List(customerId).ToList();
            }
            catch (StripeException ex)
            {
                msgs.Add("Cannot get list of bank accounts", MessageType.Error, ex);
            }

            return bankAccounts.Where(bankAccount => bankAccount.Object == "bank_account").ToList();
        }

        #endregion

        #region StripeSource

        private SourceService GetSourceService()
        {
            return new SourceService(StripeClient);
        }

        public Source CreateCreditCardSource(string token, SystemMessages msgs)
        {
            return CreateCreditCardSource(token, null, msgs);
        }

        private Source CreateCreditCardSource(string token, string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new SourceCreateOptions
            {
                Type = "card",
                Token = token,
            };

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Source source = null;

            try
            {
                source = GetSourceService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Payment source was not created", MessageType.UserError);
            }

            return source;
        }

        public List<Source> GetSources(string customerId, SystemMessages msgs)
        {
            var sources = new List<Source>();

            try
            {
                sources = GetSourceService().List(customerId).ToList();
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Cannot get list of payment sources", MessageType.UserError);
            }

            return sources;
        }

        #endregion

        #region StripeCharge

        private ChargeService GetChargeService()
        {
            return new ChargeService(StripeClient);
        }

        public Charge CreateCharge(string fromCustomerId, string destinationAccountId, string tokenOrSourceId, decimal totalAmount, decimal? platformFee,
            string description, string statementDescriptor, Dictionary<string, string> metaData, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new ChargeCreateOptions
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
                options.Destination = new ChargeDestinationOptions
                {
                    Account = destinationAccountId,
                    Amount = (totalAmount * 100).ToInteger() - (platformFee.Value * 100).ToInteger(),
                };

            Charge charge = null;

            try
            {
                charge = GetChargeService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error charging payment source", MessageType.UserError);
            }

            return charge;
        }

        public Charge GetCharge(string stripeChargeId, SystemMessages msgs)
        {
            Charge charge = null;

            try
            {
                charge = GetChargeService().Get(stripeChargeId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error charging payment source", MessageType.UserError);
            }

            return charge;
        }

        public List<Charge> GetCharges(string customerId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var charges = new List<Charge>();
            var options = new ChargeListOptions
            {
                Customer = customerId,
                Limit = 10,
            };

            try
            {
                charges.AddRange(GetChargeService().List(options).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.UserError, ex);
                msgs.Add("Error retrieving charges", MessageType.UserError);
            }

            return charges;
        }

        #endregion

        #region StripeRefunds

        private RefundService GetRefundService()
        {
            return new RefundService(StripeClient);
        }

        public Refund CreateRefund(string chargeId, decimal totalAmount, string reason, bool reverseTransfer = true, SystemMessages msgs = null)
        {
            msgs ??= new SystemMessages();

            var options = new RefundCreateOptions
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
                    msgs.Add("Error refunding", MessageType.UserError);
                }
            }

            return refund;
        }

        public List<Refund> GetRefunds(string chargeId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var refunds = new List<Refund>();
            var options = new RefundListOptions
            {
                Charge = chargeId,
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

        private CouponService GetCouponService()
        {
            return new CouponService(StripeClient);
        }

        public Coupon CreateCoupon(decimal discountValue, string stripeAccountId, CouponAmountType amountType, StripeCouponDuration couponDuration,
            int? durationInMonths, SystemMessages msgs)
        {
            return CreateCoupon(discountValue, couponDuration, durationInMonths, amountType, stripeAccountId, msgs);
        }

        private Coupon CreateCoupon(decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths, CouponAmountType couponAmountType,
            string stripeAccountId, SystemMessages msgs)
        {
            return CreateCoupon(null, discountValue, couponDuration, durationInMonths, couponAmountType, stripeAccountId, msgs);
        }

        public Coupon CreateCoupon(string couponName, decimal discountValue, StripeCouponDuration couponDuration, int? durationInMonths,
            CouponAmountType couponAmountType, string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new CouponCreateOptions();

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

            var requestOptions = new RequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeAccount = stripeAccountId;

            Coupon coupon = null;

            try
            {
                coupon = GetCouponService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Coupon was not created", MessageType.UserError);
            }

            return coupon;
        }

        #endregion

        #region StripeInvoices

        private InvoiceService GetInvoiceService()
        {
            return new InvoiceService(StripeClient);
        }

        public Invoice PreviewSubscriptionProration(string customerId, string planId, SystemMessages msgs, DateTime billingCycleAnchor)
        {
            msgs ??= new SystemMessages();

            Invoice invoice = null;

            var options = new UpcomingInvoiceOptions
            {
                Customer = customerId,
                SubscriptionBillingCycleAnchor = billingCycleAnchor,
                SubscriptionItems = new List<InvoiceSubscriptionItemOptions> {new InvoiceSubscriptionItemOptions {Plan = planId, Quantity = 1}},
                SubscriptionProrationBehavior = ProrationBehaviorCreateProrations,
                SubscriptionStartDate = DateTime.UtcNow.Date,
            };

            try
            {
                invoice = GetInvoiceService().Upcoming(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Invoice doesn't exist", MessageType.UserError);
            }

            return invoice;
        }

        #endregion

        #region StripeOAuth

        private OAuthTokenService GetOAuthTokenService()
        {
            return new OAuthTokenService(StripeClient);
        }

        public OAuthToken CompleteOAuthConnection(string code, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

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
                Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Error connecting stripe account - {ex.Message}", MessageType.UserError);
            }
            catch (Exception ex)
            {
                Utilities.SystemErrorLog(ex);
                msgs.Add($"Error connecting stripe account - {ex.Message}", MessageType.UserError);
            }

            return token;
        }

        public void DeauthorizeOAuthConnection(string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            var options = new OAuthDeauthorizeOptions
            {
                ClientId = PlatformClientId,
                StripeUserId = stripeAccountId,
            };

            try
            {
                GetOAuthTokenService().Deauthorize(options);
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
        }

        #endregion

        #region StripeLoginLink

        private LoginLinkService GetLoginLinkService()
        {
            return new LoginLinkService(StripeClient);
        }

        public LoginLink CreateLoginLink(string stripeAccountId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            LoginLink loginLink = null;

            try
            {
                loginLink = GetLoginLinkService().Create(stripeAccountId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error creating login link", MessageType.UserError);
            }

            return loginLink;
        }

        #endregion

        #region StripeBillingPortalSession

        private SessionService GetBillingPortalSessionService()
        {
            return new SessionService(StripeClient);
        }

        public Session GetBillingPortalSession(string customerId, string billingPortalConfigId, string redirectUrl, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Session session = null;
            var options = new SessionCreateOptions
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
                msgs.Add("Error creating session", MessageType.UserError);
            }

            return session;
        }
        
        #endregion
        
        #region BillingPortalConfiguration

        private ConfigurationService GetBillingPortalConfigurationService()
        {
            return new ConfigurationService(StripeClient);
        }

        public Configuration CreateBillingPortalConfigurationPaymentMethodUpdate(SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Configuration configuration = null;
            var options = new ConfigurationCreateOptions
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions
                {
                    PaymentMethodUpdate = new ConfigurationFeaturesPaymentMethodUpdateOptions
                    {
                        Enabled = true,
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
                msgs.Add("Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }

        private static ConfigurationBusinessProfileOptions GetConfigurationBusinessProfileOptions()
        {
            return new ConfigurationBusinessProfileOptions
            {
                Headline = "AfterSchool HQ partners with Stripe for simplified billing",
                PrivacyPolicyUrl = $"{Utilities.BaseUrl()}/privacy",
                TermsOfServiceUrl = $"{Utilities.BaseUrl()}/tos",
            };
        }

        public Configuration CreateBillingPortalConfigurationSubscriptionUpdate(List<ConfigurationFeaturesSubscriptionUpdateProductOptions> productOptionsList,
            SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Configuration configuration = null;
            var options = new ConfigurationCreateOptions
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions
                {
                    SubscriptionUpdate = new ConfigurationFeaturesSubscriptionUpdateOptions
                    {
                        Enabled = true,
                        DefaultAllowedUpdates = new List<string> {"price", "promotion_code"},
                        Products = productOptionsList,
                    },
                    PaymentMethodUpdate = new ConfigurationFeaturesPaymentMethodUpdateOptions
                    {
                        Enabled = true,
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
                msgs.Add("Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }

        public Configuration UpdateBillingPortalConfigurationPaymentMethodUpdate(string billingPortalConfigId, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Configuration configuration = null;
            var options = new ConfigurationUpdateOptions
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions
                {
                    PaymentMethodUpdate = new ConfigurationFeaturesPaymentMethodUpdateOptions
                    {
                        Enabled = true,
                    },
                },
            };

            try
            {
                configuration = GetBillingPortalConfigurationService().Update(billingPortalConfigId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }

        public Configuration UpdateBillingPortalConfigurationSubscriptionUpdate(string billingPortalConfigId,
            List<ConfigurationFeaturesSubscriptionUpdateProductOptions> productOptionsList, SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Configuration configuration = null;
            var options = new ConfigurationUpdateOptions
            {
                BusinessProfile = GetConfigurationBusinessProfileOptions(),
                Features = new ConfigurationFeaturesOptions
                {
                    SubscriptionUpdate = new ConfigurationFeaturesSubscriptionUpdateOptions
                    {
                        Enabled = true,
                        DefaultAllowedUpdates = new List<string> {"price", "promotion_code"},
                        Products = productOptionsList,
                    },
                },
            };

            try
            {
                configuration = GetBillingPortalConfigurationService().Update(billingPortalConfigId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error creating configuration", MessageType.UserError);
            }

            return configuration;
        }

        #endregion

        #region StripeCheckoutSession

        private Stripe.Checkout.SessionService GetCheckoutSessionService()
        {
            return new Stripe.Checkout.SessionService(StripeClient);
        }

        public Stripe.Checkout.Session GetCheckoutSession(string customerId, string priceId, string successUrl, string cancelUrl, string couponId,
            SystemMessages msgs)
        {
            msgs ??= new SystemMessages();

            Stripe.Checkout.Session session = null;
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                Customer = customerId,
                AutomaticTax = new SessionAutomaticTaxOptions
                {
                    Enabled = true,
                },
                CancelUrl = cancelUrl,
                SuccessUrl = successUrl,
                Mode = "subscription",
                BillingAddressCollection = "required",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                CustomerUpdate = new SessionCustomerUpdateOptions
                {
                    Address = "auto",
                },
                PaymentMethodTypes = new List<string> {"card"},
            };

            if (couponId.IsNotEmpty())
                options.Discounts = new List<SessionDiscountOptions>
                {
                    new SessionDiscountOptions
                    {
                        Coupon = couponId,
                    },
                };
            else
                options.AllowPromotionCodes = true;

            try
            {
                session = GetCheckoutSessionService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error creating session", MessageType.UserError);
            }

            return session;
        }

        #endregion
    }

    public enum StripePaymentMethodType
    {
        card,
        us_bank_account,
    }

    public enum StripeAccountType
    {
        SubscriberAccount,
        ConnectAccount,
    }

    // Specifies how long the discount will be in effect
    public enum StripeCouponDuration
    {
        Forever,
        Once,
        Repeating,
    }

    internal enum StripeRefundReason
    {
        Duplicate,
        Fraudulent,
        RequestedByCustomer,
    }
}