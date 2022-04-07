using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220407_SetActivityRegistrantIsDeleted
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var allRegistrantActivities = Models.RegistrantActivity.Load();
                    var registrantActivitiesWithNullIsDeleted = allRegistrantActivities.Where(ra => !ra.IsDeleted.HasValue);
                    foreach (var registrantActivity in registrantActivitiesWithNullIsDeleted)
                    {
                        registrantActivity.IsDeleted = false;
                        registrantActivity.Save(adc);
                    }
                },
                uniqueIdenfier: "Migration_20220407_SetActivityRegistrantIsDeleted",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
