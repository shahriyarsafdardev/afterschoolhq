using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateWebSessionAction_20210513
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
                        if (wba.CreatedBy.IsEmpty() && wba.ActorId.IsNotEmpty())
                        {
                            wba.CreatedBy = wba.ActorId;
                            amsgs.AddRange(wba.Save(adc));
                        }
                    });
                },
                uniqueIdenfier: "UpdateWebSessionAction_20210513",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
