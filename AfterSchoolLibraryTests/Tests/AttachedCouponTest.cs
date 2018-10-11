using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;

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
            Coupon = CouponFactory.Create(SendingAccount, 50, CouponApplyType.TuitionFee, CouponAmountType.Percentage, msgs, dc);
            ReceivingAccountOwner = WebUserFactory.Create(msgs, dc);
            ReceivingAccount = AccountFactory.Create(ReceivingAccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var attachedCoupon = AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            Assert.True(AttachedCoupon.Load(attachedCoupon.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var attachedCoupon = Factories.AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                AttachedCoupon.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var attachedCoupon = Factories.AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);
            }

            Assert.Equal(numberToCreate, AttachedCoupon.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var attachedCoupon = Factories.AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            attachedCoupon.LastModifiedBy = 1;
            msgs.AddRange(attachedCoupon.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }
        [Fact]
        public void DeleteTest()
        {
            var attachedCoupon = Factories.AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            msgs.AddRange(attachedCoupon.Delete(dc));

            Assert.Equal(0, AttachedCoupon.GetCount(msgs: msgs, dc: dc));
        }
        [Fact]
        public void ReloadTest()
        {
            var attachedCoupon = Factories.AttachedCouponFactory.Create(Coupon, ReceivingAccount, msgs, dc);

            Assert.Equal(attachedCoupon.ID, attachedCoupon.Reload(msgs, dc).ID);
        }
    }
}
