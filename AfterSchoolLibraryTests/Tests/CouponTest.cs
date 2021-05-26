using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibraryTests.Factories;

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

        [Theory]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var coupon = CouponFactory.Create(SendingAccount, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate),
                Coupon.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            for (var i = 0; i < numberToCreate; i++)
            {
                var coupon = Factories.CouponFactory.Create(SendingAccount, msgs, dc);
            }

            Assert.Equal(numberToCreate, Coupon.GetCount(msgs: msgs, dc: dc));
        }

        [Fact]
        public void SaveTest()
        {
            var coupon = Factories.CouponFactory.Create(SendingAccount, msgs, dc);

            coupon.LastModifiedBy = 1;
            msgs.AddRange(coupon.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }
        [Fact]
        public void DeleteTest()
        {
            var coupon = Factories.CouponFactory.Create(SendingAccount, msgs, dc);

            msgs.AddRange(coupon.Delete(dc));

            Assert.Equal(0, Coupon.GetCount(msgs: msgs, dc: dc));
        }
        [Fact]
        public void ReloadTest()
        {
            var coupon = Factories.CouponFactory.Create(SendingAccount, msgs, dc);

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
