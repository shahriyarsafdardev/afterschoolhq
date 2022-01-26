using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class AttachedCouponTest : BaseTest
    {
        private WebUser ReceivingAccountOwner { get; set; }
        private Account ReceivingAccount { get; set; }
        private Coupon Coupon { get; set; }
        private WebUser SendingAccountOwner { get; set; }
        private Account SendingAccount { get; set; }

        public AttachedCouponTest(ITestOutputHelper output) : base(output)
        {
            SendingAccountOwner = WebUserFactory.Create(msgs, dc);
            SendingAccount = AccountFactory.Create(SendingAccountOwner, msgs, dc);
            Coupon = CouponFactory.Create(SendingAccount, 50, CouponApplyType.RegistrationFee, CouponAmountType.Percentage, msgs, dc);
            ReceivingAccountOwner = WebUserFactory.Create(msgs, dc);
            ReceivingAccount = AccountFactory.Create(ReceivingAccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var attachedCoupon = AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            Assert.True(AttachedCoupon.Load(attachedCoupon.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void LoadByCouponIdByAccountIdIsUsedTest()
        {
            var attachedCoupon = AttachedCouponFactory.Create(Coupon, ReceivingAccount, false, msgs, dc);

            Assert.True(AttachedCoupon.LoadByCouponIdByAccountIdIsUsed(Coupon.ID.Value, ReceivingAccount.ID.Value, false, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var attachedCoupon = AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            attachedCoupon.LastModifiedBy = 1;
            msgs.AddRange(attachedCoupon.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var attachedCoupon = AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            msgs.AddRange(attachedCoupon.Delete(dc));

            Assert.Null(AttachedCoupon.Load(attachedCoupon.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var attachedCoupon = AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            Assert.Equal(attachedCoupon.ID, attachedCoupon.Reload(msgs, dc).ID);
        }
    }
}
