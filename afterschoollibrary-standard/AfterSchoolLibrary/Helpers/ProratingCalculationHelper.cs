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
        private decimal? ProratedConvenienceFee { get; set; }
        private decimal? ProratedAmountWithoutCCFee { get; set; }
        private decimal? ProratedAmountWithoutFees { get; set; }

        public ProratingCalculationHelper(SubscriptionPlan plan, ParticipationObject participationObject, Models.Account billingAccount, PaymentType paymentType, SystemMessages msgs, DatabaseConnection dc = null) 
        {
            IsProrated = false;
            AnchorDate = new DateTime?();
            StartDate = new DateTime?();
            EndDate = new DateTime?();
            PlatformFeePercent = new decimal?();
            PredictedStripeCharge = new decimal?();
            ProratedCCFee = new decimal?();
            ProratedConvenienceFee = new decimal?();
            ProratedAmountWithoutCCFee = new decimal?();
            ProratedAmountWithoutFees = new decimal?();
            CalculateProration(plan, participationObject, billingAccount, paymentType, msgs, dc);
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

        public decimal GetProratedConvenienceFee()
        {
            return ProratedConvenienceFee.Value;
        }

        public decimal GetProratedAmountWithoutFees()
        {
            return ProratedAmountWithoutFees.Value;
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

        private void CalculateProration(SubscriptionPlan plan, ParticipationObject participationObject, Models.Account billingAccount, PaymentType paymentType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var subscriptionPlanType = plan.PlanTypeId;
            var useStripeConnectedAccount = billingAccount.HasAccountPackageFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

            // need to check if we need to apply pro-rating
            var isSubscription = subscriptionPlanType == SubscriptionPlanType.Subscription;
            var isOnCheckout = plan.PaymentStartTypeId == PlanPaymentStartType.OnCheckout;

            if (isSubscription && !isOnCheckout)
            {
                //loading variables
                // understand when first charge happens
                // if the user is checking out on a charge date
                // how many periods are left in the subscription
                // TODO: Consider weekly subscriptions
                var firstChargeDateProviderTimeZone = plan.GetFirstChargeDateProviderTimeZone(participationObject);
                var lastActivityDateProviderTimeZone = participationObject.EndDate.Value.Date;

                var checkoutTimeUtc = DateTime.UtcNow;
                // var checkOutDateUtc = checkoutTimeUtc.Date;
                var checkoutTimeProviderTimeZone = participationObject.OwnerAccount.ConvertToAccountTimeZone(checkoutTimeUtc);
                var checkoutDateProviderTimeZone = checkoutTimeProviderTimeZone.Date;
                var chargeDates = new List<DateTime>();
                var isCheckoutOnAnyChargeDate = false;
                var nextChargeDate = new DateTime?();

                var lengthOfSubscriptionFromBeginning = plan.NumberOfPeriodsForSubscription(firstChargeDateProviderTimeZone.Value, lastActivityDateProviderTimeZone);                

                for (var i = 0; i < lengthOfSubscriptionFromBeginning; i++)
                    chargeDates.Add(firstChargeDateProviderTimeZone.Value.AddMonths(i));

                isCheckoutOnAnyChargeDate = chargeDates.Any(chargeDate => chargeDate == checkoutDateProviderTimeZone);
                nextChargeDate = chargeDates.FirstOrDefault(chargeDate => chargeDate.IsAfter(checkoutDateProviderTimeZone));               
                var lengthOfSubscriptionFromToday = plan.NumberOfPeriodsForSubscription(checkoutDateProviderTimeZone, lastActivityDateProviderTimeZone);

                if (nextChargeDate.HasValue)
                    EndDate = plan.CalculateEndDateFromNumberOfPeriods(nextChargeDate.Value, lengthOfSubscriptionFromToday);

                // needs prorating
                // time to prorate if the checkout date is after the billing start date
                if (!isCheckoutOnAnyChargeDate && lengthOfSubscriptionFromToday > 0 && nextChargeDate != chargeDates.First())
                {
                    var subscriptionStoredCostCC = plan.GetTotalCostWithFees(participationObject.OwnerAccount, msgs, dc);
                    var subscriptionStoredCostACH = plan.GetTotalCostWithConvenienceFee(participationObject.OwnerAccount, msgs, dc);
                    AnchorDate = nextChargeDate; // anchor date has to be in the future. It should be on the same day as the period start or the activity start
                    StartDate = DateTime.UtcNow.AddSeconds(1); // start the subscription right now

                    if (paymentType == PaymentType.ACH)
                    {
                        PredictedStripeCharge = StripeHelper.CalculateStripeProratedCharge(AnchorDate.Value, checkoutTimeProviderTimeZone, subscriptionStoredCostACH);
                        ProratedCCFee = 0;
                    }
                    else
                    {
                        PredictedStripeCharge = StripeHelper.CalculateStripeProratedCharge(AnchorDate.Value, checkoutTimeProviderTimeZone, subscriptionStoredCostCC);
                        ProratedCCFee = StripeHelper.CalculateStripeCCFeeOutOfTotalCost(PredictedStripeCharge.Value); // get our estimated CC fee (it will be wrong for a few reasons, we can discuss)
                    }

                    ProratedAmountWithoutCCFee = PredictedStripeCharge - ProratedCCFee; //how much is the "original" charge before cc fee
                    ProratedConvenienceFee = new FeeCalculationObject(ProratedAmountWithoutCCFee.Value, billingAccount, plan.IsConvenienceFeeTakenOutOfCost, msgs, dc).GetConvenienceFee();
                    ProratedAmountWithoutFees = ProratedAmountWithoutCCFee - ProratedConvenienceFee;

                    // get percent that subscription convenience fee should be
                    PlatformFeePercent = useStripeConnectedAccount ?
                                            billingAccount.GetSubscriptionPlatformFeePercent(plan, paymentType, ProratedAmountWithoutFees.Value, msgs, dc)
                                            : new decimal?();
                    

                    IsProrated = true;
                }
            }
        }

        public void ProcessCustomerBalanceTransactionAsCoupon(PaymentType paymentType, Stripe.Customer customer, SystemMessages msgs)
        {
            if (paymentType == PaymentType.ACH)
                StripeHelper.GetStripeHelperConnectAccount().CreateCustomerBalanceTransaction(customer.Id, -ProratedCCFee.Value, "Adjustments for pro-rated subscription coupon", null, msgs);
        }

    }
}
