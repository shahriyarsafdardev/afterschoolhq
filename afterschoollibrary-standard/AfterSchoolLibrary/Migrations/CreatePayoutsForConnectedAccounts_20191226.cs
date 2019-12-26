using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class CreatePayoutsForConnectedAccounts_20191226
    {
        public static SystemMessages Do(DatabaseConnection dc)
        {
            return Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var loggedInUser = WebUser.Load(1, amsgs, adc);
                    var transactions = Transaction.GetUnmarkedConnectedAccountTransactions(amsgs, adc);

                    transactions.ForEach(transaction => {
                        // just creating then immediately marking as completed 
                        // because they have connected accounts and payouts are done immediately
                        transaction.CreatePayout(
                            payoutStatus: PayoutStatus.Completed,
                            paymentType: PaymentType.ACH,
                            loggedInUser: loggedInUser,
                            msgs: amsgs,
                            dc: adc);
                    });
                },
                uniqueIdenfier: "CreatePayoutsForConnectedAccounts_20191226",
                dc: dc
            );
        }
    }
}
