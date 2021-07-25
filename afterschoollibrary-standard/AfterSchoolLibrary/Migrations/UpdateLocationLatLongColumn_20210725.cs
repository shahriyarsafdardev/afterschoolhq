using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    class UpdateLocationLatLongColumn_20210725
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql1 = "alter table Location alter column Latitude Decimal(30,5)"; 
                    var sql2 = "alter table Location alter column Longitude Decimal(30,5)";
                    var sql3 = "alter table Location alter column DistanceFromOrigin Decimal(30,5)";

                    var dq = Location.get_DatabaseQuery(Location.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(sql1));
                    amsgs.AddRange(dq.ExecuteSql(sql2));
                    amsgs.AddRange(dq.ExecuteSql(sql3));

                },
                uniqueIdenfier: "UpdateLocationLatLongColumn_20210725",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
