using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary
{
    public class PurchaseObject
    {
        public Registration Registration { get; set; }
        public Coupon Coupon { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public PaymentPlan PaymentPlan { get; set; }
        public string StripeTokenOrCardId { get; set; }
        public ParticipationObject ParticipationObject { get; set; }
        public Account Purchaser { get; set; }
        public WebUser LoggedInUser { get; set; }
        public int NumberOfSeats { get; set; }
        public List<Product> Products { get; set; }

        public PurchaseObject()
        {
            Products = new List<Product>();
        }

        public decimal TotalCostToPurchase
        {
            get
            {
                return RegistrationFee + SubscriptionCostToday + PaymentPlanCostToday + ProductCost;
            }
        }

        public decimal RegistrationFee
        {
            get
            {
                return ParticipationObject.IsNotEmpty() ? ParticipationObject.Cost.Value * NumberOfSeats : 0;
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

        public decimal ProductCost
        {
            get
            {
                return Products.Sum(Product => Product.Cost.IfEmpty(0));
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
