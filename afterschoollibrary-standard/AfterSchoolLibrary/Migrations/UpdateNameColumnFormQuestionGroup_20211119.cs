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
                    var sql1 = "alter table FormQuestionGroup alter column Name NVarchar(400)";

                    var dq = FormQuestionGroup.get_DatabaseQuery(FormQuestionGroup.ObjectMap);
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql(sql1));
                },
                uniqueIdenfier: "UpdateNameColumnFormQuestionGroup_20211119",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
