using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace AfterSchoolLibrary.Helpers
{
    public class ProratingCalculationHelper
    {
        private DateTime? AnchorDate { get; set; }
        private decimal? PlatformFeePercent { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        private bool IsProrated { get; set; }
        private decimal? PredictedStripeCharge { get; set; }
        private decimal? ProratedCCFee { get; set; }
        private decimal? ProratedAmountWithoutCCFee { get; set; }

        public ProratingCalculationHelper(CheckoutObject checkoutObject, Stripe.Customer customer, SystemMessages msgs, DatabaseConnection dc) 
        {
            IsProrated = false;
            AnchorDate = new DateTime?();
            StartDate = new DateTime?();
            EndDate = new DateTime?();
            CalculateProration(checkoutObject, customer, msgs, dc);
        }

        public DateTime GetAnchorDate()
        {
            return AnchorDate.Value;
        }

        public decimal GetPlatformFeePercent()
        {
            return PlatformFeePercent.Value;
        }

        public decimal GetPredictedStripeCharge()
        {
            return PredictedStripeCharge.Value;
        }

        public decimal GetProratedCCFee()
        {
            return ProratedCCFee.Value;
        }

        public decimal GetProratedAmountWithoutCCFee()
        {
            return ProratedAmountWithoutCCFee.Value;
        }

        public DateTime GetStartDate()
        {
            return StartDate.Value;
        }

        public DateTime GetEndDate()
        {
            return EndDate.Value;
        }

        public bool GetIsProrated()
        {
            return IsProrated;
        }

        private void CalculateProration(CheckoutObject checkoutObject, Stripe.Customer customer, SystemMessages msgs, DatabaseConnection dc)
        {
            var plan = checkoutObject.SubscriptionPlan;
            var subscriptionPlanType = plan.PlanTypeId;
            var participationObject = checkoutObject.ParticipationObject;
            var billingAccount = checkoutObject.BillingAccount;
            var useStripeConnectedAccount = billingAccount.HasAccountPackageFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

            // need to check if we need to apply pro-rating
            var isSubscription = subscriptionPlanType == SubscriptionPlanType.Subscription;
            var isOnCheckout = plan.PaymentStartTypeId == PlanPaymentStartType.OnCheckout;

            if (isSubscription && !isOnCheckout)
            {
                //loading variables
                // understand when first charge happens
                // if the user is checking out on a charge date
                // how many months are left in the subscription
                // TODO: Consider weekly subscriptions
                var firstChargeDateProviderTimeZone = (plan.PaymentStartTypeId == PlanPaymentStartType.OnPeriodStart ? plan.StartDate.Value : participationObject.StartDate.Value).Date;
                var lastActivityDateProviderTimeZone = participationObject.EndDate.Value.Date;

                var nowUtc = DateTime.UtcNow;
                var checkoutTimeProviderTimeZone = participationObject.OwnerAccount.ConvertToAccountTimeZone(nowUtc);
                var checkoutDateProviderTimeZone = checkoutTimeProviderTimeZone.Date;
                var isCheckoutAfterFirstChargeDate = checkoutDateProviderTimeZone.IsAfter(firstChargeDateProviderTimeZone);
                var lengthOfSubscriptionFromBeginning = 1; // at least one month if last activity date is after first charge date
                var chargeDates = new List<DateTime>();
                var lengthOfSubscriptionFromToday = 0;
                var isCheckoutOnAnyChargeDate = false;
                var nextChargeDate = new DateTime?();

                // get months between first charge and activity end date
                var isCurrentlyBeforeLastActivityDate = firstChargeDateProviderTimeZone.IsBefore(lastActivityDateProviderTimeZone);
                while (isCurrentlyBeforeLastActivityDate)
                {
                    if (firstChargeDateProviderTimeZone.AddMonths(lengthOfSubscriptionFromBeginning).IsOnOrBefore(lastActivityDateProviderTimeZone))
                        lengthOfSubscriptionFromBeginning++;
                    else
                        isCurrentlyBeforeLastActivityDate = false;
                }

                for (var i = 0; i < lengthOfSubscriptionFromBeginning; i++)
                    chargeDates.Add(firstChargeDateProviderTimeZone.AddMonths(i));

                isCheckoutOnAnyChargeDate = chargeDates.Any(chargeDate => chargeDate == checkoutDateProviderTimeZone);
                isCurrentlyBeforeLastActivityDate = checkoutDateProviderTimeZone.IsBefore(lastActivityDateProviderTimeZone);
                nextChargeDate = chargeDates.FirstOrDefault(chargeDate => chargeDate.IsAfter(checkoutDateProviderTimeZone));

                if (isCurrentlyBeforeLastActivityDate)
                {
                    lengthOfSubscriptionFromToday = 1; // at least one month then
                    while (isCurrentlyBeforeLastActivityDate)
                    {
                        if (nextChargeDate.Value.AddMonths(lengthOfSubscriptionFromToday).IsOnOrBefore(lastActivityDateProviderTimeZone))
                            lengthOfSubscriptionFromToday++;
                        else
                            isCurrentlyBeforeLastActivityDate = false;
                    }

                    EndDate = nextChargeDate.Value.AddMonths(lengthOfSubscriptionFromToday); // end the subscription exactly x months after activity start date (if it needs to end)
                }

                // needs prorating
                // time to prorate if the checkout date is after the billing start date
                // TODO: need to check if it is after the first month. If it's after the next billing date it should also prorate
                if (!isCheckoutOnAnyChargeDate && lengthOfSubscriptionFromToday > 0)
                {
                    var subscriptionStoredCost = plan.GetTotalCostWithFees(participationObject.OwnerAccount, msgs, dc);
                    AnchorDate = nextChargeDate; // anchor date has to be in the future. It should be on the same day as the period start or the activity start
                    StartDate = DateTime.UtcNow.AddSeconds(1); // start the subscription right now

                    PredictedStripeCharge = StripeHelper.CalculateStripeProratedCharge(AnchorDate.Value, checkoutTimeProviderTimeZone, subscriptionStoredCost);
                    ProratedCCFee = StripeHelper.CalculateStripeCCFeeOutOfTotalCost(PredictedStripeCharge.Value); // get our estimated CC fee (it will be wrong for a few reasons, we can discuss)
                    ProratedAmountWithoutCCFee = PredictedStripeCharge - ProratedCCFee; //how much is the "original" charge before cc fee

                    // get percent that subscription convenience fee should be
                    PlatformFeePercent = useStripeConnectedAccount
                    ? billingAccount.GetSubscriptionPlatformFeePercent(plan, checkoutObject.PaymentType, ProratedAmountWithoutCCFee.Value, msgs, dc)
                        : new decimal?();
                    

                    IsProrated = true;
                }
            }
        }

        public void ProcessCustomerBalanceTransactionAsCoupon(CheckoutObject checkoutObject, Stripe.Customer customer, SystemMessages msgs)
        {
            if (checkoutObject.PaymentType == PaymentType.ACH)
            {
                // coupons don't work when prorating
                // stripeCouponId = null;
                StripeHelper.GetStripeHelperConnectAccount().CreateCustomerBalanceTransaction(customer.Id, -ProratedCCFee.Value, "Adjustments for pro-rated subscription coupon", null, msgs);
            }

        }

    }
}
