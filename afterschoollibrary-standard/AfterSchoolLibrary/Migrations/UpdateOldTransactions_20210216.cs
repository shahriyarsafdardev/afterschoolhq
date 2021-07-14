using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateOldTransactions_20210216
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                var oldTransactions = Transaction.LoadByDate(new DateTime(2017, 01, 01), new DateTime(2019, 7, 1), false, null, null, amsgs, adc);

                foreach (var transaction in oldTransactions)
                {
                    decimal paymentTypeFeeCharged = 0M;
                    //(Platform Flat fee +(Cost * Platform Percent fee)) -Stripe Fee) = Convenience Fee
                    transaction.ConvenienceFeeCharged = (transaction.PlatformFlatFee.IfEmpty(0) + (transaction.Amount.IfEmpty(0) * transaction.PlatformPercentFee.IfEmpty(0))) - StripeHelper.GetStripeFee(transaction.Amount.IfEmpty(0), transaction.GetBillerAccount(amsgs, adc).IsCreditCardFeeTakenOutOfCost);
                    var cost = transaction.Amount.IfEmpty(0) + transaction.ConvenienceFeeCharged.IfEmpty(0);

                        if (cost <= 0 || transaction.PlatformFlatFee.IsEmpty() || transaction.PlatformPercentFee.IsEmpty())
                            continue;

                        transaction.PaymentTypeFeePayerAccountId = transaction.PayerAccountId;
                        transaction.ConvenienceFeePayerAccountId = transaction.BillerAccountId;

                        //.. calculate PaymentTypeFeeCharged
                        if (transaction.PaymentTypeId == PaymentType.CreditCard)
                        {
                            var billingAccount = transaction.GetBillerAccount(amsgs, adc);
                            paymentTypeFeeCharged = PlatformOldLogic.GetCreditCardFee(cost, billingAccount);
                        }

                        transaction.PaymentTypeFeeCharged = paymentTypeFeeCharged;

                        transaction.Save(adc);
                    }

                },
                uniqueIdenfier: "UpdateOldTransactions_20210216",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
