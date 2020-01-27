using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateRegistrantStatuses_20190721
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Registrant.Update(
                        updateValues: new Hashtable() { { "ProfileStatusId", (int)RegistrantProfileStatus.Completed } },
                        criterias: new Hashtable()
                        {
                            { "IsMaster", true },
                            { "ProfileStatusId", 0 }
                        },
                        ObjectMap: Registrant.ObjectMap,
                        dc: adc));
                },
                uniqueIdenfier: "UpdateRegistrantStatuses-20190721",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
