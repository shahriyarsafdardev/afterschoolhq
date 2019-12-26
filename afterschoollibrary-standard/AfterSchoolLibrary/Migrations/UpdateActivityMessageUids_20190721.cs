using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateActivityMessageUids_20190721
    {
        public static SystemMessages Do(DatabaseConnection dc)
        {
            return Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var ams = ActivityMessage.Load(
                        criterias: new Hashtable() { { "Uid", DBNull.Value } },
                        msgs: amsgs,
                        dc: adc);

                    ams.ForEach(am =>
                    {
                        am.Uid = Guid.NewGuid();
                        amsgs.AddRange(am.Save(adc));
                    });
                },
                uniqueIdenfier: "UpdateActivityMessageUids-20190721",
                dc: dc
            );
        }
    }
}
