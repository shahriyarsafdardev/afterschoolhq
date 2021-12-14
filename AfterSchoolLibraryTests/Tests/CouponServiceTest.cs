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

        [Theory(DisplayName = "Test GetValue For FlatAmount")]
        [InlineData(10, 107)] // Entire activity or single class, ACH, 107$
        [InlineData(10, 111)] // Entire activity or single class, CC, sin class 107$ + 4$ cc fee
        [InlineData(10, 214)] // 2 classes, ACH, per class 107$
        [InlineData(10, 222)] // 2 classes, CC, per class 107$ + 4$ cc fee
        public void GetValueTest1(decimal couponAmount, decimal activityRegistrationFee)
        {
            // Arrange
            var coupon = CouponFactory.Create(account: CouponAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: CouponAmountType.FlatAmount, msgs: msgs, dc: dc);

            // Act
            var amount = coupon.GetValue(amount: activityRegistrationFee);

            // Assert            
            Assert.True(amount > 0);

            var expected = Math.Min(coupon.Amount.Value, activityRegistrationFee);
            Assert.Equal(expected: expected, actual: amount);
        }

        [Theory(DisplayName = "Test GetValue For Percentage")]
        [InlineData(10, 107)] // Entire activity or single class, ACH, 107$
        [InlineData(10, 111)] // Entire activity or single class, CC, 107$ + 4$ cc fee
        [InlineData(10, 214)] // 2 classes, ACH, per class 107$
        [InlineData(10, 222)] // 2 classes, CC, per class 107$ + 4$ cc fee
        public void GetValueTest2(decimal couponAmount, decimal activityRegistrationFee)
        {
            // Arrange
            var coupon = CouponFactory.Create(account: CouponAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: CouponAmountType.Percentage, msgs: msgs, dc: dc);

            // Act
            var amount = coupon.GetValue(amount: activityRegistrationFee);

            // Assert
            Assert.True(amount > 0);

            var expected = Math.Round(activityRegistrationFee * (coupon.Amount.Value / 100));
            Assert.Equal(expected: expected, actual: amount);
        }

        [Theory(DisplayName = "Test GetDiscountedValue With FlatAmount Coupon")]
        [InlineData(10, 107)] // Entire activity or single class, ACH, 107$
        [InlineData(10, 111)] // Entire activity or single class, CC, sin class 107$ + 4$ cc fee
        [InlineData(10, 214)] // 2 classes, ACH, per class 107$
        [InlineData(10, 222)] // 2 classes, CC, per class 107$ + 4$ cc fee
        public void GetDiscountedValue1(decimal couponAmount, decimal activityRegistrationFee)
        {
            // Arrange
            var coupon = CouponFactory.Create(account: CouponAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: CouponAmountType.FlatAmount, msgs: msgs, dc: dc);

            // Act
            var amount = coupon.GetDiscountedValue(amount: activityRegistrationFee);

            // Assert            
            Assert.True(amount > 0);

            var expected = activityRegistrationFee - coupon.GetValue(amount: activityRegistrationFee);
            Assert.Equal(expected: expected, actual: amount);
        }

        [Theory(DisplayName = "Test GetDiscountedValue With Percentage Coupon")]
        [InlineData(10, 107)] // Entire activity or single class, ACH, 107$
        [InlineData(10, 111)] // Entire activity or single class, CC, sin class 107$ + 4$ cc fee
        [InlineData(10, 214)] // 2 classes, ACH, per class 107$
        [InlineData(10, 222)] // 2 classes, CC, per class 107$ + 4$ cc fee
        public void GetDiscountedValue2(decimal couponAmount, decimal activityRegistrationFee)
        {
            // Arrange
            var coupon = CouponFactory.Create(account: CouponAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: CouponAmountType.Percentage, msgs: msgs, dc: dc);

            // Act
            var amount = coupon.GetDiscountedValue(amount: activityRegistrationFee);

            // Assert            
            Assert.True(amount > 0);

            var expected = activityRegistrationFee - coupon.GetValue(amount: activityRegistrationFee);
            Assert.Equal(expected: expected, actual: amount);
        }

        [Theory(DisplayName = "Test GetDiscountedValue For Activity amount lower then coupon discount")]
        [InlineData(100, 50)]
        public void GetDiscountedValue3(decimal couponAmount, decimal activityRegistrationFee)
        {
            // Arrange
            var coupon = CouponFactory.Create(account: CouponAccount, amount: couponAmount, couponApplyType: CouponApplyType.RegistrationFee, couponAmountType: CouponAmountType.FlatAmount, msgs: msgs, dc: dc);

            // Act
            var amount = coupon.GetDiscountedValue(amount: activityRegistrationFee);

            // Assert            
            Assert.Equal(expected: 0, actual: amount);
        }
    }
}
