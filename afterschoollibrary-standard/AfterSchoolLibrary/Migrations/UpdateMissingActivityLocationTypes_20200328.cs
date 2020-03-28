using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateMissingActivityLocationTypes_20200328
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Activity.Update(
                        new System.Collections.Hashtable() { { "ActivityLocationTypeId", (int)ActivityLocationType.Physical } },
                        new System.Collections.Hashtable() { { "ActivityLocationTypeId", 0 } },
                        Activity.ObjectMap,
                        adc));
                },
                uniqueIdenfier: "AddFieldTripsFeatureToSchools-20200126",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
