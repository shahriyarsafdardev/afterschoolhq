using Microsoft.Data.SqlClient;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220824_AlterActivityAlterDetails {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery {Connection = adc};
                    amsgs.AddRange(dq.ExecuteSql("alter table Activity alter column Details nvarchar(max)"));
                },
                uniqueIdenfier: "Migration_20220824_AlterActivityAlterDetails",
                msgs: msgs,
                dc: dc
            );
        }
    }
}