using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using static AfterSchoolLibrary.Helpers.GoogleAutocomplete;
using Location = AfterSchoolLibrary.Models.Location;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220323_1_LatitudeLongitudeAlterColumns
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql1 = "alter table Location alter column Latitude Decimal(12,7)";
                    var sql2 = "alter table Location alter column Longitude Decimal(12,7)";
                    var sql3 = "alter table Location alter column DistanceFromOrigin Decimal(10,5)";

                    var dq = Location.get_DatabaseQuery(Location.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(sql1));
                    amsgs.AddRange(dq.ExecuteSql(sql2));
                    amsgs.AddRange(dq.ExecuteSql(sql3));
                },
                uniqueIdenfier: "Migration_20220323_1_LatitudeLongitudeAlterColumns",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
