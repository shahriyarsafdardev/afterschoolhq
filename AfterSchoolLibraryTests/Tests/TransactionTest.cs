using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Tests
{
    public class TransactionTest : BaseTest
    {
        private WebUser BillerAccountOwner { get; set; }
        private Account BillerAccount { get; set; }
        private WebUser PayerAccountOwner { get; set; }
        private Account PayerAccount { get; set; }

        public TransactionTest(ITestOutputHelper output) : base(output)
        {
            BillerAccountOwner = WebUserFactory.Create(msgs, dc);
            BillerAccount = AccountFactory.Create(BillerAccountOwner, msgs, dc);
            PayerAccountOwner = WebUserFactory.Create(msgs, dc);
            PayerAccount = AccountFactory.Create(PayerAccountOwner, msgs, dc);
        }

        [Fact]
        public void LoadTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            Assert.True(Transaction.Load(transaction.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            transaction.LastModifiedBy = 1;
            msgs.AddRange(transaction.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            msgs.AddRange(transaction.Delete(dc));

            Assert.Null(Transaction.Load(transaction.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            Assert.Equal(transaction.ID, transaction.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByBillerAccountIdTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            Assert.Single(Transaction.LoadByBillerAccountIdByIsDeleted(BillerAccount.ID.Value, false, msgs, dc));
        }

        [Fact]
        public void LoadByBillerAccountIdTestWithParticipationObject()
        {
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(BillerAccount, package, msgs, dc);
            var activity = ActivityFactory.Create(BillerAccount, msgs, dc);
            var participationObject = activity.GetParticipationObject(msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);
            var activityPayment = ActivityPaymentFactory.Create(BillerAccount, activity, registrant, transaction, msgs, dc);
            var participationObjects = participationObject.AsList().Select(po => (po.ObjectID.Value, po.ObjectType)).ToList();

            var transactions = Transaction.LoadByBillerAccountId(
                BillerAccount.ID.Value, 
                null,
                null,
                null,
                participationObjects, 
                false,
                null, 
                null, 
                msgs, 
                dc);

            Assert.Single(transactions);
        }

        [Fact]
        public void LoadByPayerAccountIdTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            Assert.Single(Transaction.LoadByPayerAccountId(PayerAccount.ID.Value, null, null, null, null, false, null, null, msgs, dc));
        }

        [Fact]
        public void LoadByPayerAccountIdTestWithParticipationObject()
        {
            var package = PackageFactory.Create(msgs, dc);
            var accountPackage = AccountPackageFactory.Create(BillerAccount, package, msgs, dc);
            var activity = ActivityFactory.Create(BillerAccount, msgs, dc);
            var participationObject = activity.GetParticipationObject(msgs, dc);
            var registrant = RegistrantFactory.Create(msgs, dc);
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);
            var activityPayment = ActivityPaymentFactory.Create(BillerAccount, activity, registrant, transaction, msgs, dc);
            var participationObjects = participationObject.AsList().Select(po => (po.ObjectID.Value, po.ObjectType)).ToList();

            var transactions = Transaction.LoadByPayerAccountId(
                PayerAccount.ID.Value,
                null,
                null,
                null,
                participationObjects,
                false,
                null,
                null,
                msgs,
                dc);

            Assert.Single(transactions);
        }

        [Fact]
        public void LoadByUidTest()
        {
            var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);

            Assert.NotNull(Transaction.LoadByUid(transaction.Uid, msgs, dc));
        }

        [Fact]
        public void GetTotalAmountReceivedByAccountIdTest()
        {
            var amounts = new List<decimal>
            {
                5.23.ToDecimal(),
                100.95.ToDecimal(),
                345.ToDecimal(),
                0.55.ToDecimal()
            };

            foreach (var amount in amounts)
            {
                var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, amount, msgs, dc);
            }

            Assert.Equal(amounts.Sum(), Transaction.GetTotalAmountBilledByBillerAccount(BillerAccount.ID.Value, null, null, null, null, null, false, msgs, dc));
        }

        [Fact]
        public void GetTotalAmountPaidByAccountIdTest()
        {
            var amounts = new List<decimal>
            {
                5.23.ToDecimal(),
                100.95.ToDecimal(),
                345.ToDecimal(),
                0.55.ToDecimal()
            };

            foreach (var amount in amounts)
            {
                var transaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, amount, msgs, dc);
            }

            Assert.Equal(amounts.Sum(), Transaction.GetTotalAmountPaidByPayerAccount(PayerAccount.ID.Value, null, null, null, null, false, msgs, dc));
        }

        [Fact]
        public void CreateNewTest()
        {
            var amount = 100;
            var originalTransaction = TransactionFactory.Create(BillerAccount, PayerAccount, PaymentType.ACH, msgs, dc);
            var purchase = PurchaseFactory.Create(PayerAccount, amount, msgs, dc);
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
                TransactionType.Refund,
                "re_1DNlPlCEgXQRZLhk3Rzzyzj6",
                originalTransaction,
                originalTransaction.PaymentTypeId,
                originalTransaction.PaymentSourceTypeId,
                false,
                BillerAccountOwner,
                msgs: msgs,
                dc: dc);

            Assert.True(transaction.ID.HasValue);
        }

        [Theory]
        [InlineData(100, 4, 6)]
        [InlineData(10, 1, 2)]
        public void GetTotalConvenienceFeesReceivedByBillerAccountTest(decimal amount, decimal paymentTypeFee, decimal convenienceFee)
        {
            try
            {
                // Arrange
                var billingAccountOwner = WebUserFactory.CreateWithEmail("DonJohn@test.com", msgs, dc);
                var purchasingAccountOwner = WebUserFactory.CreateWithEmail("CristineTrump@test.com", msgs, dc);
                var billerAccount = AccountFactory.CreateWithFreePackage(billingAccountOwner, AccountType.School, msgs, dc);
                var payerAccount = AccountFactory.CreateWithFreePackage(purchasingAccountOwner, AccountType.School, msgs, dc);

                var transaction = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.CreditCard, amount, 0, 0, billerAccount.ID.Value, billerAccount.ID.Value, convenienceFee, paymentTypeFee, msgs, dc);

                // Act
                var expected = Transaction.GetTotalConvenienceFeesReceivedByBillerAccount(BillerAccountId: billerAccount.ID.Value, paymentType: PaymentType.CreditCard, startDate: null, endDate: null, isDeleted: false, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected, actual: convenienceFee);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(true, ex.Message);
            }
        }
    }
}
