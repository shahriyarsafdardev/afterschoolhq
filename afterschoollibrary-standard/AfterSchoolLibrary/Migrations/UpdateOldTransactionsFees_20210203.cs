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
                        //.. calculate ConvenienceFeeCharged
                        decimal convenienceFeeCharged, paymentTypeFeeCharged, fee = 0M;
                        var cost = transaction.Amount.IfEmpty(0);

                        if (cost <= 0)
                            continue;

                        var conveniencePercentFee = transaction.PlatformPercentFee.IfEmpty(0);
                        var convenienceFlatFee = transaction.PlatformFlatFee.IfEmpty(0);

                        //.. since the platform fees were always taken out of the cost
                        fee = (cost * conveniencePercentFee) + convenienceFlatFee;

                        var finalFee = Math.Ceiling(fee * 2) / 2;

                        if (cost > 50)
                            finalFee = Math.Ceiling(fee);

                        convenienceFeeCharged = Math.Max(finalFee, 0.50M) + (Math.Ceiling(cost) - cost);

                        //.. calculate PaymentTypeFeeCharged
                        //.. since the platform fees were always taken out of the cost
                        var stripeFee = StripeHelper.GetStripeFee(cost, true);

                        // always round up to nearest dollar or .5
                        // result should always be 3.50 or 4.00 or 2.50 or 3.00, etc.
                        if (cost <= 50)
                            paymentTypeFeeCharged = (Math.Ceiling(stripeFee * 2) / 2);
                        else
                            paymentTypeFeeCharged = Math.Ceiling(stripeFee);

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
