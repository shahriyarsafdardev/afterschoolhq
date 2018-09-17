using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary
{
    public class CheckoutObject
    {
        public Registrant Registrant { get; set; }
        public Registration Registration { get; set; }
        public Coupon Coupon { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public SubscriptionPlan PaymentPlan { get; set; }
        public string StripeToken { get; set; }
        public string SourceId { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account Purchaser { get; set; }
        public int NumberOfSeats { get; set; }
        public List<Product> Products { get; set; }
        public bool SavePaymentInfo { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal? OverrideAmount { get; set; }

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
                return ParticipationObject.IsNotEmpty() && ParticipationObject.Cost.HasValue ? ParticipationObject.Cost.Value * NumberOfSeats : 0;
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
                if (Coupon.AmountTypeId == AmountType.FlatAmount)
                    return Math.Max(0, total - Coupon.Amount.Value);
                else if (Coupon.AmountTypeId == AmountType.Percentage)
                    return Math.Max(0, total - (total * Coupon.Amount.Value));
            }

            return total;
        }

    }
}
