using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20221019_AddBeamerProviderFeatureFlag
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        connection.Execute(@"insert into FeatureFlag (FlagName, IsEnabled) values ('beamer_provider', 0)");
                    }
                },
                uniqueIdenfier: "Migration_20221019_AddBoomerProviderFeatureFlag",
                msgs: msgs,
                dc: dc
            );
        }
    }
}