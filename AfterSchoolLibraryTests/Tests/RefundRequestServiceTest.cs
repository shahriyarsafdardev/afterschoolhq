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
    public class RefundRequestServiceTest : BaseTest
    {
        private WebUser BillingWebUser { get; set; }
        private WebUser PayingWebUser { get; set; }
        private Account BillingAccount { get; set; }
        private Account PayingAccount { get; set; }
        private Transaction TestTransaction { get; set; }

        public RefundRequestServiceTest(ITestOutputHelper output) : base(output)
        {
            BillingWebUser = WebUserFactory.Create(msgs, dc);
            BillingAccount = AccountFactory.Create(BillingWebUser, msgs, dc);
            PayingWebUser = WebUserFactory.Create(msgs, dc);
            PayingAccount = AccountFactory.Create(PayingWebUser, msgs, dc);
            TestTransaction = TransactionFactory.Create(BillingAccount, PayingAccount, PaymentType.ACH, 50, msgs, dc);
        }

        [Fact]
        public void CreateNewTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestService.CreateNew(TestTransaction, amount, RefundRequestStatus.RequestedByParent,"i want my money back", PayingWebUser, msgs, dc);

            Assert.True(RefundRequest.Load(refundRequest.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void GetCreatorTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestFactory.Create(amount, TestTransaction, BillingAccount, "i want my money back", PayingWebUser,
                msgs, dc);

            Assert.Equal(PayingWebUser.ID.ToString(), refundRequest.GetCreator(msgs, dc).ID.ToString());
        }

        [Fact]
        public void ApproveAndProcessRequestTest()
        {
            var amount = 25;
            var refundRequest = RefundRequestFactory.Create(amount, TestTransaction, BillingAccount, "i want my money back", PayingWebUser, msgs, dc);
            refundRequest.ApproveAndProcessRequest(true, true, true, PaymentSourceType.AfterSchoolPlatform, BillingWebUser, msgs, dc);

            Assert.Equal(RefundRequestStatus.Completed, refundRequest.RefundRequestStatusId);
        }   
    }
}
