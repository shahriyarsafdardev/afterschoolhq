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

        public decimal TotalCostToPurchaseToday
        {
            get
            {
                if (PaymentPlan.IsNotEmpty())
                    return PaymentPlanCostToday + SubscriptionCostToday;

                return TuitionFee + SubscriptionCostToday;
            }
        }

        public decimal TuitionFee
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

        public decimal SubscriptionCostToday
        {
            get
            {
                return SubscriptionPlan.IsNotEmpty() ? SubscriptionPlan.CostPerPeriod.Value * NumberOfSeats : 0;
            }
        }

        public decimal PaymentPlanCostToday
        {
            get
            {
                return PaymentPlan.IsNotEmpty() ? PaymentPlan.CostPerPeriod.Value * NumberOfSeats : 0;
            }
        }

        public decimal PaymentPlanTotalCost
        {
            get
            {
                return PaymentPlan.IsNotEmpty() ? PaymentPlan.CostPerPeriod.Value * PaymentPlan.TotalPeriods.Value * NumberOfSeats : 0;
            }
        }

        public decimal ProductCost
        {
            get
            {
                return Products.Any() ? Products.Sum(Product => Product.Cost.IfEmpty(0)) : 0;
            }
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
            if (!IsTuitionFree && RegistrantSubscriptionPlan.IsEmpty() && PaymentSourceType == PaymentSourceType.AfterSchoolPlatform)
                return BillingAccount.GetPlatformFlatFee(msgs, dc);

            return 0;
        }

        public void CreateOrUpdatePurchase(WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.Purchase.IsEmpty())
                this.Purchase = this.Registration.GetPurchase(msgs, dc);

            if (this.Purchase.IsEmpty())
            {
                this.Purchase = AccountService.AddPurchase(this, loggedInUser, msgs, dc);
                this.Registration.PurchaseId = this.Purchase.ID;
                msgs.AddRange(this.Registration.Save(dc));
            }
        }

    }
}
