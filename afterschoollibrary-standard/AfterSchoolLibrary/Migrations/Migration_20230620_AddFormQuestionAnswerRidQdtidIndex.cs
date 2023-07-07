using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230620_AddFormQuestionAnswerRidQdtidIndex
    {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    amsgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_RID_QDTID ON [FormQuestionAnswer] (RegistrationId, QuestionDataTypeId)"));

                },
                uniqueIdenfier: "Migration_20230620_AddFormQuestionAnswerRidQdtidIndex",
                msgs: msgs,
                dc: dc
            );
        }
    }
}