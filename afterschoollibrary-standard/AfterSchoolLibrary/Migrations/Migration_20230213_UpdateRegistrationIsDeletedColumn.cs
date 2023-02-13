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
                    var query = "UPDATE Registration SET IsDeleted = 0 WHERE IsDeleted IS NULL";
                    var dq = Activity.get_DatabaseQuery(Activity.ObjectMap, adc);
                    amsgs.AddRange(dq.ExecuteSql(query));
                },
                uniqueIdenfier: "Migration_20230213_UpdateRegistrationIsDeletedColumn",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
