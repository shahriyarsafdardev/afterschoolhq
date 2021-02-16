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



                },
                uniqueIdenfier: "UpdateOldTransactions_20210216",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
