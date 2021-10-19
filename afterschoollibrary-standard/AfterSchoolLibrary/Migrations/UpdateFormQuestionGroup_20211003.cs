using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateFormQuestionGroup_20211003
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var formGroups = FormQuestionGroup.Load(msgs: amsgs, dc: adc);

                    foreach (var item in formGroups)
                    {
                        item.IsActive = true;
                        amsgs.AddRange(item.Save(dc: adc));
                    }
                },
                uniqueIdenfier: "UpdateFormQuestionGroup_20211003",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
