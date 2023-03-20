
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230320_LockDOBAndGender
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql(@"update fq set fq.IsRequired = 1 , fq.IsLocked = 1
                    from FormQuestion fq
                        inner join Form f on fq.FormId = f.ID
                    where QuestionDataTypeId in (4 , 45) and f.FormTypeId = 1"));
                },
                uniqueIdenfier: "Migration_20230320_LockDOBAndGender",
                msgs: msgs,
                dc: dc
            );
        }
    }
}