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
            var transactionOutsideAHQ = TransactionFactory.CreateManualTransaction(
                BillerAccount,
                PayerAccount,
                PaymentType.ACH,
                amount,
                platformPercentFee,
                platformFlatFee,
                PayerAccount.ID.Value, // convenience fee payer
                BillerAccount.ID.Value, // payment type fee payer
                convenienceFee,
                paymentTypeFeeCharged,
                PaymentSourceType.ManualAfterSchoolHQ,
                msgs,
                dc
            );

            var oldTransaction = TransactionFactory.Create(
                BillerAccount, 
                PayerAccount, 
                PaymentType.ACH, 
                amount, 
                platformPercentFee, 
                platformFlatFee,
                null, // convenience fee payer
                PayerAccount.ID.Value, // payment type fee payer
                null, 
                null,
                msgs, 
                dc
            );

           var billerPaysNoFees = TransactionFactory.Create(
                BillerAccount,
                PayerAccount,
                PaymentType.ACH,
                amount,
                platformPercentFee,
                platformFlatFee,
                PayerAccount.ID.Value, // convenience fee payer
                PayerAccount.ID.Value, // payment type fee payer
                convenienceFee,
                paymentTypeFeeCharged,
                msgs,
                dc
            );

            var billerPaysConvenienceFee = TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.ACH,
                 amount,
                 platformPercentFee,
                 platformFlatFee,
                 BillerAccount.ID.Value, // convenience fee payer
                 PayerAccount.ID.Value, // payment type fee payer
                 convenienceFee,
                 paymentTypeFeeCharged,
                 msgs,
                 dc
             );

            var billerPaysPaymentTypeFees = TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.CreditCard,
                 amount,
                 platformPercentFee,
                 platformFlatFee,
                 PayerAccount.ID.Value,  // convenience type fee payer
                 BillerAccount.ID.Value,  // payment type fee payer
                 convenienceFee,
                 paymentTypeFeeCharged, 
                 msgs,
                 dc
             );

            var billerPaysBothFees = TransactionFactory.Create(
                 BillerAccount,
                 PayerAccount,
                 PaymentType.CreditCard,
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


            if (msgs.HasNoErrors())
            {
                Assert.Equal(expected: 0M, actual: transactionOutsideAHQ.GetFeesChargedToBiller());
                Assert.Equal(expected: convenienceFee, actual: oldTransaction.GetFeesChargedToBiller());
                Assert.Equal(expected: 0M, actual: billerPaysNoFees.GetFeesChargedToBiller());
                Assert.Equal(expected: convenienceFee, actual:  billerPaysConvenienceFee.GetFeesChargedToBiller());
                Assert.Equal(expected: paymentTypeFeeCharged, billerPaysPaymentTypeFees.GetFeesChargedToBiller());
                Assert.Equal(expected: convenienceFee + paymentTypeFeeCharged, billerPaysBothFees.GetFeesChargedToBiller());
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

            Assert.Equal(Math.Max(amount - paymentTypeFee,paymentTypeFee), transaction1.GetRefundableAmount(false, msgs, dc));
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
        [InlineData(50, 3.49, 4.18)]
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

            var refund = transaction.CreateNewRefund(
                        fullAmount: transaction.GetPayerAmount(),
                        convenienceFeeAmount: convenienceFee,
                        paymentTypeFeeAmount: paymentTypeFee,
                        refundId: null,
                        transactionType: TransactionType.Refund,
                        paymentType: PaymentType.CreditCard,
                        paymentSourceType: transaction.PaymentSourceTypeId,
                        refundingAccount: transaction.GetBillerAccount(msgs, dc),
                        refundedAccount: transaction.GetPayerAccount(msgs, dc),
                        loggedInUser: PayerAccountOwner,
                        msgs: msgs,
                        dc: dc);

            Assert.True(refund.IsNotEmpty());
            Assert.True(transaction.IsTotallyRefunded(true, msgs, dc));
        }
        
        [Theory]
        [InlineData(110, 6, 4)]
        public void GetRefundableAmountWithCCFeeTest(decimal amount, decimal convenienceFee, decimal paymentTypeFee)
        {
            try
            {
                var transaction = TransactionFactory.Create(
                    BillerAccount,
                    PayerAccount,
                    PaymentType.ACH,
                    amount, //here this will be a payer amount as we are dealing with transaction table
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

                var refundableAmount = transaction.GetRefundableAmount(allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);
                var expectedRefund = amount - paymentTypeFee;
                var expectedRefund2 = transaction.GetBillerAmount(msgs, dc) + convenienceFee;

                Assert.Equal(refundableAmount, expectedRefund);
                Assert.Equal(refundableAmount, expectedRefund2);
                Assert.True(msgs.HasNoErrors());
            }
            catch (Exception ex)
            {
                Assert.False(true, ex.Message);
            }           
        }

        [Theory]
        [InlineData(110, 6, 4)]
        public void IsAmountRefundableWithCCFeeTest(decimal amount, decimal convenienceFee, decimal paymentTypeFee)
        {
            try
            {
                var transaction = TransactionFactory.Create(
                    BillerAccount,
                    PayerAccount,
                    PaymentType.ACH,
                    amount, //here this will be a payer amount as we are dealing with transaction table
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

                var issAmountRefundable1 = transaction.IsAmountRefundable(107, allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);
                var issAmountRefundable2 = transaction.IsAmountRefundable(110, allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);
                var issAmountRefundable3 = transaction.IsAmountRefundable(106, allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);
                var issAmountRefundable4 = transaction.IsAmountRefundable(108, allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);
                var issAmountRefundable5 = transaction.IsAmountRefundable(100, allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);
                var issAmountRefundable6 = transaction.IsAmountRefundable(0, allowFullAmountRefund: false, isPartialRefundWithConvenienceFee: true, msgs: msgs, dc: dc);

                Assert.False(issAmountRefundable1);
                Assert.False(issAmountRefundable2);
                Assert.True(issAmountRefundable3);                
                Assert.False(issAmountRefundable4);
                Assert.True(issAmountRefundable5);
                Assert.False(issAmountRefundable6);

                Assert.True(msgs.HasNoErrors());
            }
            catch (Exception ex)
            {
                Assert.False(true, ex.Message);
            }
        }

        [Fact]
        public void RefundTest()
        {
            var amount = 100;
            var originalTransaction = Factories.TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);
            var purchase = Factories.PurchaseFactory.Create(PayerAccount, amount, msgs, dc);
            var transaction = TransactionService.CreateNew(
                BillerAccount,
                PayerAccount,
                amount,
                originalTransaction.ConvenienceFeeCharged,
                originalTransaction.ConvenienceFeePayerAccountId,
                originalTransaction.PaymentTypeFeeCharged,
                originalTransaction.ConvenienceFeePayerAccountId,
                purchase,
                "Net Club refund",
                TransactionType.Charge,
                "re_1DNlPlCEgXQRZLhk3Rzzyzj6",
                originalTransaction,
                originalTransaction.PaymentTypeId,
                originalTransaction.PaymentSourceTypeId,
                false,
                BillerAccountOwner,
                msgs: msgs,
                dc: dc);

            var refund = transaction.Refund(99, true, 0, 0, BillerAccountOwner, msgs, dc);

            Assert.True(refund.StripeChargeOrRefundId.IsNotEmpty());
        }
    }
}
