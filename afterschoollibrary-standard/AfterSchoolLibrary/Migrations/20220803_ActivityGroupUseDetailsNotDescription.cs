using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220803_ActivityGroupUseDetailsNotDescription {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql("update ActivityGroup set Details = Description where Details is null and Description Is Not NULL"));
                    amsgs.AddRange(dq.ExecuteSql("update ActivityGroup set Description = NULL where Details IS NOT null and Details = Description"));
                },
                uniqueIdenfier: "Migration_20220803_ActivityGroupUseDetailsNotDescription",
                msgs: msgs,
                dc: dc
            );
        }
    }
}