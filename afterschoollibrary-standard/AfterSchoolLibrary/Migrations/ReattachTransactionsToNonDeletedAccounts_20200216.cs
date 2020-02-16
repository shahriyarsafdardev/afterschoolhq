using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    public class ReattachTransactionsToNonDeletedAccounts_20200216
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = Transaction.get_DatabaseQuery(Transaction.ObjectMap, typeof(Transaction).Name);
                    dq.Connection = adc;
                    var dt = dq.ExecuteSelect("select * from [Transaction] where PayerAccountId in (select accountid from accountwebuser where isdeleted = 1)", amsgs);
                    var transactions = dt.AsEnumerable().ToList().ConvertAll(Transaction.LoadObject).ToList();

                    transactions.ForEach(transaction => {

                        var deletedAccount = transaction.GetPayerAccount(amsgs, adc);
                        var payerUser = deletedAccount.GetAccountOwner(amsgs, adc);
                        var realParentAccount = payerUser.GetRegistrantAccount(amsgs, adc);
                        var purchase = transaction.GetPurchase(amsgs, adc);

                        if (realParentAccount.IsNotEmpty())
                        {
                            transaction.PayerAccountId = realParentAccount.ID.Value;
                            amsgs.AddRange(transaction.Save(adc));

                            purchase.PurchaserAccountId = realParentAccount.ID.Value;
                            amsgs.AddRange(purchase.Save(adc));
                        }

                    });
                },
                uniqueIdenfier: "ReattachTransactionsToNonDeletedAccounts_20200216",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
