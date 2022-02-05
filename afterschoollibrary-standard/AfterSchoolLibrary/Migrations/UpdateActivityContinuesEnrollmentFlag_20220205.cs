using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateActivityContinuesEnrollmentFlag_20220205
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var query = "UPDATE Activity SET IsContinuousEnrollment = 0 WHERE IsContinuousEnrollment IS NULL";
                    var dq = Activity.get_DatabaseQuery(Activity.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(query));
                },
                uniqueIdenfier: "UpdateActivityContinuesEnrollmentFlag_20220205",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
