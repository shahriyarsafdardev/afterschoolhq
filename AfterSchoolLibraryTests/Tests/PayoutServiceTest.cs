using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Collections.Generic;
using BasicLibrary;
using System;
using System.Linq;
using System.Transactions;

namespace AfterSchoolLibraryTests.Tests
{
    public class PayoutServiceTest : BaseTest
    {

        public PayoutServiceTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void DeleteAndUpdateTransactionsTest()
        {
            var billerUser = WebUserFactory.Create(msgs, dc);
            var billerAccount = AccountFactory.Create(billerUser, msgs, dc);
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var payout = PayoutFactory.Create(billerAccount, msgs, dc);
            var payout2 = PayoutFactory.Create(billerAccount, msgs, dc);
            var transactionCount = 5;

            for (var i = 0; i < transactionCount; i++)
            {
                var transaction = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.ACH, msgs, dc);
                transaction.PayoutId = payout.ID.Value;
                msgs.AddRange(transaction.Save(dc));
            }

            // create one that isn't attached to the payout
            var transaction2 = TransactionFactory.Create(billerAccount, payerAccount, PaymentType.ACH, msgs, dc);
            transaction2.PayoutId = payout2.ID.Value;
            msgs.AddRange(transaction2.Save(dc));

            payout.DeleteAndUpdateTransactions(billerUser, msgs, dc);

            var transactions = AfterSchoolLibrary.Models.Transaction.LoadByPayoutIdByIsDeleted(payout.ID.Value, false, msgs, dc);
            var transaction2Reload = AfterSchoolLibrary.Models.Transaction.LoadByPayoutIdByIsDeleted(payout2.ID.Value, false, msgs, dc);
            var payoutReload = Payout.Load(payout.ID.Value, msgs, dc);

            Assert.True(msgs.HasNoErrors());
            Assert.Empty(transactions);
            Assert.Null(payoutReload);
            Assert.Single(transaction2Reload);
        }

        [Fact(DisplayName = "Test CreateNew")]
        public void CreatePayoutTest()
        {
            // Arrange
            var billerUser = WebUserFactory.Create(msgs, dc);
            var billerAccount = AccountFactory.Create(billerUser, msgs, dc);
            var payerUser = WebUserFactory.Create(msgs, dc);
            var payerAccount = AccountFactory.Create(payerUser, msgs, dc);
            var transaction = TransactionFactory.Create(billerAccount: billerAccount, payerAccount: payerAccount, paymentType: PaymentType.ACH, amount: 100,
                platformPercentFee: 0, platformFlatFee: 0, convenienceFeePayerAccountId: billerUser.ID.Value, paymentTypeFeePayerAccountId: payerAccount.ID.Value,
                convenienceFeeCharged: 7, paymentTypeFeeCharged: 4, msgs: msgs, dc: dc, transactionType: TransactionType.Charge);

            // Act
            var payout = PayoutService.CreateNew(account: billerAccount, statementDescriptor: "Ahq Payout - Test", transactions: transaction.AsList(), 
                payoutStatus: PayoutStatus.Requested, paymentType: PaymentType.CreditCard, loggedInUser: payerUser,
                msgs: msgs, dc: dc);

            // Assert
            Assert.True(msgs.HasNoErrors());
            Assert.NotNull(payout);
            Assert.NotNull(payout.ID);
            Assert.Equal(expected: "Ahq Payout - Test", actual: payout.StatementDescriptor);

            var payoutTransactions = payout.GetTransactions(msgs: msgs, dc: dc);            
            Assert.Single(payoutTransactions);
            Assert.Equal(payoutTransactions.FirstOrDefault().ID, transaction.ID);            
        }
    }
}
