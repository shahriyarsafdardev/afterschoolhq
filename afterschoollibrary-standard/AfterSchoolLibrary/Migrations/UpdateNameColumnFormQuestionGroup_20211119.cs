using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateNameColumnFormQuestionGroup_20211119
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql = "alter table FormQuestionGroup alter column Name nvarchar(250)";

                    var dq = Location.get_DatabaseQuery(Location.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(sql));
                },
                uniqueIdenfier: "UpdateNameColumnFormQuestionGroup_20211119",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
