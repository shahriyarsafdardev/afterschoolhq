using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;

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
                        item.Save(dc: adc);
                    }

                    //amsgs.AddRange(FormQuestionGroup.Update(updateValues: new Hashtable() { { "IsActive", true } },
                    //    criterias: new Hashtable() { },
                    //    ObjectMap: Account.ObjectMap,
                    //    dc: adc));
                },
                uniqueIdenfier: "UpdateFormQuestionGroup_20211003",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
