using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20231019_AlterAccountPropertyValue
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery { Connection = adc };
                    amsgs.AddRange(dq.ExecuteSql("ALTER FULLTEXT INDEX ON [AccountProperty] SET STOPLIST OFF" +
                        "ALTER TABLE [AccountProperty] alter column Value nvarchar(max) null" +
                        "ALTER FULLTEXT INDEX ON [AccountProperty] SET STOPLIST SYSTEM"));
                },
                uniqueIdenfier: "Migration_20231019_AlterAccountPropertyValue",
                msgs: msgs,
                dc: dc
            );
        }
    }
}