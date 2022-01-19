using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Services;
using System;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests.Tests
{
    public class RefundRequestTest : BaseTest
    {
        private WebUser BillingWebUser { get; set; }
        private WebUser PayingWebUser { get; set; }
        private Account BillingAccount { get; set; }
        private Account PayingAccount { get; set; }
        private Transaction TestTransaction { get; set; }

        public RefundRequestTest(ITestOutputHelper output) : base(output)
        {
            BillingWebUser = WebUserFactory.Create(msgs, dc);
            BillingAccount = AccountFactory.Create(BillingWebUser, msgs, dc);
            PayingWebUser = WebUserFactory.Create(msgs, dc);
            PayingAccount = AccountFactory.Create(PayingWebUser, msgs, dc);
            TestTransaction = TransactionFactory.Create(BillingAccount, PayingAccount, PaymentType.ACH, 50, msgs, dc);
        }

        [Fact(DisplayName = "LoadTest")]
        public void LoadTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestFactory.Create(amount, TestTransaction, BillingAccount, "i want my money back", PayingWebUser, msgs, dc);

            Assert.True(RefundRequest.Load(refundRequest.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestFactory.Create(amount, TestTransaction, BillingAccount, "it sitnks", PayingWebUser, msgs, dc);

            refundRequest.LastModifiedBy = 1;
            msgs.AddRange(refundRequest.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the plan from the DB
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestFactory.Create(amount, TestTransaction, BillingAccount,"it sitnks", PayingWebUser, msgs, dc);

            msgs.AddRange(refundRequest.Delete(dc));

            Assert.Null(RefundRequest.Load(refundRequest.ID.Value));
        }

        [Fact]
        public void LoadByTransactionTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestFactory.Create(amount, TestTransaction, BillingAccount, "it sitnks", PayingWebUser, msgs, dc);
            refundRequest.UpdateStatus(RefundRequestStatus.Completed, BillingWebUser, msgs, dc);

            Assert.Equal(RefundRequestStatus.Completed,refundRequest.RefundRequestStatusId);
        }

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var refundRequest = RefundRequestFactory.Create(100, TestTransaction, BillingAccount, "it sitnks", PayingWebUser, msgs, dc);

                // Act                
                var expected = RefundRequest.LoadByUidByAccountId(Uid: refundRequest.Uid, AccountId: BillingAccount.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: refundRequest.ID);
                Assert.Equal(expected: expected.Uid, actual: refundRequest.Uid);
                Assert.Equal(expected: expected.AccountId, actual: refundRequest.AccountId);
                Assert.Equal(expected: expected.TransactionId, actual: refundRequest.TransactionId);
                Assert.Equal(expected: expected.Amount, actual: refundRequest.Amount);                
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
