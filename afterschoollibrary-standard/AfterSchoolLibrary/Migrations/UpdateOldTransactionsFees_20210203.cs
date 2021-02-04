using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateOldTransactionsFees_20210203
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    //.. For older records with only Platform Fees stored as a percentage or flat fee, and the Conv Fee and CC Fee is null, the platform fees were always taken out of the cost
                    var loggedInUser = WebUser.Load(1, amsgs, adc);
                    var oldTransactions = Transaction.Load(
                                            criterias: new Hashtable() { 
                                                { "TransactionTypeId", (int)TransactionType.Charge }, 
                                                { "ConvenienceFeeCharged", DBNull.Value },
                                                { "PaymentTypeFeeCharged", DBNull.Value }
                                            },
                                            msgs: amsgs,
                                            dc: adc);

                    foreach (var transaction in oldTransactions)
                    {                        
                        decimal convenienceFeeCharged, paymentTypeFeeCharged = 0M;
                        var cost = transaction.Amount.IfEmpty(0);                        

                        if (cost <= 0 || transaction.PlatformFlatFee.IsEmpty() || transaction.PlatformPercentFee.IsEmpty())
                            continue;

                        var billingAccount = transaction.GetBillerAccount(amsgs, adc);

                        //.. calculate ConvenienceFeeCharged
                        if (billingAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc))
                        {
                            var percentageFee = billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFee, msgs, dc).GetMaxValue();
                            var flatFee = billingAccount.GetAccountPackageFeature(FeatureType.PaymentsPercentageFlatFee, msgs, dc).GetMaxValue();
                            convenienceFeeCharged = (cost * percentageFee) + flatFee;
                        }
                        else
                        {
                            convenienceFeeCharged = PlatformOldLogic.GetConvenienceFee(cost, billingAccount);
                        }

                        //.. calculate PaymentTypeFeeCharged
                        if (transaction.PaymentTypeId == PaymentType.CreditCard)
                        {
                            paymentTypeFeeCharged = PlatformOldLogic.GetCreditCardFee(cost, billingAccount);
                        }                        

                        transaction.ConvenienceFeeCharged = convenienceFeeCharged;
                        transaction.PaymentTypeFeeCharged = paymentTypeFeeCharged;

                        transaction.Save(adc);
                    }
                },
                uniqueIdenfier: "UpdateOldTransactionsFees_20210203",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
