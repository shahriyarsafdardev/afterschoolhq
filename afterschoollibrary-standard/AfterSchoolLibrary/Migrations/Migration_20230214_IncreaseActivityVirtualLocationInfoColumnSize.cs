using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230214_IncreaseActivityVirtualLocationInfoColumnSize
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        connection.Execute(@"ALTER TABLE [Activity]
                                                ALTER COLUMN [VirtualLocationInfo] NVARCHAR(4000)");
                    }
                },
                uniqueIdenfier: "Migration_20230214_IncreaseActivityVirtualLocationInfoColumnSize",
                msgs: msgs,
                dc: dc
            );
        }
    }
}