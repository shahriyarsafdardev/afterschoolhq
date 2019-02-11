using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class PurchaseableObjectCouponFactory
    {
        public static PurchaseableObjectCoupon Create(PurchaseableObject purchaseableObject, Coupon coupon, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new PurchaseableObjectCoupon()
            {
                CouponId = coupon.ID,
                PurchaseableObjectId = purchaseableObject.ObjectID,
                PurchaseableObjectTypeId = purchaseableObject.ObjectType
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
