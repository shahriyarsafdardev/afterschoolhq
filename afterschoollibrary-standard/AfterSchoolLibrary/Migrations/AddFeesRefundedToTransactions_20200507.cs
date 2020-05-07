using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class AddFeesRefundedToTransactions_20200507
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var loggedInUser = WebUser.Load(1, amsgs, adc);
                    var refunds = Transaction.Load(
                                            new Hashtable() { { "TransactionTypeId", (int)TransactionType.Refund } }, 
                                            msgs: amsgs, 
                                            dc: adc);

                    refunds.ForEach(refund => {

                        var originalTransaction = refund.GetOriginalTransaction(amsgs, adc);

                        if (originalTransaction.IsNotEmpty())
                        {
                            if (refund.Amount == originalTransaction.Amount)
                            {
                                refund.ConvenienceFeeCharged = originalTransaction.ConvenienceFeeCharged;
                                refund.PaymentTypeFeeCharged = originalTransaction.PaymentTypeFeeCharged;
                                amsgs.AddRange(refund.Save(adc));
                            }
                            else if (refund.Amount == originalTransaction.Amount - originalTransaction.ConvenienceFeeCharged)
                            {
                                refund.ConvenienceFeeCharged = 0;
                                refund.PaymentTypeFeeCharged = originalTransaction.PaymentTypeFeeCharged;
                                amsgs.AddRange(refund.Save(adc));
                            }
                            else if (refund.Amount == originalTransaction.Amount - originalTransaction.PaymentTypeFeeCharged)
                            {
                                refund.ConvenienceFeeCharged = originalTransaction.ConvenienceFeeCharged;
                                refund.PaymentTypeFeeCharged = 0;
                                amsgs.AddRange(refund.Save(adc));
                            }
                        }
                    });
                },
                uniqueIdenfier: "AddFeesRefundedToTransactions_20200507",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
