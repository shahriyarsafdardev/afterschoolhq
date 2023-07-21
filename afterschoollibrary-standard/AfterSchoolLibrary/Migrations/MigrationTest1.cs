using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    public class MigrationTest1
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var query = "UPDATE Activity SET IsTesting = 0 WHERE IsTesting IS NULL";
                    var dq = Activity.get_DatabaseQuery(Activity.ObjectMap, adc);
                    amsgs.AddRange(dq.ExecuteSql(query));
                },
                uniqueIdenfier: "MigrationTest1",
                msgs: msgs,
                dc: dc
            );
            if(msgs.HasErrors())
                msgs.Add("This error occured in MigrationTest1", MessageType.Error);
        }
    }
}
