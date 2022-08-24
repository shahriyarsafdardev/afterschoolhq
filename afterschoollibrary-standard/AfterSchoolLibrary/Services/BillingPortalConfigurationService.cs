using System;
using System.Collections.Generic;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using System.Linq;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Enums;
using BasicLibrary.DataAccess;
using Stripe.BillingPortal;

namespace AfterSchoolLibrary.Services
{
    public static class BillingPortalConfigurationService
    {
        public static BillingPortalConfiguration GetBillingPortalConfiguration(BillingPortalConfigurationType billingConfigType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var billingConfig = BillingPortalConfiguration.LoadByBillingPortalConfigurationTypeId(billingConfigType, msgs, dc);
            var stripeHelper = StripeHelper.GetStripeHelperSubscriberAccount();
            Stripe.BillingPortal.Configuration stripeBillingPortalConfig = null;
                    
            if (billingConfigType == BillingPortalConfigurationType.SubscriptionUpdate)
            {
                var packageOptionsList = new List<ConfigurationFeaturesSubscriptionUpdateProductOptions>();
                Package.LoadByIsActive(true, msgs).ForEach(package =>
                {
                    packageOptionsList.Add(new ConfigurationFeaturesSubscriptionUpdateProductOptions()
                    {
                        Product = package.Uid.ToString(),
                        Prices = new List<string>() { package.StripePricePerMonthId, package.StripePricePerYearId }
                    });
                });

                if (billingConfig.IsEmpty())
                    stripeBillingPortalConfig = stripeHelper.CreateBillingPortalConfigurationSubscriptionUpdate(packageOptionsList, msgs);
                else
                    stripeBillingPortalConfig =
                        stripeHelper.UpdateBillingPortalConfigurationSubscriptionUpdate(billingConfig.StripeId, packageOptionsList, msgs);
            }
            else if (billingConfigType == BillingPortalConfigurationType.PaymentMethodUpdate)
            {
                if (billingConfig.IsEmpty())
                    stripeBillingPortalConfig = stripeHelper.CreateBillingPortalConfigurationPaymentMethodUpdate(msgs);
                else
                    stripeBillingPortalConfig = stripeHelper.UpdateBillingPortalConfigurationPaymentMethodUpdate(billingConfig.StripeId, msgs);
            }

            if (billingConfig.IsEmpty() && stripeBillingPortalConfig.IsNotEmpty())
            {
                billingConfig = new BillingPortalConfiguration()
                {
                    StripeId = stripeBillingPortalConfig.Id,
                    BillingPortalConfigurationTypeId = billingConfigType,
                };
                msgs.AddRange(billingConfig.Save(dc));
            }

            return billingConfig;
        }
    }
}