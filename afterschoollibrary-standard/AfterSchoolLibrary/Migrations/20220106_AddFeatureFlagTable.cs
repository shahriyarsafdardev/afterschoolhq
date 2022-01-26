using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220106_AddFeatureFlagTable {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        connection.Execute(@"if object_id('FeatureFlag', 'U') is null
                                                    create table FeatureFlag (FlagName nvarchar(50), IsEnabled bit)");
                    }
                },
                uniqueIdenfier: "Migration_20220106_AddFeatureFlagTable",
                msgs: msgs,
                dc: dc
            );
        }
    }
}