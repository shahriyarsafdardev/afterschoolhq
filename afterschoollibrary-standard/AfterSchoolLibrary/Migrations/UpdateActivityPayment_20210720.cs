using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateActivityPayment_20210720
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // Loop through All Activity Payments
                    ActivityPayment.Load(msgs: amsgs, dc: adc).ForEach(activityPayment =>
                    {
                        if (activityPayment.TransactionId.HasValue)
                        {
                            var transaction = Transaction.Load(activityPayment.TransactionId.Value, amsgs, adc);

                            // Amount -> Amount Provider Received
                            activityPayment.Amount = transaction.GetBillerAmount(amsgs, adc);

                            // AmountPaidByParent -> Transaction Amount
                            activityPayment.AmountPaidByParent = transaction.Amount.Value;

                            // IsProRated = false
                            activityPayment.IsProRated = false;

                            amsgs.AddRange(activityPayment.Save(adc));
                        }

                    });
                },
                uniqueIdenfier: "UpdateActivityPayment_20210720-1",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
