using System;
using System.Collections.Generic;
using System.Text;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Billing;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class CouponService
    {
        public static Coupon CreateBlank(Account account, WebUser loggedInUser)
        {
            var coupon = new Coupon()
            {
                AccountId = account.ID.Value,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID
            };

            return coupon;
        }

        public static Coupon CreateOrUpdate(this Coupon coupon, Account account, decimal amount, DateTime? expirationDate, CouponAmountType couponAmountType, CouponApplyType couponApplyType, int? maxUsesAll, int? maxUsesPerPerson, string couponCode, string name, string description, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            coupon.AccountId = account.ID.Value;
            coupon.Amount = amount;
            coupon.ExpirationDate = expirationDate ?? null;
            coupon.AmountTypeId = couponAmountType;
            coupon.AppliesTo = couponApplyType;
            coupon.MaxNumberOfUsesAll = maxUsesAll ?? 0;
            coupon.MaxNumberOfUsesPerPerson = maxUsesPerPerson ?? 0;
            coupon.Name = name;
            coupon.Description = description;
            coupon.LastModifiedDate = DateTime.UtcNow;
            coupon.LastModifiedBy = loggedInUser?.ID;
            coupon.ValidateCouponCode(account.ID.Value, couponCode, msgs, dc);

            if (coupon.Uid.IsEmpty())
                coupon.Uid = new Guid();

            if (msgs.HasNoErrors())
                msgs.AddRange(coupon.Save(dc));

           return coupon;
        }

        public static Coupon CreateCoupon(Account account, decimal amount, DateTime? expirationDate, CouponAmountType couponAmountType, CouponApplyType couponApplyType, int? maxUsesAll, int? maxUsesPerPerson, string couponCode, string name, string description, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var coupon = CreateBlank(account, loggedInUser);

            coupon.AccountId = account.ID.Value;
            coupon.Amount = amount;
            coupon.ExpirationDate = expirationDate ?? null;
            coupon.AmountTypeId = couponAmountType;
            coupon.AppliesTo = couponApplyType;
            coupon.MaxNumberOfUsesAll = maxUsesAll ?? 0;
            coupon.MaxNumberOfUsesPerPerson = maxUsesPerPerson ?? 0;
            coupon.Name = name;
            coupon.Description = description;
            coupon.LastModifiedDate = DateTime.UtcNow;
            coupon.LastModifiedBy = loggedInUser?.ID;
            coupon.ValidateCouponCode(account.ID.Value, couponCode, msgs, dc);

            if (coupon.Uid.IsEmpty())
                coupon.Uid = new Guid();

            if (msgs.HasNoErrors())
                msgs.AddRange(coupon.Save(dc));

            return coupon;
        }

        public static void Remove(this Coupon coupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs.AddRange(coupon.Delete(dc));
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
                return String.Format("{0:0.00}%", (coupon.Amount).Value);

            return coupon.Amount.ToCurrency();
        }

        public static int GetNumberOfUses(this Coupon coupon)
        {
            return Purchase.LoadByCouponId(coupon.ID.Value).Count;
        }

        public static bool DoesApplyToRegistrationFee(this Coupon coupon)
        {
            return coupon.AppliesTo == CouponApplyType.RegistrationFee;
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

        public static List<ParticipationObject> GetParticipationObjects(this Coupon coupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var purchasbleObjectCoupons = coupon.GetPurchaseableObjectCoupons(msgs, dc);
            List<ParticipationObject> participationObjects = new List<ParticipationObject>();

            purchasbleObjectCoupons.ForEach(purchaseableObjectCoupon =>
            {
                participationObjects.Add(purchaseableObjectCoupon.GetPurchaseableObject(msgs, dc).GetParticipationObject(msgs,dc));
            });

            return participationObjects;
        }

        public static void ValidateCouponCode(this Coupon coupon, int accountId, string newCode, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //unique for given account and check for expiration date
            var coupons = Coupon.LoadByAccountId(accountId, msgs, dc).ToList();
            var duplicateCoupoun = coupons.Where(m => m.ID != coupon.ID && m.CouponCode.IsNotBlank() && m.CouponCode.ToLower() == newCode.ToLower() && m.ExpirationDate.IsNotEmpty() && DateTime.Now < m.ExpirationDate);

            if (duplicateCoupoun.Any())
                msgs.Add("This coupon code is in use, try something different.", MessageType.UserError);
            else
                coupon.CouponCode = newCode;
        }

        public static decimal GetValue(this Coupon coupon, decimal amount)
        {
            if (coupon.AmountTypeId == CouponAmountType.FlatAmount && coupon.Amount.HasValue)
                return coupon.Amount.Value;

            if (coupon.AmountTypeId == CouponAmountType.Percentage && coupon.Amount.HasValue)
                return amount * (coupon.Amount.Value / 100);

            return 0;
        }
    }
}
