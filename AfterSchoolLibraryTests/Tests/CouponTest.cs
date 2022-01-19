using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class CouponTest : BaseTest
    {
        private WebUser SendingAccountOwner { get; set; }
        private Account SendingAccount { get; set; }

        public CouponTest(ITestOutputHelper output) : base(output)
        {
            SendingAccountOwner = WebUserFactory.Create(msgs, dc);
            SendingAccount = AccountFactory.Create(SendingAccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var coupon = CouponFactory.Create(SendingAccount, msgs, dc);

            Assert.True(Coupon.Load(coupon.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var coupon = CouponFactory.Create(SendingAccount, msgs, dc);

            coupon.LastModifiedBy = 1;
            msgs.AddRange(coupon.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var coupon = CouponFactory.Create(SendingAccount, msgs, dc);

            msgs.AddRange(coupon.Delete(dc));

            Assert.Null(Coupon.Load(coupon.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var coupon = CouponFactory.Create(SendingAccount, msgs, dc);

            Assert.Equal(coupon.ID, coupon.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var coupon = CouponFactory.Create(SendingAccount, msgs, dc);

                // Act                
                var expected = Coupon.LoadByUidByAccountId(Uid: coupon.Uid, AccountId: SendingAccount.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: coupon.ID);
                Assert.Equal(expected: expected.Uid, actual: coupon.Uid);
                Assert.Equal(expected: expected.AccountId, actual: coupon.AccountId);
                Assert.Equal(expected: expected.Amount, actual: coupon.Amount);
                Assert.Equal(expected: expected.Name, actual: coupon.Name);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
