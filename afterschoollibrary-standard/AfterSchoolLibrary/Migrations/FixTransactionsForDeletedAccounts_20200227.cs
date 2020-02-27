using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class FixTransactionsForDeletedAccounts_20200227
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = Transaction.get_DatabaseQuery(Transaction.ObjectMap, typeof(Transaction).Name);
                    dq.Connection = adc;
                    var dt = dq.ExecuteSelect("select * from [Transaction] where PayerAccountId != ConvenienceFeePayerAccountId and BillerAccountId != ConvenienceFeePayerAccountId", amsgs);
                    var transactions = dt.AsEnumerable().ToList().ConvertAll(Transaction.LoadObject).ToList();

                    transactions.ForEach(transaction => {
                        transaction.ConvenienceFeePayerAccountId = transaction.PayerAccountId;
                        transaction.PaymentTypeFeePayerAccountId = transaction.PayerAccountId;
                        amsgs.AddRange(transaction.Save(adc));
                    });
                },
                uniqueIdenfier: "FixTransactionsForDeletedAccounts_20200227",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
