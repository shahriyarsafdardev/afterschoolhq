using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateRequiresParentalConsent_20190721
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Activity.Update(
                        updateValues: new Hashtable() { { "RequiresParentalConsent", true } },
                        criterias: new Hashtable() { },
                        ObjectMap: Activity.ObjectMap,
                        dc: adc));
                },
                uniqueIdenfier: "UpdateRequiresParentalConsent-20190721",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
