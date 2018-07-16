using System;
using BasicLibrary;
using Stripe;
using System.Collections.Generic;
using System.Linq;

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
        private static string PublishableKey
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

        private StripeRequestOptions RequestOptions { get; set; }

        private string StripeAccessToken { get; set; }
        
        public StripeHelper()
        {
            StripeAccessToken = StripeHelper.SecretKey;
            RequestOptions = new StripeRequestOptions
            {
                ApiKey = StripeAccessToken
            };
        }

        public StripeHelper(string stripeAccessToken)
        {
            StripeAccessToken = stripeAccessToken;
            RequestOptions = new StripeRequestOptions
            {
                ApiKey = StripeAccessToken
            };
        }

        #region StripeAccounts

        //public StripeAccountService GetAccountService()
        //{
        //    return new StripeAccountService(StripeAccessToken);
        //}

        //public StripeAccount CreateAccount(SystemMessages msgs)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();

        //    var options = new StripeAccountCreateOptions()
        //    {
        //        BusinessName = "",
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
        //        {
        //            AccountHolderName = "",
        //            AccountHolderType = "",
        //            AccountNumber = "",
        //            Country = "US",
        //            Currency = "usd",
        //            RoutingNumber = ""
        //        },
        //        LegalEntity = new StripeAccountLegalEntityOptions()
        //        {
        //            AddressCity = "",
        //            AddressLine1 = "",
        //            AddressLine2 = "",
        //             AddressPostalCode = "",
        //             AddressState = "",
        //             AddressTwoLetterCountry = "US",
        //             BusinessName = "",
        //             BusinessTaxId = "",
        //             BirthDay = "",
        //             BirthMonth = "",
        //             BirthYear = "",
        //              FirstName = "",
        //              LastName = "",
        //              PersonalAddressCity = "",
        //              PersonalAddressLine1 = "",
        //              PersonalAddressLine2 = "",
        //              PersonalAddressState = "",
        //              PersonalAddressPostalCode = "",
        //               PersonalAddressTwoLetterCountry = "US",
        //               PersonalIdNumber = "",
        //               PhoneNumber = "",
        //               SSNLast4 = "",

        //        },
                
        //    };

        //    try
        //    {

        //    }
        //    catch(StripeException ex)
        //    {
                
        //    }
        //}

        #endregion

        #region StripeProducts

        private StripeProductService GetProductService()
        {
            return new StripeProductService(StripeAccessToken);
        }

        public StripeProduct CreateProduct(string productId, string productName, string statementDescriptor, bool active, SystemMessages msgs)
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

            try
            {
                product = GetProductService().Create(options, RequestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe product was not created", MessageType.Error, ex);
            }

            return product;
        }

        public SystemMessages UpdateProduct(string productId, string productName, string statementDescriptor, bool active)
        {
            var msgs = new SystemMessages();
            var product = GetProduct(productId, msgs);

            if (product.IsNotEmpty())
            {
                var options = new StripeProductUpdateOptions
                {
                    Name = productName,
                    StatementDescriptor = statementDescriptor.MaxChars(22),
                    Active = active
                };

                try
                {
                    GetProductService().Update(product.Id, options, RequestOptions);
                }
                catch (StripeException ex)
                {
                    msgs.Add($"Stripe product ({product.Id}) was not updated", MessageType.Error, ex);
                }
            }
            else
            {
                msgs.Add($"No stripe product found with ID: {productId}", MessageType.Notice);
            }

            return msgs;
        }

        public SystemMessages DeleteProduct(string productId)
        {
            var msgs = new SystemMessages();

            try
            {
                var result = GetProductService().Delete(productId);

                if (!result.Deleted)
                    msgs.Add($"Stripe product ({productId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe product ({productId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public StripeProduct GetProduct(string productId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            StripeProduct product = null;

            try
            {
                product = GetProductService().Get(productId, RequestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe product found with ID: {productId}", MessageType.Notice, ex);
            }

            return product;
        }

        #endregion

        #region StripePlans

        private StripePlanService GetPlanService()
        {
            return new StripePlanService(StripeAccessToken);
        }

        public StripePlan CreatePlan(StripeProduct stripeProduct, string planId, decimal price, BillingPeriodType billingPeriodType, string planNickName, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripePlan plan = null;
            var options = new StripePlanCreateOptions
            {
                Id = planId,
                Amount = Convert.ToInt32(price * 100),
                Currency = "usd",
                IntervalCount = 1,
                Interval = billingPeriodType.ToString().ToLower(),
                Nickname = planNickName,
                ProductId = stripeProduct.Id
            };

            try
            {
                plan = GetPlanService().Create(options, RequestOptions);
            }
            catch(StripeException ex)
            {
                msgs.Add($"Stripe plan was not created", MessageType.Error, ex);
            }

            return plan;
        }

        public SystemMessages UpdatePlan(string planId, string planNickName, bool active)
        {
            var msgs = new SystemMessages();
            var plan = GetPlan(planId, msgs);

            if (plan.IsNotEmpty())
            {
                var options = new StripePlanUpdateOptions
                {
                    Active = active,
                    Nickname = planNickName
                };

                try
                {
                    GetPlanService().Update(plan.Id, options, RequestOptions);
                }
                catch (StripeException ex)
                {
                    msgs.Add($"Stripe plan ({plan.Id}) was not updated", MessageType.Error, ex);
                }
            }
            else
            {
                msgs.Add($"No stripe plan found with ID: {planId}", MessageType.Notice);
            }

            return msgs;
        }

        public SystemMessages DeletePlan(string planId)
        {
            var msgs = new SystemMessages();

            try
            {
                var result = GetPlanService().Delete(planId);

                if (!result.Deleted)
                    msgs.Add($"Stripe plan ({planId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe plan ({planId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public StripePlan GetPlan(string planId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            StripePlan plan = null;

            try
            {
                plan = GetPlanService().Get(planId, RequestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe plan found with ID: {planId}", MessageType.Notice, ex);
            }

            return plan;
        }

        #endregion

        #region StripeCustomers

        private StripeCustomerService GetCustomerService()
        {
            return new StripeCustomerService(StripeAccessToken);
        }

        public StripeCustomer CreateCustomer(string description, string email, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeCustomer customer = null;
            var options = new StripeCustomerCreateOptions
            {
                Description = description,
                Email = email
            };

            try
            {
                customer = GetCustomerService().Create(options);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer was not created", MessageType.Error, ex);
            }

            return customer;
        }

        public SystemMessages UpdateCustomerDefaultCard(string customerId, string cardId)
        {
            var msgs = new SystemMessages();
            var options = new StripeCustomerUpdateOptions
            {
                DefaultSource = cardId
            };

            try
            {
                GetCustomerService().Update(customerId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not updated", MessageType.Error, ex);
            }

            return msgs;
        }

        public SystemMessages UpdateCustomer(string customerId, string description, string email)
        {
            var msgs = new SystemMessages();
            
            var options = new StripeCustomerUpdateOptions
            {
                Description = description,
                Email = email
            };

            try
            {
                GetCustomerService().Update(customerId, options);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not updated", MessageType.Error, ex);
            }

            return msgs;
        }

        public SystemMessages DeleteCustomer(string customerId)
        {
            var msgs = new SystemMessages();

            try
            {
                var result = GetCustomerService().Delete(customerId);

                if (!result.Deleted)
                    msgs.Add($"Stripe customer ({customerId}) was not deleted", MessageType.Error);
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe customer ({customerId}) was not deleted", MessageType.Error, ex);
            }

            return msgs;
        }

        public StripeCustomer GetCustomer(string customerId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeCustomer customer = null;

            try
            {
                customer = GetCustomerService().Get(customerId, RequestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe customer found with ID: {customerId}", MessageType.Notice, ex);
            }

            return customer;
        }

        #endregion

        #region StripeSubscriptions

        private StripeSubscriptionService GetSubscriptionService()
        {
            return new StripeSubscriptionService(StripeAccessToken);
        }

        public StripeSubscription CreateSubscription(string customerId, string planId, DateTime? trialEndDate, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeSubscription subscription = null;

            var options = new StripeSubscriptionCreateOptions
            {
                CustomerId = customerId,
                Billing = StripeBilling.ChargeAutomatically,
                TrialEnd = trialEndDate
            };

            // not using this in sdk even though it's recommended
            // because the planid is passed in the create below
            //Items = new List<StripeSubscriptionItemOption>()
            //{
            //    new StripeSubscriptionItemOption()
            //    {
            //        PlanId = planId,
            //        Quantity = 1
            //    }
            //}

            try
            {
                subscription = GetSubscriptionService().Create(customerId, planId, options, RequestOptions);
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
        public StripeSubscription CreateSubscription(StripeCustomer customer, StripePlan plan, DateTime? trialEndDate, SystemMessages msgs)
        {
            return CreateSubscription(customer.Id, plan.Id, trialEndDate, msgs);
        }

        public SystemMessages UpdateSubscription(string subscriptionId, string newPlanId, bool? billingCycleAnchorNow, bool? billingCycleAnchorUnchanged, bool? endTrialNow, bool? prorate)
        {
            var msgs = new SystemMessages();
            var subscription = GetSubscription(subscriptionId, msgs);

            if (subscription.IsNotEmpty())
            {
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

                try
                {
                    GetSubscriptionService().Update(subscription.Id, options, RequestOptions);
                }
                catch (StripeException ex)
                {
                    msgs.Add($"Stripe subscription ({subscription.Id}) was not cancelled", MessageType.Error, ex);
                }
            }
            else
            {
                msgs.Add($"No stripe subscription found with ID: {subscriptionId}", MessageType.Notice);
            }

            return msgs;
        }

        public SystemMessages CancelSubscription(string subscriptionId)
        {
            var msgs = new SystemMessages();
            var subscription = GetSubscription(subscriptionId, msgs);

            if (subscription.IsNotEmpty())
            {
                var options = new StripeSubscriptionUpdateOptions()
                {
                    CancelAtPeriodEnd = true
                };

                try
                {
                    GetSubscriptionService().Update(subscription.Id, options, RequestOptions);
                }
                catch (StripeException ex)
                {
                    msgs.Add($"Stripe subscription ({subscription.Id}) was not cancelled", MessageType.Error, ex);
                }
            }
            else
            {
                msgs.Add($"No stripe subscription found with ID: {subscriptionId}", MessageType.Notice);
            }

            return msgs;
        }

        public StripeSubscription GetSubscription(string subscriptionId, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeSubscription subscription = null;

            try
            {
                subscription = GetSubscriptionService().Get(subscriptionId, RequestOptions);
            }
            catch (StripeException ex)
            {
                msgs.Add($"No stripe subscription found with ID: {subscriptionId}", MessageType.Notice, ex);
            }

            return subscription;
        }

        #endregion

        #region StripeCard

        public StripeCardService GetCardService()
        {
            return new StripeCardService(StripeAccessToken);
        }

        public StripeCard CreateCard(StripeCustomer customer, SourceCard sourceCard, SystemMessages msgs)
        {
            return CreateCard(customer.Id, sourceCard, msgs);
        }
        public StripeCard CreateCard(string customerId, SourceCard sourceCard, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            StripeCard card = null;

            try
            {
                var options = new StripeCardCreateOptions()
                {
                   SourceCard = sourceCard
                };

                card = GetCardService().Create(customerId, options);
            }
            catch(StripeException ex)
            {
                msgs.Add($"Stripe source card was not created", MessageType.Error, ex);
            }

            return card;
        }

        public List<StripeCard> GetCards(string customerId, SystemMessages msgs)
        {
            var cards = new List<StripeCard>();

            try
            {
                cards.AddRange(GetCardService().List(customerId).ToList());
            }
            catch (StripeException ex)
            {
                msgs.Add($"Stripe source card was not created", MessageType.Error, ex);
            }

            return cards;
        }

        public SystemMessages DeleteCard(string customerId, string cardId)
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

        public SourceCard GetTestCard(StripeTestCardType stripeTestCardType)
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

        private string GetTestCardNumber(StripeTestCardType stripeTestCardType)
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

        #region StripeCharge

        public StripeChargeService GetChargeService()
        {
            return new StripeChargeService(StripeAccessToken);
        }

        public StripeCharge CreateChargeWithSource(decimal totalAmount, string description, Account destinationAccount, SourceCard sourceCard, string statementDescriptor, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var options = new StripeChargeCreateOptions
            {
                Amount = (totalAmount * 100).ToInteger(),
                Currency = "usd",
                Description = description,
                Destination = destinationAccount.StripeUserId,
                SourceCard = sourceCard,
                StatementDescriptor = statementDescriptor
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

        public List<StripeCharge> GetCharges(string customerId, SystemMessages msgs)
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

        #region StripeRefunds

        public StripeRefundService GetRefundService()
        {
            return new StripeRefundService(StripeAccessToken);
        }

        public List<StripeRefund> GetRefunds(string chargeId, SystemMessages msgs)
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
