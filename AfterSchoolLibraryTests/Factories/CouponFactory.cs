using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System;
using AfterSchoolLibrary.Services;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Factories
{
    public class CouponFactory
    {
        public static Coupon Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Coupon()
            {
                ID = 1,
                AccountId = account.ID,
                ExpirationDate = DateTime.UtcNow.AddMonths(1),
                CouponCode = CouponService.GenerateCouponCode(account.ID.Value, msgs, dc),
                Amount = 50,
                AppliesTo = CouponApplyType.RegistrationFee,
                AmountTypeId = CouponAmountType.Percentage,
                MaxNumberOfUsesAll = 1,
                MaxNumberOfUsesPerPerson = 1,
                Name = "Whatever",
                Description = "test description",
                IsDisabled = false                
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Coupon Create(Account account, decimal amount, CouponApplyType couponApplyType, CouponAmountType couponAmountType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Coupon()
            {
                AccountId = account.ID,
                ExpirationDate = DateTime.UtcNow.AddMonths(1),
                CouponCode = CouponService.GenerateCouponCode(account.ID.Value, msgs, dc),
                Amount = amount,
                AppliesTo = couponApplyType,
                AmountTypeId = couponAmountType,
                MaxNumberOfUsesAll = 1,
                MaxNumberOfUsesPerPerson = 1,
                Name = "Whatever"
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Coupon Create(Account account, decimal amount, CouponApplyType couponApplyType, CouponAmountType couponAmountType, List<PurchaseableObject> purchaseableObjects, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Coupon()
            {
                AccountId = account.ID,
                ExpirationDate = DateTime.UtcNow.AddMonths(1),
                CouponCode = CouponService.GenerateCouponCode(account.ID.Value, msgs, dc),
                Amount = amount,
                AppliesTo = couponApplyType,
                AmountTypeId = couponAmountType,
                MaxNumberOfUsesAll = 1,
                MaxNumberOfUsesPerPerson = 1,
                Name = "Whatever"
            };
            msgs.AddRange(obj.Save(dc: dc));

            purchaseableObjects.ForEach(po =>
           {
               PurchaseableObjectCouponFactory.Create(po, obj, msgs, dc);
           });
            

            return obj;
        }
    }
}
