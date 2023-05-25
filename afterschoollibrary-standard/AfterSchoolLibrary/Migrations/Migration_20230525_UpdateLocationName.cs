using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230525_UpdateLocationName
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql1 = "alter table Location alter COLUMN Name NVARCHAR(400)";
                    
                    var dq = Location.get_DatabaseQuery(Location.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(sql1));
                },
                uniqueIdenfier: "Migration_20230525_UpdateLocationName",
                msgs: msgs,
                dc: dc
            );
        }
    }
}