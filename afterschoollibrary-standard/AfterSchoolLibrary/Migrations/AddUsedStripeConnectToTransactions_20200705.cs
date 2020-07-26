using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace AfterSchoolLibrary.Migrations
{
    public class AddUsedStripeConnectToTransactions_20200705
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dpvs = new DatabaseParameterValues();
                    var dcs = new DatabaseCriterias(LogicalOperatorType.And);

                    dpvs.Add(Transaction.ObjectMap["UsedStripeConnect"], 1);
                    dcs.Add(Transaction.ObjectMap["PlatformPercentFee"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);

                    amsgs.AddRange(Transaction.Update(dpvs, dcs, adc));
                },
                uniqueIdenfier: "AddUsedStripeConnectToTransactions_20200705",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
