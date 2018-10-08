using System;
using System.Collections.Generic;
using System.Text;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Services
{
    public static class CouponService
    {
        public static Coupon CreateNew(int accountId, decimal amount, DateTime expirationDate, CouponAmountType couponAmountType, CouponApplyType couponApplyType, int? maxUsesAll, int? maxUsesPerPerson, string couponCode, string label, string description, DatabaseActionHelper actionHelper)
        {
            var coupon = new Coupon()
            {
                AccountId = accountId,
                Amount = amount,
                CouponAmountTypeId = couponAmountType,
                CouponCode = couponCode,
                AppliesTo = couponApplyType,
                CreateDate = DateTime.UtcNow,
                CreatedBy = actionHelper.LoggedInUser.ID,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = actionHelper.LoggedInUser.ID,
                Description = description,
                ExpirationDate = expirationDate,
                MaxNumberOfUsesAll = maxUsesAll,
                MaxNumberOfUsesPerPerson = maxUsesPerPerson,
                Name = label
            };

            actionHelper.AddMessages(coupon.Save(actionHelper.Connection));

            return coupon;
        }

        public static string GenerateCouponCode(int accountId, DatabaseActionHelper actionHelper)
        {
            var code = Guid.NewGuid().ToString("n").Substring(0, 8);
            var coupon = Coupon.LoadByAccountIdByCouponCode(accountId, code, actionHelper.Messages, actionHelper.Connection);

            if (coupon.IsEmpty())
                return code;

            return GenerateCouponCode(accountId, actionHelper);
        }
    }
}
