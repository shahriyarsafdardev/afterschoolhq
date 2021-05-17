using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateWebSessionActionIndirectObjectType_20210517
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var webSessionActions = WebSessionAction.Load(null, null, null, msgs: amsgs, dc: adc);
                    webSessionActions.ForEach(wba =>
                    {
                        if (wba.IndirectObjectTypeId.IsEmpty())
                        {
                            wba.IndirectObjectTypeId = ObjectType.NotSet;
                            amsgs.AddRange(wba.Save(dc: adc));
                        }
                    });
                },
                uniqueIdenfier: "UpdateWebSessionActionIndirectObjectType_20210517",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
