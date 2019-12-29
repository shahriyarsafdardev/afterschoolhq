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
        public static SystemMessages Do(DatabaseConnection dc)
        {
            return Migration.Run
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
                dc: dc
            );
        }
    }
}
