using System;
using System.Collections;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230213_UpdateRegistrationIsDeletedColumn
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Registration.Update(
                        new Hashtable() { { "IsDeleted", 0 } },
                        new Hashtable() { { "IsDeleted", DBNull.Value } },
                        Registration.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20230213_UpdateRegistrationIsDeletedColumn",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
