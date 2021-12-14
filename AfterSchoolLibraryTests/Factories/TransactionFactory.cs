using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class TransactionFactory
    {
        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = 13.95.ToDecimal(),
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase",
                TransactionTypeId = TransactionType.Charge
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction CreateTransactionOfPastDate(Account billerAccount, Account payerAccount, decimal amount, PaymentType paymentType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                CreateDate = paymentType == PaymentType.ACH ? DateTime.Now.AddDays(-6) : DateTime.Now.AddDays(-3), // 5 days for ACH and 2 days for CC
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase",
                TransactionTypeId = TransactionType.Charge,
                PaymentSourceTypeId = PaymentSourceType.AfterSchoolPlatform,
                UsedStripeConnect = false,
                ConvenienceFeeCharged = 7,
                ConvenienceFeePayerAccountId = payerAccount.ID,
                PaymentTypeFeeCharged = 4,
                PaymentTypeFeePayerAccountId = payerAccount.ID
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, decimal amount, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase",
                TransactionTypeId = TransactionType.Charge
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction Create(Account billerAccount, Account payerAccount, PaymentType paymentType, decimal amount, decimal platformPercentFee, decimal platformFlatFee, int? convenienceFeePayerAccountId, int? paymentTypeFeePayerAccountId, decimal? convenienceFeeCharged, decimal? paymentTypeFeeCharged,  SystemMessages msgs, DatabaseConnection dc, TransactionType transactionType = TransactionType.Charge)
        {
            var obj = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase",
                PlatformFlatFee = platformFlatFee,
                PlatformPercentFee = platformPercentFee,
                ConvenienceFeeCharged = convenienceFeeCharged,
                ConvenienceFeePayerAccountId = convenienceFeePayerAccountId,
                PaymentTypeFeeCharged = paymentTypeFeeCharged,
                PaymentTypeFeePayerAccountId = paymentTypeFeePayerAccountId,
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = PaymentSourceType.AfterSchoolPlatform,
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static Transaction CreateUsingBillingAccountPackageFeatures(Account billerAccount, Account payerAccount, PaymentType paymentType, decimal amount, SystemMessages msgs, DatabaseConnection dc)
        {
            var accountPackage = AccountPackage.LoadByAccountIdByIsActive(billerAccount.ID.Value, true, msgs, dc);
            var platformFlatFee = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();
            var platformPecrentFee = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();

            var obj = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase",
                PlatformFlatFee = platformFlatFee,
                PlatformPercentFee = platformPecrentFee,
                ConvenienceFeeCharged = (amount * platformPecrentFee) + platformFlatFee,
                ConvenienceFeePayerAccountId = billerAccount.ID.Value,
                PaymentTypeFeeCharged = paymentType == PaymentType.CreditCard ? new FeeCalculationObject(amount, billerAccount, msgs, dc).GetCreditCardFee() : 0,
                PaymentTypeFeePayerAccountId = payerAccount.ID.Value,
                TransactionTypeId = TransactionType.Charge
            };

             msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static Transaction CreateManualTransaction(Account billerAccount, Account payerAccount, PaymentType paymentType, decimal amount, decimal platformPercentFee, decimal platformFlatFee, int? convenienceFeePayerAccountId, int? paymentTypeFeePayerAccountId, decimal? convenienceFeeCharged, decimal? paymentTypeFeeCharged, PaymentSourceType paymentSourceType, SystemMessages msgs, DatabaseConnection dc, TransactionType transactionType = TransactionType.Charge)
        {
            var obj = new Transaction()
            {
                Amount = amount,
                BillerAccountId = billerAccount.ID,
                PayerAccountId = payerAccount.ID,
                PaymentTypeId = paymentType,
                Description = "User purchase",
                PlatformFlatFee = platformFlatFee,
                PlatformPercentFee = platformPercentFee,
                ConvenienceFeeCharged = convenienceFeeCharged,
                ConvenienceFeePayerAccountId = convenienceFeePayerAccountId,
                PaymentTypeFeeCharged = paymentTypeFeeCharged,
                PaymentTypeFeePayerAccountId = paymentTypeFeePayerAccountId,
                TransactionTypeId = transactionType,
                PaymentSourceTypeId = paymentSourceType,
            };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
