using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests.Tests
{
    public class PurchaseableObjectCouponTest : BaseTest
    {
        private WebUser SendingAccountOwner { get; set; }
        private Account SendingAccount { get; set; }
        private Activity Activity { get; set; }
        private PurchaseableObject PurchaseableObject { get; set; }
        private Coupon Coupon { get; set; }

        public PurchaseableObjectCouponTest(ITestOutputHelper output) : base(output)
        {
            SendingAccountOwner = WebUserFactory.Create(msgs, dc);
            SendingAccount = AccountFactory.Create(SendingAccountOwner, msgs, dc);
            Activity = ActivityFactory.Create(SendingAccount, msgs, dc);
            PurchaseableObject = Activity.GetPurchaseableObject();
            Coupon = CouponFactory.Create(SendingAccount, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var poc = PurchaseableObjectCouponFactory.Create(PurchaseableObject, Coupon, msgs, dc);

            Assert.True(PurchaseableObjectCoupon.Load(poc.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var poc = PurchaseableObjectCouponFactory.Create(PurchaseableObject, Coupon, msgs, dc);

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var poc = PurchaseableObjectCouponFactory.Create(PurchaseableObject, Coupon, msgs, dc);

            msgs.AddRange(poc.Delete(dc));

            Assert.Null(PurchaseableObjectCoupon.Load(poc.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var poc = PurchaseableObjectCouponFactory.Create(PurchaseableObject, Coupon, msgs, dc);

            Assert.Equal(poc.ID, poc.Reload(msgs, dc).ID);
        }
    }
}
