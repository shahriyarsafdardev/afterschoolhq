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
                    amsgs.AddRange(WebSessionAction.Update(
                        updateValues: new Hashtable() { { "IndirectObjectTypeId", 0 } },
                        criterias: new Hashtable() { { "IndirectObjectTypeId", DBNull.Value } },
                        ObjectMap: WebSessionAction.ObjectMap,
                        dc: adc));
                },
                uniqueIdenfier: "UpdateWebSessionActionIndirectObjectType_20210517",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
