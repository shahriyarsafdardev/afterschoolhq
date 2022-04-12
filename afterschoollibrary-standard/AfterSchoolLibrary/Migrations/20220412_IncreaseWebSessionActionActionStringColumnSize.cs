using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220412_IncreaseWebSessionActionActionStringColumnSize
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        connection.Execute(@"ALTER TABLE [WebSessionAction]
                                                ALTER COLUMN [ActionString] NVARCHAR(1000)");
                    }
                },
                uniqueIdenfier: "Migration_20220412_IncreaseWebSessionActionActionStringColumnSize",
                msgs: msgs,
                dc: dc
            );
        }
    }
}