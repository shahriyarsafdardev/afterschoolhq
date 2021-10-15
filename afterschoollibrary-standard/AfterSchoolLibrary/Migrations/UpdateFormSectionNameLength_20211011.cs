using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateFormSectionNameLength_20211011
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql1 = "alter table FormQUestionGroup alter column Name NVarchar(400)";

                    var dq = FormQuestionGroup.get_DatabaseQuery(FormQuestionGroup.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(sql1));
                },
                uniqueIdenfier: "UpdateFormSectionNameLength_20211011",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
