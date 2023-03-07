using System;
using System.Collections;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230215_UpdateActivityIsFeaturedColumn
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Activity.Update(
                        new Hashtable() { { "IsFeatured", 0 } },
                        new Hashtable() { { "IsFeatured", DBNull.Value } },
                        Activity.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20230215_UpdateActivityIsFeaturedColumn",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
