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

        public static AccountPackage CreateNewAccountPackage(int accountId, Package package, BillingPeriodType billingPeriodType, string stripeSubscriptionId, DateTime? startDate, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                StartDate = startDate
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
                    trialEndDate: DateTime.UtcNow.Date.AddDays(trialDays),
                    prorate: null,
                    couponId: null,
                    msgs: msgs);
        }
        
        public static void UpdateTrialDays(this AccountPackage accountPackage, int trialDays, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (!accountPackage.TrialStartDate.HasValue)
                accountPackage.StartTrialNow(msgs, dc);
                
            accountPackage.TrialEndDate = DateTime.UtcNow.Date.AddDays(trialDays);
            msgs.AddRange(accountPackage.Save(dc));
        }

        public static void StartTrialNow(this AccountPackage accountPackage, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.TrialStartDate = DateTime.UtcNow;
            msgs.AddRange(accountPackage.Save(dc));
        }

        public static void EndTrialNow(this AccountPackage accountPackage, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.TrialEndDate = DateTime.UtcNow;
            accountPackage.StartDate = DateTime.UtcNow;
            msgs.AddRange(accountPackage.Save(dc));
        }

        public static void SyncTrialDates(this AccountPackage accountPackage, DateTime? trialStartDate, DateTime? trialEndDate, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.TrialStartDate = trialStartDate;
            accountPackage.TrialEndDate = trialEndDate;
            msgs.AddRange(accountPackage.Save(dc));
        }

        public static void ContinueTrialFromPreviousPackage(this AccountPackage accountPackage, AccountPackage previousPackage, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.TrialStartDate = previousPackage.TrialStartDate;
            accountPackage.TrialEndDate = previousPackage.TrialEndDate;
            
            msgs.AddRange(accountPackage.Save(dc));
            
            accountPackage.AddOrUpdatePackageFeature(
                FeatureType.TrialDays, 
                previousPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue(), 
                loggedInUser, 
                msgs,
                dc);
        }

        public static DateTime? GetTrialEndDate(this AccountPackage accountPackage, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (accountPackage.TrialEndDate.HasValue)
                return accountPackage.TrialEndDate.Value;

            if (accountPackage.TrialStartDate.HasValue)
                return accountPackage.TrialStartDate.Value.AddDays((double) accountPackage.GetFeature(FeatureType.TrialDays, msgs, dc).GetMaxValue());

            return new DateTime?();
        }

        public static void UpdateStripeSubscriptionId(this AccountPackage accountPackage, string stripeSubscriptionId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            accountPackage.StripeSubscriptionId = stripeSubscriptionId;
            msgs.AddRange(accountPackage.Save(dc));
        }

        public static string GetStripePriceId(this AccountPackage accountPackage, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var package = accountPackage.GetPackage(msgs, dc);
            
            if (package.IsNotEmpty())
                return accountPackage.BillingPeriodTypeId == BillingPeriodType.Month ? package.StripePricePerMonthId : package.StripePricePerYearId;

            return null;
        }

        public static DateTime? GetFirstRenewalDate(this AccountPackage accountPackage)
        {
            if (accountPackage.StartDate.HasValue)
            {
                if (accountPackage.BillingPeriodTypeId == BillingPeriodType.Month)
                    return accountPackage.StartDate.Value.AddMonths(1);
                else if (accountPackage.BillingPeriodTypeId == BillingPeriodType.Year)
                    return accountPackage.StartDate.Value.AddYears(1);
            }
            else if (accountPackage.TrialEndDate.HasValue)
                return accountPackage.TrialEndDate.Value;

            return null;
        }
    }
}