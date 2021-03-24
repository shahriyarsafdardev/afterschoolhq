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
    public class RefundRequest : BaseTest
    {
        private WebUser BillingWebUser { get; set; }
        private WebUser PayingWebUser { get; set; }
        private Account BillingAccount { get; set; }
        private Account PayingAccount { get; set; }
        private Transaction TestTransaction { get; set; }
        private RefundRequest TestRefundRequest { get; set; }

        public RefundRequest(ITestOutputHelper output) : base(output)
        {
            BillingWebUser = WebUserFactory.Create(msgs, dc);
            BillingAccount = AccountFactory.Create(BillingWebUser, msgs, dc);
            PayingWebUser = WebUserFactory.Create(msgs, dc);
            PayingAccount = AccountFactory.Create(PayingWebUser, msgs, dc);
            TestTransaction = TransactionFactory.Create(BillingAccount, PayingAccount, PaymentType.ACH, 50, msgs, dc);
            TestRefundRequest = Refun
        }

        [Fact(DisplayName = "LoadTest")]
        public void LoadTest()
        {
            TestTransaction.R
        }
    }
}
