using Microsoft.Data.SqlClient;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220907_AddIsGenderEnforced {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery {Connection = adc};
                    amsgs.AddRange(dq.ExecuteSql("alter table Activity add IsGenderEnforced bit set default 0"));
                },
                uniqueIdenfier: "Migration_20220907_AddIsGenderEnforced",
                msgs: msgs,
                dc: dc
            );
        }
    }
}