using System;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AccountPackageService
    {
        public static Account GetAccount(this AccountPackage accountPackage, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (accountPackage.IsNotEmpty())
                return Account.Load(accountPackage.AccountId.Value, msgs, dc);

            return null;
        }

        public static void RemoveFeature(this AccountPackage accountPackage, FeatureType featureType,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (accountPackage.IsNotEmpty())
            {
                var apf = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value,
                    featureType, msgs, dc);

                if (apf.IsNotEmpty())
                    msgs.AddRange(apf.Delete(dc));
            }
        }

        public static Stripe.Subscription GetStripeSubscription(this AccountPackage accountPackage, SystemMessages msgs = null)
        {
            if (accountPackage.StripeSubscriptionId.IsNotEmpty())
                return StripeHelper.GetStripeHelperSubscriberAccount().GetSubscription(accountPackage.StripeSubscriptionId, msgs);

            return null;
        }

        public static AccountPackage CreateNewAccountPackage(int accountId, Package package, BillingPeriodType billingPeriodType, string stripeSubscriptionId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var accountPackage = new AccountPackage
            {
                AccountId = accountId,
                PackageId = package.ID,
                BillingPeriodTypeId = billingPeriodType,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                IsActive = true,
                Price = billingPeriodType == BillingPeriodType.Month
                    ? package.Price
                    : package.PricePerYear,
                StripeSubscriptionId = stripeSubscriptionId,
                StartDate = DateTime.UtcNow
            };

            msgs.AddRange(accountPackage.Save(dc));
            
            package.CopyFeaturesToAccountPackage(accountPackage, loggedInUser, msgs, dc);

            return accountPackage;
        }

        public static void UpdateTrialDaysWithStripe(this AccountPackage accountPackage, int trialDays, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.UpdateTrialDays(trialDays, msgs, dc);

            if (msgs.HasNoErrors())
                StripeHelper.GetStripeHelperSubscriberAccount().UpdateSubscription(
                    subscriptionId: accountPackage.StripeSubscriptionId,
                    priceId: null,
                    billingCycleAnchorNow: null,
                    billingCycleAnchorUnchanged: null,
                    trialEndDate: accountPackage.TrialStartDate.Value.AddDays(trialDays),
                    prorate: null,
                    msgs: msgs);
        }
        
        public static void UpdateTrialDays(this AccountPackage accountPackage, int trialDays, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.TrialEndDate = accountPackage.TrialStartDate.Value.AddDays(trialDays);
            msgs.AddRange(accountPackage.Save(dc));
        }
    }
}