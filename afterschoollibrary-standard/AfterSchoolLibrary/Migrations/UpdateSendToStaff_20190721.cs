using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateSendToStaff_20190721
    {
        public static SystemMessages Do(DatabaseConnection dc)
        {
            return Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(ActivityMessage.Update(
                        updateValues: new Hashtable() { { "SendToStaff", true } },
                        criterias: new Hashtable() { },
                        ObjectMap: ActivityMessage.ObjectMap,
                        dc: adc));
                },
                uniqueIdenfier: "UpdateSendToStaff-20190721",
                dc: dc
            );
        }
    }
}
