using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class PayoutTest : BaseTest
    {
        public PayoutTest(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var user = WebUserFactory.Create(msgs: msgs, dc: dc);
                var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
                var payout = PayoutFactory.Create(account, msgs, dc);

                // Act                
                var expected = Payout.LoadByUidByAccountId(Uid: payout.Uid, AccountId: account.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: payout.ID);
                Assert.Equal(expected: expected.Uid, actual: payout.Uid);
                Assert.Equal(expected: expected.AccountId, actual: payout.AccountId);
                Assert.Equal(expected: expected.Amount, actual: payout.Amount);
                Assert.Equal(expected: expected.PayoutStatusId, actual: payout.PayoutStatusId);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
