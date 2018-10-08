using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AttachedCouponService
    {
        public static Account GetAccount(this AttachedCoupon attachedCoupon, DatabaseActionHelper actionHelper)
        {
            if (attachedCoupon.AccountId.HasValue)
                return Account.Load(attachedCoupon.AccountId.Value, actionHelper.Messages, actionHelper.Connection);

            return null;
        }

        public static Coupon GetCoupon(this AttachedCoupon attachedCoupon, DatabaseActionHelper actionHelper)
        {
            if (attachedCoupon.CouponId.HasValue)
                return Coupon.Load(attachedCoupon.CouponId.Value, actionHelper.Messages, actionHelper.Connection);

            return null;
        }

        public static AttachedCoupon CreateNew(Account account, Coupon coupon, DatabaseActionHelper actionHelper)
        {
            var attachedCoupon = new AttachedCoupon()
            {
                AccountId = account.ID,
                CouponId = coupon.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = actionHelper.LoggedInUser.ID
            };

            actionHelper.AddMessages(attachedCoupon.Save(actionHelper.Connection));

            return attachedCoupon;
        }

    }
}
