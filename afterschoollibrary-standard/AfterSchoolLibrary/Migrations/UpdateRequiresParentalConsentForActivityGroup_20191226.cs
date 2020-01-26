using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateRequiresParentalConsentForActivityGroup_20191226
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(ActivityGroup.Update(
                       updateValues: new Hashtable() { { "RequiresParentalConsent", true } },
                       criterias: new Hashtable() { },
                       ObjectMap: ActivityGroup.ObjectMap,
                       dc: adc));
                },
                uniqueIdenfier: "UpdateRequiresParentalConsentForActivityGroup_20191226",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
