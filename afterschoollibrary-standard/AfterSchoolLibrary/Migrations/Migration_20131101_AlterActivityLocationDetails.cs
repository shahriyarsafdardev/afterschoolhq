using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20131101_AlterActivityLocationDetails
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery { Connection = adc };
                    amsgs.AddRange(dq.ExecuteSql("alter table Activity alter column LocationDetails nvarchar(max) null"));
                },
                uniqueIdenfier: "Migration_20131101_AlterActivityLocationDetails",
                msgs: msgs,
                dc: dc
            );
        }
    }
}