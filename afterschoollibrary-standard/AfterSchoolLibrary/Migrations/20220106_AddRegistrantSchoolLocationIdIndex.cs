using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220106_AddRegistrantSchoolLocationIdIndex {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_SchoolLocationId ON [Registrant] (SchoolLocationId)"));

                },
                uniqueIdenfier: "Migration_20220106_AddRegistrantSchoolLocationIdIndex",
                msgs: msgs,
                dc: dc
            );
        }
    }
}