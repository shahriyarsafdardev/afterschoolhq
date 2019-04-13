using System;
using System.Collections.Generic;
using System.Text;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Billing;

namespace AfterSchoolLibrary.Services
{
    public static class CouponService
    {
        public static Coupon CreateNew(int accountId, decimal amount, DateTime? expirationDate, CouponAmountType couponAmountType, CouponApplyType couponApplyType, int? maxUsesAll, int? maxUsesPerPerson, string couponCode, string label, string description, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var coupon = new Coupon()
            {
                AccountId = accountId,
                Amount = amount,
                AmountTypeId = couponAmountType,
                CouponCode = couponCode,
                AppliesTo = couponApplyType,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID,
                Description = description,
                ExpirationDate = expirationDate,
                MaxNumberOfUsesAll = maxUsesAll,
                MaxNumberOfUsesPerPerson = maxUsesPerPerson,
                Name = label
            };

            msgs.AddRange(coupon.Save(dc));

            return coupon;
        }

        public static string GenerateCouponCode(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var code = Guid.NewGuid().ToString("n").Substring(0, 8);
            var coupon = Coupon.LoadByAccountIdByCouponCode(accountId, code, msgs, dc);

            if (coupon.IsEmpty())
                return code;

            return GenerateCouponCode(accountId, msgs, dc);
        }

        public static Account GetAccount(this Coupon coupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(coupon.AccountId.Value, msgs, dc);
        }

        public static List<PurchaseableObjectCoupon> GetPurchaseableObjectCoupons(this Coupon coupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PurchaseableObjectCoupon.LoadByCouponId(coupon.ID.Value, msgs, dc);
        }

        public static string GetAmountAsString(this Coupon coupon)
        {
            if (coupon.AmountTypeId == CouponAmountType.Percentage)
                return String.Format("{0:0.00}%", (coupon.Amount * 100).Value);

            return coupon.Amount.ToCurrency();
        }

        public static bool DoesApplyToTuitionFee(this Coupon coupon)
        {
            return coupon.AppliesTo == CouponApplyType.TuitionFee;
        }

        public static bool DoesApplyToSubscriptionPlan(this Coupon coupon)
        {
            return coupon.AppliesTo == CouponApplyType.SubscriptionPlan;
        }

        public static bool HasPercentageAmountType(this Coupon coupon)
        {
            return coupon.AmountTypeId == CouponAmountType.Percentage;
        }

        public static Stripe.Coupon CreateStripeCoupon(this Coupon coupon, string stripeAccountId, StripeCouponDuration couponDuration, int? durationInMonths, decimal discountValue, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var stripeCoupon = Billing.StripeHelper.CreateCoupon(discountValue, stripeAccountId, coupon.AmountTypeId, couponDuration, durationInMonths, msgs);

            if (stripeCoupon.IsNotEmpty())
            {
                coupon.StripeCouponId = stripeCoupon.Id;
                msgs.AddRange(coupon.Save(dc: dc));
            }

            return stripeCoupon;
        }
    }
}
