using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AttachedCouponService
    {
        public static Account GetAccount(this AttachedCoupon attachedCoupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (attachedCoupon.AccountId.HasValue)
                return Account.Load(attachedCoupon.AccountId.Value, msgs, dc);

            return null;
        }

        public static Coupon GetCoupon(this AttachedCoupon attachedCoupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (attachedCoupon.CouponId.HasValue)
                return Coupon.Load(attachedCoupon.CouponId.Value, msgs, dc);

            return null;
        }

        public static AttachedCoupon CreateNew(Account account, Coupon coupon, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var attachedCoupon = new AttachedCoupon()
            {
                AccountId = account.ID,
                CouponId = coupon.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID
            };

            msgs.AddRange(attachedCoupon.Save(dc));

            return attachedCoupon;
        }

        public static SystemMessages Send(this AttachedCoupon attachedCoupon)
        {
            return EmailHelper.SendAttachedCoupon(attachedCoupon);
        }

    }
}
