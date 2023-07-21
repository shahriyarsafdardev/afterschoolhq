using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    public class MigrationTest2
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var query = "UPDATE SubscriptionPlan SET IsTestColumn = 0 WHERE IsTestColumn IS NULL";
                    var dq = SubscriptionPlan.get_DatabaseQuery(Activity.ObjectMap, adc);
                    amsgs.AddRange(dq.ExecuteSql(query));
                },
                uniqueIdenfier: "MigrationTest2",
                msgs: msgs,
                dc: dc
            );
            if (msgs.HasErrors())
                msgs.Add("This error occured in MigrationTest2", MessageType.Error);
        }
    }
}
