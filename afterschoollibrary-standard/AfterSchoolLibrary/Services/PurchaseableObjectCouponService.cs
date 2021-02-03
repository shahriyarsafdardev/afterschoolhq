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
        public static PurchaseableObjectCoupon CreateNew(PurchaseableObject purchaseableObject, WebUser loggedInUser, int CouponId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var poc = new PurchaseableObjectCoupon()
            {
                CouponId = CouponId,
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

        public static void InsertPurchaseableObjectCoupons(List<int> activityIds, int CouponId, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activities = Activity.LoadList(activityIds, msgs);
            activities.ForEach(activity =>
            {
                CreateNew(
                    purchaseableObject: activity.GetPurchaseableObject(),                 
                    loggedInUser: loggedInUser,
                    CouponId: CouponId,
                    msgs: msgs,
                    dc: dc);
            });
        }

        public static void ClearPurchaseableObjectCoupons(int couponId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var purchasableObjects = PurchaseableObjectCoupon.LoadByCouponId(couponId);
            if (purchasableObjects.Count > 0)
            {
                purchasableObjects.ForEach(coupon =>
                {
                    msgs.AddRange(coupon.Delete(dc));
                });
            }
        }
    }
}
