using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Models
{
    public class CheckoutObject
    {
        public Registrant MasterRegistrant { get; set; }
        public Registration Registration { get; set; }
        public Coupon Coupon { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public SubscriptionPlan PaymentPlan { get; set; }
        public string StripeToken { get; set; }
        public string SourceId { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account PurchasingAccount { get; set; }
        public Account BillingAccount { get; set; }
        public int NumberOfSeats { get; set; }
        public List<Product> Products { get; set; }
        public bool SavePaymentInfo { get; set; }
        public PaymentType PaymentType { get; set; }
        /// <summary>
        /// override will sometimes be the manual payment or the subscription payment
        /// </summary>
        public decimal? OverrideAmount { get; set; }
        public string StripeChargeId { get; set; }
        public Purchase Purchase { get; set; }
        public PaymentSourceType PaymentSourceType { get; set; }
        public RegistrantSubscriptionPlan RegistrantSubscriptionPlan { get; set; }
        public Account ConvenienceFeePayer { get; set; }
        public Account CreditCardFeePayer { get; set; }

        public CheckoutObject()
        {
            Products = new List<Product>();
        }

        public bool IsPaying
        {
            get
            {
                return StripeToken.IsNotEmpty() || SourceId.IsNotEmpty();
            }
        }

        public bool IsFree
        {
            get
            {
                return ParticipationObject.IsFree;
            }
        }   
        
        public bool IsTuitionFree
        {
            get
            {
                return ParticipationObject.IsTuitionFree;
            }
        }

        public decimal TotalCostToPurchaseTodayNoFees
        {
            get
            {
                if (PaymentPlan.IsNotEmpty())
                    return PaymentPlanCostTodayNoFees + SubscriptionCostTodayNoFees;

                return TuitionCostNoFees + SubscriptionCostTodayNoFees;
            }
        }

        public decimal GetTotalCostToPurchaseTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);

            return GetTuitionCostWithFees(msgs, dc) + GetSubscriptionCostTodayWithFees(msgs, dc);
        }

        public decimal GetTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanTotalFeesToday(msgs, dc) + GetSubscriptionTotalFeesToday(msgs, dc);

            return GetTuitionTotalFees(msgs, dc) + GetSubscriptionTotalFeesToday(msgs, dc);
        }

        public decimal GetTotalConvenienceFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayConvenienceFee(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);

            return GetTuitionConvenienceFee(msgs, dc) + GetSubscriptionCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetTotalCreditCardFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentPlan.IsNotEmpty())
                return GetPaymentPlanCostTodayCreditCardFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);

            return GetTuitionCreditCardFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);
        }

        public decimal TuitionCostNoFees
        {
            get
            {
                if (ParticipationObject.IsNotEmpty() && ParticipationObject.Cost.HasValue)
                {
                    var total = ParticipationObject.Cost.Value * NumberOfSeats;

                    // if coupon applyTo is `TuitionFee`
                    // then apply coupon discount
                    if (Coupon.IsNotEmpty() && Coupon.ID.IsNotEmpty() && Coupon.DoesApplyToTuitionFee())
                    {
                        var couponAmount = Coupon.Amount.Value;
                        if (Coupon.HasPercentageAmountType())
                            couponAmount *=  total;
                        return total - couponAmount;
                    }
                    return total;
                }
                return 0;
            }
        }

        public decimal GetTuitionCostWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return TuitionCostNoFees + GetTuitionConvenienceFee(msgs, dc) +  GetTuitionCreditCardFee(msgs, dc);
        }     

        public decimal GetTuitionCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsTuitionFree && !GetUseStripeConnectedAccount(msgs, dc))
                return Billing.Platform.GetCreditCardFee(TuitionCostNoFees, BillingAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public decimal GetTuitionConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsTuitionFree && !GetUseStripeConnectedAccount(msgs, dc))
                return Billing.Platform.GetConvenienceFee(TuitionCostNoFees, BillingAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public decimal GetTuitionTotalFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetTuitionConvenienceFee(msgs, dc) + GetTuitionCreditCardFee(msgs, dc);
        }

        public decimal SubscriptionCostTodayNoFees
        {
            get
            {
                return SubscriptionPlan.IsNotEmpty() ? SubscriptionPlan.CostPerPeriod.Value * NumberOfSeats : 0;
            }
        }

        public decimal GetSubscriptionCostTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionCostTodayNoFees +
                    GetSubscriptionCostTodayCreditCardFee(msgs, dc) +
                    GetSubscriptionCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetSubscriptionCostTodayCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!GetUseStripeConnectedAccount(msgs, dc))
                return Billing.Platform.GetCreditCardFee(SubscriptionCostTodayNoFees, BillingAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public decimal GetSubscriptionCostTodayConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!GetUseStripeConnectedAccount(msgs, dc))
                return Billing.Platform.GetConvenienceFee(SubscriptionCostTodayNoFees, BillingAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public decimal GetSubscriptionTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetSubscriptionCostTodayConvenienceFee(msgs, dc) + GetSubscriptionCostTodayCreditCardFee(msgs, dc);
        }

        public decimal PaymentPlanCostTodayNoFees
        {
            get
            {
                return PaymentPlan.IsNotEmpty() ? PaymentPlan.CostPerPeriod.Value * NumberOfSeats : 0;
            }
        }

        public decimal GetPaymentPlanCostTodayWithFees(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentPlanCostTodayNoFees +
                    GetPaymentPlanCostTodayCreditCardFee(msgs, dc) +
                    GetPaymentPlanCostTodayConvenienceFee(msgs, dc);
        }

        public decimal GetPaymentPlanCostTodayCreditCardFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!GetUseStripeConnectedAccount(msgs, dc))
                return Billing.Platform.GetCreditCardFee(PaymentPlanCostTodayNoFees, BillingAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public decimal GetPaymentPlanCostTodayConvenienceFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!GetUseStripeConnectedAccount(msgs, dc))
                return Billing.Platform.GetConvenienceFee(PaymentPlanCostTodayNoFees, BillingAccount.IsFeeTakenOutOfCost);

            return 0;
        }

        public decimal GetPaymentPlanTotalFeesToday(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetPaymentPlanCostTodayConvenienceFee(msgs, dc) + GetPaymentPlanCostTodayCreditCardFee(msgs, dc);
        }        

        public decimal ApplyCoupon(decimal total)
        {
            if (Coupon.IsNotEmpty() && Coupon.Amount.HasValue)
            {
                if (Coupon.AmountTypeId == CouponAmountType.FlatAmount)
                    return Math.Max(0, total - Coupon.Amount.Value);
                else if (Coupon.AmountTypeId == CouponAmountType.Percentage)
                    return Math.Max(0, total - (total * Coupon.Amount.Value));
            }

            return total;
        }

        public decimal GetPlatformPercentFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (PaymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                return BillingAccount.GetPlatformPercentFee(msgs, dc);

            return 0;
        }

        public decimal GetPlatformFlatFee(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!IsTuitionFree && PaymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                return BillingAccount.GetPlatformFlatFee(msgs, dc);

            return 0;
        }

        public Purchase CreateOrUpdatePurchase(WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.Purchase.IsEmpty())
                this.Purchase = this.Registration.GetPurchase(msgs, dc);

            if (this.Purchase.IsEmpty())
            {
                this.Purchase = AccountService.AddPurchase(this, loggedInUser, msgs, dc);
                this.Registration.PurchaseId = this.Purchase.ID;
                msgs.AddRange(this.Registration.Save(dc));
            }

            return this.Purchase;
        }

        private bool? _UseStripeConnectedAccount = null;
        private bool GetUseStripeConnectedAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!_UseStripeConnectedAccount.HasValue)
                _UseStripeConnectedAccount = BillingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

            return _UseStripeConnectedAccount.Value;
        }

    }
}
