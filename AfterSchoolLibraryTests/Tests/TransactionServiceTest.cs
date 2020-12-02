using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class TransactionServiceTest : BaseTest
    {
        private WebUser BillerAccountOwner { get; set; }
        private Account BillerAccount { get; set; }
        private WebUser PayerAccountOwner { get; set; }
        private Account PayerAccount { get; set; }

        public TransactionServiceTest(ITestOutputHelper output) : base(output)
        {
            BillerAccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            BillerAccount = Factories.AccountFactory.CreateWithFreePackage(BillerAccountOwner, AccountType.ActivityProvider, msgs, dc);
            PayerAccountOwner = Factories.WebUserFactory.Create(msgs, dc);
            PayerAccount = Factories.AccountFactory.Create(PayerAccountOwner, msgs, dc);
        }

        /// <summary>
        /// Gets Fees charged to biller
        /// </summary>
        /// <param name="amount"></param>
        [Theory]
        [InlineData(24.99, 0.05, 0.0, 2)]
        [InlineData(1, 0.05, 0.3, 4)]
        public void GetFeesChargedToBillerTest(decimal amount, decimal platformPercentFee, decimal platformFlatFee, decimal paymentTypeFeeCharged)
        {

            var convenienceFee = (amount * platformPercentFee) + platformFlatFee;
            var transaction1 = Factories.TransactionFactory.Create(
                BillerAccount, 
                PayerAccount, 
                PaymentType.ACH, 
                amount, 
                platformPercentFee, 
                platformFlatFee, 
                BillerAccount.ID.Value,
                PayerAccount.ID.Value,
                convenienceFee, 
                paymentTypeFeeCharged,
                msgs, 
                dc
            );

           var transaction2 = Factories.TransactionFactory.Create(
                BillerAccount,
                PayerAccount,
                PaymentType.ACH,
                amount,
                platformPercentFee,
                platformFlatFee,
                PayerAccount.ID.Value,
                BillerAccount.ID.Value,
                convenienceFee,
                paymentTypeFeeCharged,
                msgs,
                dc
            );

            var transaction3 = Factories.TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.ACH,
                 amount,
                 platformPercentFee,
                 platformFlatFee,
                 BillerAccount.ID.Value,
                 BillerAccount.ID.Value,
                 convenienceFee,
                 paymentTypeFeeCharged,
                 msgs,
                 dc
             );

            var transaction4 = Factories.TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.ACH,
                 amount,
                 platformPercentFee,
                 platformFlatFee,
                 null,
                 BillerAccount.ID.Value,
                 convenienceFee,
                 paymentTypeFeeCharged,
                 msgs,
                 dc
             );


            if (msgs.HasNoErrors())
            {
                Assert.Equal(convenienceFee, transaction1.GetFeesChargedToBiller());
                Assert.Equal(paymentTypeFeeCharged, transaction2.GetFeesChargedToBiller());
                Assert.Equal(paymentTypeFeeCharged + convenienceFee, transaction3.GetFeesChargedToBiller());
                Assert.Equal(convenienceFee, transaction4.GetFeesChargedToBiller());
            }
            else
                Assert.True(false);
                
        }

        /// <summary>
        /// Get the Refundable Amount from transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="convenienceFee"></param>
        /// <param name="paymentTypeFee"></param>
        [Theory]
        [InlineData(0.01, 1,1)]
        [InlineData(5,1,1)]
        public void GetRefundableAmountTest(decimal amount, decimal convenienceFee, decimal paymentTypeFee)
        {
            var transaction1 = Factories.TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.ACH,
                 amount,
                 0,
                 0,
                 BillerAccount.ID.Value,
                 PayerAccount.ID.Value,
                 convenienceFee,
                 paymentTypeFee,
                 msgs,
                 dc,
                 TransactionType.Charge
             );

            var transaction2 = Factories.TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.ACH,
                 amount,
                 0,
                 0,
                 BillerAccount.ID.Value,
                 PayerAccount.ID.Value,
                 convenienceFee,
                 paymentTypeFee,
                 msgs,
                 dc,
                 TransactionType.NotSet
             );

            Assert.Equal(amount - paymentTypeFee, transaction1.GetRefundableAmount(false, msgs, dc));
            Assert.Equal(amount, transaction1.GetRefundableAmount(true, msgs, dc));

            Assert.Equal(0, transaction2.GetRefundableAmount(false, msgs, dc));
        }

        [Theory]
        [InlineData(0.1,1,1)]
        [InlineData(25, 2.49, 2.18)]
        public void GetBillerAmountTest(decimal amount, decimal paymentTypeFee, decimal convenienceFee)
        {
            var transaction = TransactionFactory.Create(
                BillerAccount, 
                PayerAccount, 
                PaymentType.CreditCard, 
                amount, 
                0,
                0,
                BillerAccount.ID.Value, 
                PayerAccount.ID.Value, 
                convenienceFee, 
                paymentTypeFee, 
                msgs, 
                dc
            );

            Assert.Equal(Math.Max(amount - (convenienceFee + paymentTypeFee),0), transaction.GetBillerAmount(msgs,dc));
        }

        [Theory]
        [InlineData(25, 2.49, 2.18)]
        public void IsTotallyRefundedTest(decimal amount, decimal paymentTypeFee, decimal convenienceFee)
        {
            var transaction = TransactionFactory.Create(
                BillerAccount,
                PayerAccount,
                PaymentType.CreditCard,
                amount,
                0,
                0,
                BillerAccount.ID.Value,
                PayerAccount.ID.Value,
                convenienceFee,
                paymentTypeFee,
                msgs,
                dc
            );

            var refund = transaction.RefundPayerFullAmount(BillerAccountOwner, msgs, dc);

            Assert.True(refund.IsTotallyRefunded(true, false, msgs, dc));
        }

    }
}
