using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20221225_AddFormsReportingFeatureFlag
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        connection.Execute(@"insert into FeatureFlag (FlagName, IsEnabled) values ('forms-reporting', 0)");
                    }
                },
                uniqueIdenfier: "Migration_20221225_AddFormsReportingFeatureFlag",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
