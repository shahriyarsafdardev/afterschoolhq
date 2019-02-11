using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class AttachedCouponFactory
    {
        public static AttachedCoupon Create(Coupon coupon, Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AttachedCoupon() { CouponId = coupon.ID, AccountId = account.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static AttachedCoupon Create(Coupon coupon, Account account, bool isUsed, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AttachedCoupon() { CouponId = coupon.ID, AccountId = account.ID, IsUsed = isUsed };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
