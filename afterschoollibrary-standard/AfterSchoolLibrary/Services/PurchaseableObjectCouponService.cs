using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class PurchaseableObjectCouponService
    {
        public static PurchaseableObjectCoupon CreateNew(PurchaseableObject purchaseableObject, Coupon coupon, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var poc = new PurchaseableObjectCoupon()
            {
                CouponId = coupon.ID,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID,
                LastModifiedBy = loggedInUser?.ID,
                PurchaseableObjectId = purchaseableObject.ObjectID,
                PurchaseableObjectTypeId = purchaseableObject.ObjectType,                   
            };

            msgs.AddRange(poc.Save(dc));

            return poc;
        }

        public static PurchaseableObject GetPurchaseableObject(this PurchaseableObjectCoupon purchaseableObjectCoupon, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PurchaseableObject.FromPurchaseableObjectCoupon(purchaseableObjectCoupon, msgs, dc);
        }
    }
}
