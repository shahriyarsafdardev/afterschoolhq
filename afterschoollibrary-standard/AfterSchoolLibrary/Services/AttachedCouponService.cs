using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AttachedCouponService
    {
        public static Account GetAccount(this AttachedCoupon attachedCoupon, DatabaseActionHelper databaseActivity)
        {
            if (attachedCoupon.AccountId.HasValue)
                return Account.Load(attachedCoupon.AccountId.Value, databaseActivity.Messages, databaseActivity.Connection);

            return null;
        }

        public static Coupon GetCoupon(this AttachedCoupon attachedCoupon, DatabaseActionHelper databaseActivity)
        {
            if (attachedCoupon.CouponId.HasValue)
                return Coupon.Load(attachedCoupon.CouponId.Value, databaseActivity.Messages, databaseActivity.Connection);

            return null;
        }


    }
}
