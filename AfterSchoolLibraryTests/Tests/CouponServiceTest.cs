using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class CouponServiceTest : BaseTest
    {
        private WebUser CouponAccountOwner { get; set; }
        private Account CouponAccount { get; set; }

        public CouponServiceTest(ITestOutputHelper output) : base(output)
        {
            CouponAccountOwner = WebUserFactory.Create(msgs, dc);
            CouponAccount = AccountFactory.Create(CouponAccountOwner, msgs, dc);
        }

        [Fact(DisplayName ="RemoveTest")]
        public void RemoveTest()
        {
            var coupon = CouponFactory.Create(CouponAccount, msgs, dc);

            Assert.True(Coupon.Load(coupon.ID.Value, msgs, dc).IsNotEmpty());

            coupon.Remove(msgs, dc);

            Assert.True(Coupon.Load(coupon.ID.Value, msgs, dc).IsEmpty());
        }

        [Fact(DisplayName = "GenerateCouponCodeTest")]
        public void GenerateCouponCodeTest()
        {
            var couponCode = CouponService.GenerateCouponCode(CouponAccount.ID.Value, msgs, dc);

            Assert.True(couponCode.IsNotEmpty());
        }

        [Fact(DisplayName = "GetAccountTest")]
        public void GetAccountTest()
        {
            var coupon = CouponFactory.Create(CouponAccount, msgs, dc);
            
            Assert.Equal(CouponAccount.ID.Value, coupon.GetAccount(msgs, dc).ID.Value);
        }
    }
}
