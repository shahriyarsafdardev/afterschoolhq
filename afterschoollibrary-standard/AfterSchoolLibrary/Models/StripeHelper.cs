using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AfterSchoolLibrary.Billing
{
    public class StripeHelper
    {
        private static string TestSecretKey { get { return "sk_test_gfeMjl5rgl16L1HHITh3YT5s"; } }
        private static string TestPublishableKey { get { return "pk_test_wuL50O8i1mAzrmV2xDAXuClJ"; } }
        private static string LiveSecretKey { get { return "sk_live_SDa559erkpDCJ9BkrPS58GrC"; } }
        private static string LivePublishableKey { get { return "pk_live_4q5JuU2JIxsjWfQauJ7bNISK"; } }

        private static string DevPlatformClientId { get { return "ca_7kmNt22jLhXZWtyVV1884Kng6mgs79Jv"; } }
        private static string LivePlatformClientId { get { return "ca_7kmNqL4oMYfH5z7tY2N9riALjFpkMJAQ"; } }

        public static string SecretKey
        {
            get
            {
                if(Utilities.IsProduction)
                {
                    return LiveSecretKey;
                }
                else
                {
                    return TestSecretKey;
                }
            }
        }
        public static string PublishableKey
        {
            get
            {
                if (Utilities.IsProduction)
                {
                    return LivePublishableKey;
                }
                else
                {
                    return TestPublishableKey;
                }
            }
        }
        public static string PlatformClientId
        {
            get
            {
                if (Utilities.IsProduction)
                {
                    return LivePlatformClientId;
                }
                else
                {
                    return DevPlatformClientId;
                }
            }
        }

        private static string StripeAccessToken { get { return StripeHelper.SecretKey; } }

        #region StripeAccounts

        public static StripeAccountService GetAccountService()
        {
            return new StripeAccountService(StripeAccessToken);
        }

        public static Stripe.StripeAccount GetCustomAccount(string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();            

            Stripe.StripeAccount account = null;

            try
            {
                account = GetAccountService().Get(stripeAccountId);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error getting stripe account", MessageType.Error);
            }

            return account;
        }

        public static Stripe.StripeAccount CreateCustomAccount(string businessName, string email, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripeAccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                DefaultCurrency = "usd",
                BusinessName = businessName,
                Email = email,
                DebitNegativeBalances = true,
                PayoutStatementDescriptor = "AfterSchool transfer".MaxChars(22),
                TransferScheduleInterval = "manual"
            };

            Stripe.StripeAccount account = null;

            try
            {
                account = GetAccountService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error creating stripe account", MessageType.Error);
            }

            return account;
        }

        public static Stripe.StripeAccount UpdateCustomAccountBankInfo(string stripeAccountId, string routingNumber, string accountNumber, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripeAccountUpdateOptions()
            {
                ExternalBankAccount = new StripeAccountBankAccountOptions()
                {
                     RoutingNumber = routingNumber,
                     AccountNumber = accountNumber,
                     Country = "US",
                     Currency = "usd",
                } 
            };

            Stripe.StripeAccount account = null;

            try
            {
                account = GetAccountService().Update(stripeAccountId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error updating stripe account", MessageType.Error);
            }

            return account;
        }

        public static Stripe.StripeAccount UpdateCustomAccountLegalEntity(string stripeAccountId, LegalEntityType entityType, string firstName, string lastName, string ssnLast4, string fullSSN, DateTime dateOfBirth, string address1, string address2, string city, string state, string postalCode, string supportPhone, string supportEmail, string businessName, string ein, string businessUrl, string businessPhone, string documentFileId, DateTime tosAcceptanceDate, string tosIPAddress, string tosUserAgent, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var legalOptions = new StripeAccountLegalEntityOptions()
            {
                AddressCity = city,
                AddressState = state,
                AddressPostalCode = postalCode,
                AddressTwoLetterCountry = "US",
                AddressLine1 = address1,
                AddressLine2 = address2,
                Type = entityType.ToString().ToLower(),
                FirstName = firstName,
                LastName = lastName,
                BirthDay = dateOfBirth.Day,
                BirthMonth = dateOfBirth.Month,
                BirthYear = dateOfBirth.Year,
                SSNLast4 = ssnLast4
            };

            var options = new StripeAccountUpdateOptions()
            {
                LegalEntity = legalOptions,
                TosAcceptanceDate = tosAcceptanceDate,
                TosAcceptanceIp = tosIPAddress,
                SupportPhone = supportPhone,
                SupportEmail = supportEmail
            };

            if (entityType == LegalEntityType.Company)
            {
                legalOptions.BusinessName = businessName;
                legalOptions.BusinessTaxId = ein;
                legalOptions.PhoneNumber = businessPhone;
                options.BusinessUrl = businessUrl;
            }

            if (fullSSN.IsNotEmpty())
            {
                legalOptions.PersonalIdNumber = fullSSN;
            }

            if (documentFileId.IsNotEmpty())
            {
                legalOptions.VerificationDocumentFileId = documentFileId;
            }

            Stripe.StripeAccount account = null;

            try
            {
                account = GetAccountService().Update(stripeAccountId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error updating stripe account", MessageType.Error);
            }

            return account;
        }

        public static void UpdateCustomAccountLegalEntity(string stripeAccountId, LegalEntityType entityType, string firstName, string lastName, DateTime dateOfBirth, string address1, string address2, string city, string state, string postalCode, string businessName, string ein, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var legalOptions = new StripeAccountLegalEntityOptions()
            {
                AddressCity = city,
                AddressState = state,
                AddressPostalCode = postalCode,
                AddressTwoLetterCountry = "US",
                AddressLine1 = address1,
                AddressLine2 = address2,
                Type = entityType.ToString(),
                FirstName = firstName,
                LastName = lastName,
                BirthDay = dateOfBirth.Day,
                BirthMonth = dateOfBirth.Month,
                BirthYear = dateOfBirth.Year
            };

            if (entityType == LegalEntityType.Company)
            {
                legalOptions.BusinessName = businessName;
                legalOptions.BusinessTaxId = ein;
            }

            var options = new StripeAccountUpdateOptions()
            {
                LegalEntity = legalOptions
            };

            Stripe.StripeAccount account = null;

            try
            {
                account = GetAccountService().Update(stripeAccountId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add("Error updating stripe account", MessageType.Error);
            }
        }

        //BusinessName = "",
        //        BusinessUrl = "",
        //        Country = "US",
        //        DefaultCurrency = "usd",
        //        Type = StripeAccountType.Custom.ToString().ToLower(),
        //        Email = "",
        //        SupportEmail = "",
        //        SupportPhone = "",
        //        SupportUrl = "",
        //        StatementDescriptor = "",
        //        ProductDescription = "",
        //        PayoutStatementDescriptor = "",
        //        TosAcceptanceDate = "",
        //        TosAcceptanceUserAgent = "",
        //        TosAcceptanceIp = "",
        //        DebitNegativeBalances = true,
        //        DeclineChargeOnCvcFailure = true,
        //        TransferScheduleInterval = "",
        //        ExternalBankAccount = new StripeAccountBankAccountOptions()
        //{
        //    AccountHolderName = "",
        //            AccountHolderType = "",
        //            AccountNumber = "",
        //            Country = "US",
        //            Currency = "usd",
        //            RoutingNumber = ""
        //        },
        //        LegalEntity = new StripeAccountLegalEntityOptions()
        //{
        //    AddressCity = "",
        //            AddressLine1 = "",
        //            AddressLine2 = "",
        //            AddressPostalCode = "",
        //            AddressState = "",
        //            AddressTwoLetterCountry = "US",
        //            BusinessName = "",
        //            BusinessTaxId = "",
        //            BirthDay = "",
        //            BirthMonth = "",
        //            BirthYear = "",
        //            FirstName = "",
        //            LastName = "",
        //            PersonalAddressCity = "",
        //            PersonalAddressLine1 = "",
        //            PersonalAddressLine2 = "",
        //            PersonalAddressState = "",
        //            PersonalAddressPostalCode = "",
        //            PersonalAddressTwoLetterCountry = "US",
        //            PersonalIdNumber = "",
        //            PhoneNumber = "",
        //            SSNLast4 = "",

        //        },

        #endregion

        #region StripeFiles

        private static StripeFileUploadService GetFileUploadService()
        {
            return new StripeFileUploadService(StripeAccessToken);
        }

        public static StripeFileUpload UploadFile(StripeUploadFile stripeUploadFile, SystemMessages msgs)
        {
            return UploadFile(stripeUploadFile.Document, stripeUploadFile.FileName, stripeUploadFile.Purpose, msgs);
        }

        public static StripeFileUpload UploadFile(Stream file, string filename, StripeFilePurpose purpose, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeFileUpload fileUpload = null;
            
            try
            {
                fileUpload = GetFileUploadService().Create(filename, file, purpose.ToString().SplitByUpperCase().Replace(" ", "_").ToLower());
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe file was not uploaded", MessageType.Error, ex);
            }

            return fileUpload;
        }

        #endregion

        #region StripeProducts

        private static StripeProductService GetProductService()
        {
            return new StripeProductService(StripeAccessToken);
        }

        public static StripeProduct CreateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeProduct product = null;

            var options = new StripeProductCreateOptions
            {
                Id = productId,
                Name = productName,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                Active = active,
                Type = "service"
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                product = GetProductService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe product was not created", MessageType.Error, ex);
            }

            return product;
        }

        public static StripeProduct UpdateProduct(string productId, string productName, string statementDescriptor, bool active, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();


            var options = new StripeProductUpdateOptions
            {
                Name = productName,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                Active = active
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeProduct product = null;

            try
            {
                product = GetProductService().Update(productId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe product ({productId}) was not updated", MessageType.Error, ex);
            }

            return product;
        }

        public static SystemMessages DeleteProduct(string productId, string stripeAccountId)
        {
            var msgs = new SystemMessages();

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var result = GetProductService().Delete(productId, requestOptions);

                if (!result.Deleted)
                    msgs.Add($"Stripe product ({productId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe product ({productId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public static StripeProduct GetProduct(string productId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeProduct product = null;

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

        private static StripePlanService GetPlanService()
        {
            return new StripePlanService(StripeAccessToken);
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
        /// <param name="msgs"></param>
        /// <returns></returns>
        public static StripePlan CreatePlan(StripeProduct stripeProduct, string planId, decimal price, BillingPeriodType billingPeriodType, string planNickName, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripePlan plan = null;
            var options = new StripePlanCreateOptions
            {
                Amount = Convert.ToInt32(price * 100),
                Currency = "usd",
                IntervalCount = 1,
                Interval = billingPeriodType.ToString().ToLower(),
                Nickname = planNickName,
                ProductId = stripeProduct.Id
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            // you can set your own id or just save it and get the plan id back
            if (planId.IsNotEmpty())
                options.Id = planId;

            try
            {
                plan = GetPlanService().Create(options, requestOptions);
            }
            catch(StripeException ex)
            {
                msgs.Add($"Stripe plan was not created", MessageType.Error, ex);
            }

            return plan;
        }

        public static StripePlan UpdatePlan(string planId, string planNickName, bool active, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripePlanUpdateOptions
            {
                Active = active,
                Nickname = planNickName
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripePlan plan = null;

            try
            {
                plan = GetPlanService().Update(planId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe plan ({planId}) was not updated", MessageType.Error, ex);
            }

            return plan;
        }

        public static SystemMessages DeletePlan(string planId, string stripeAccountId)
        {
            var msgs = new SystemMessages();
            var requestOptions = new Stripe.StripeRequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var result = GetPlanService().Delete(planId, requestOptions);

                if (!result.Deleted)
                    msgs.Add($"Stripe plan ({planId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe plan ({planId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public static StripePlan GetPlan(string planId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var requestOptions = new Stripe.StripeRequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripePlan plan = null;

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

        private static StripeCustomerService GetCustomerService()
        {
            return new StripeCustomerService(StripeAccessToken);
        }

        public static StripeCustomer CreateCustomer(string description, string email, string stripeAccountId, SystemMessages msgs)
        {
            return CreateCustomer(description, email, null, null, msgs);
        }

        public static StripeCustomer CreateCustomer(string description, string email, string sourceId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeCustomer customer = null;
            var options = new StripeCustomerCreateOptions
            {
                Description = description,
                Email = email
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                customer = GetCustomerService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer was not created", MessageType.Error, ex);
            }

            return customer;
        }

        public static StripeCustomer CreateCustomerFromToken(string token, string description, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeCustomer customer = null;
            var options = new StripeCustomerCreateOptions
            {
                Description = description,
                SourceToken = token                
            };

            var requestOptions = new StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                customer = GetCustomerService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer was not created", MessageType.Error, ex);
            }

            return customer;
        }

        public static SystemMessages UpdateCustomerDefaultCard(string customerId, string stripeAccountId, string cardId)
        {
            var msgs = new SystemMessages();
            var options = new StripeCustomerUpdateOptions
            {
                DefaultSource = cardId
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                GetCustomerService().Update(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not updated", MessageType.Error, ex);
            }

            return msgs;
        }
        
        public static StripeCustomer UpdateCustomer(string customerId, string description, string email, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            var options = new StripeCustomerUpdateOptions
            {
                Description = description,
                Email = email
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeCustomer customer = null;

            try
            {
                customer = GetCustomerService().Update(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not updated", MessageType.Error, ex);
            }

            return customer;
        }

        public static SystemMessages DeleteCustomer(string customerId, string stripeAccountId)
        {
            var msgs = new SystemMessages();

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                var result = GetCustomerService().Delete(customerId, requestOptions);

                if (!result.Deleted)
                    msgs.Add($"Stripe customer ({customerId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public static StripeCustomer GetCustomer(string customerId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var requestOptions = new Stripe.StripeRequestOptions();

            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeCustomer customer = null;

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

            var options = new StripeCustomerUpdateOptions
            {
                SourceToken = sourceToken
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                GetCustomerService().Update(customerId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not updated", MessageType.Error, ex);
            }

            return msgs;
        }

        #endregion

        #region StripeSubscriptions

        private static StripeSubscriptionService GetSubscriptionService()
        {
            return new StripeSubscriptionService(StripeAccessToken);
        }

        public static StripeSubscription CreateSubscription(string customerId, string planId, string sourceId, decimal? platformFeePercent, DateTime? trialEndDate, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeSubscription subscription = null;

            var options = new StripeSubscriptionCreateOptions
            {
                CustomerId = customerId,
                Billing = StripeBilling.ChargeAutomatically,
                TrialEnd = trialEndDate,
                ApplicationFeePercent = platformFeePercent * 100, // convert to integer
                Source = sourceId
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                subscription = GetSubscriptionService().Create(customerId, planId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Stripe subscription was not created", MessageType.Error);
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
        public static StripeSubscription CreateSubscription(StripeCustomer customer, StripePlan plan, string sourceId, decimal? platformFeePercent, DateTime? trialEndDate, string stripeAccountId, SystemMessages msgs)
        {
            return CreateSubscription(customer.Id, plan.Id, sourceId, platformFeePercent, trialEndDate, stripeAccountId, msgs);
        }

        public static StripeSubscription UpdateSubscription(string subscriptionId, string newPlanId, bool? billingCycleAnchorNow, bool? billingCycleAnchorUnchanged, bool? endTrialNow, bool? prorate, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();


            var options = new StripeSubscriptionUpdateOptions();

            if (newPlanId.IsNotBlank())
            {
                options.Items = new List<StripeSubscriptionItemUpdateOption>()
                    {
                        new StripeSubscriptionItemUpdateOption()
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

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeSubscription subscription = null;

            try
            {
                subscription = GetSubscriptionService().Update(subscription.Id, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe subscription ({subscriptionId}) was not cancelled", MessageType.Error, ex);
            }

            return subscription;
        }

        public static SystemMessages CancelSubscription(string subscriptionId, string stripeAccountId)
        {
            var msgs = new SystemMessages();
            
            var options = new StripeSubscriptionUpdateOptions()
            {
                CancelAtPeriodEnd = true
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            try
            {
                GetSubscriptionService().Update(subscriptionId, options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe subscription ({subscriptionId}) was not cancelled", MessageType.Error, ex);
            }

            return msgs;
        }

        public static StripeSubscription GetSubscription(string subscriptionId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeSubscription subscription = null;

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

        private static StripeCardService GetCardService()
        {
            return new StripeCardService(StripeAccessToken);
        }

        public static StripeCard CreateCard(StripeCustomer customer, string token, SystemMessages msgs)
        {
            return CreateCard(customer.Id, token, msgs);
        }

        public static StripeCard CreateCard(string customerId, string token, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeCard card = null;

            try
            {
                var options = new StripeCardCreateOptions()
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

        public static List<StripeCard> GetCards(string customerId, SystemMessages msgs)
        {
            var cards = new List<StripeCard>();

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

        public static StripeCard GetCard(string customerId, string cardId, SystemMessages msgs)
        {
            var card = new StripeCard();

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

                if (!result.Deleted)
                    msgs.Add($"Stripe card ({cardId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe card ({cardId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public static SourceCard GetTestCard(StripeTestCardType stripeTestCardType)
        {
            return new SourceCard()
            {
                Name = "Bill Payer",
                Number = GetTestCardNumber(stripeTestCardType),
                ExpirationMonth = 1,
                ExpirationYear = DateTime.UtcNow.AddYears(2).Year,
                Cvc = "123"
            };
        }

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

        private static StripeSourceService GetSourceService()
        {
            return new StripeSourceService(StripeAccessToken);
        }

        public static StripeSource CreateSource(StripeCustomer customer, string token, SystemMessages msgs)
        {
            return CreateSource(token, msgs);
        }

        public static StripeSource CreateSource(string token, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeSource source = null;

            try
            {
                var options = new StripeSourceCreateOptions()
                {
                    Type = "card",
                    Token = token
                };

                source = GetSourceService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Stripe source was not created", MessageType.Error);
            }

            return source;
        }

        public static StripeSource CreateSourceFromOriginal(string customerId, string originalSourceId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripeSourceCreateOptions()
            {
                Customer = customerId,
                OriginalSource = originalSourceId,
                Usage = "reusable"
            };

            var requestOptions = new Stripe.StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeSource source = null;

            try
            {
                source = GetSourceService().Create(options, requestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.Add($"Stripe source was not created", MessageType.Error);
            }

            return source;
        }

        public static List<StripeSource> GetSources(string customerId, SystemMessages msgs)
        {
            var cards = new List<StripeSource>();

            try
            {
                cards.AddRange(GetSourceService().List(customerId).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add($"Cannot get list of sources", MessageType.Error, ex);
            }

            return cards;
        }

        public static StripeSource GetSource(string sourceId, SystemMessages msgs)
        {
            var card = new StripeSource();

            try
            {
                card = GetSourceService().Get(sourceId);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Source id ({sourceId}) not found", MessageType.Error, ex);
            }

            return card;
        }

        #endregion

        #region StripeCharge

        private static StripeChargeService GetChargeService()
        {
            return new StripeChargeService(StripeAccessToken);
        }        

        public static StripeCharge CreateCharge(string fromCustomerId, string destinationAccountId, string tokenOrSourceId, decimal totalAmount, decimal platformFee, string description, string statementDescriptor, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripeChargeCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Currency = "usd",
                Description = description,
                SourceTokenOrExistingSourceId = tokenOrSourceId,
                StatementDescriptor = statementDescriptor.MaxChars(22),
                CustomerId = fromCustomerId,
                // ApplicationFee = (platformFee * 100).ToInteger(),
                Destination = destinationAccountId,
                DestinationAmount = (totalAmount * 100).ToInteger() - (platformFee * 100).ToInteger()
            };

            StripeCharge charge = null;

            try
            {
                charge = GetChargeService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add("Error charging payment source", MessageType.Error, ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
            }

            return charge;
        }

        public static List<StripeCharge> GetCharges(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var charges = new List<StripeCharge>();
            var options = new StripeChargeListOptions
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
                msgs.Add("Error retrieving charges", MessageType.Error, ex);
            }

            return charges;
        }

        #endregion

        #region StripeTokens

        private static StripeTokenService GetTokenService()
        {
            return new StripeTokenService(StripeAccessToken);
        }

        public static StripeToken CreateCustomerToken(string customerId, string stripeAccountId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripeTokenCreateOptions()
            {
                CustomerId = customerId               
            };

            var requestOptions = new StripeRequestOptions();
            if (stripeAccountId.IsNotEmpty())
                requestOptions.StripeConnectAccountId = stripeAccountId;

            StripeToken token = null;

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

        public static StripeRefundService GetRefundService()
        {
            return new StripeRefundService(StripeAccessToken);
        }

        public static List<StripeRefund> GetRefunds(string chargeId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var refunds = new List<StripeRefund>();
            var options = new StripeRefundListOptions
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
}
