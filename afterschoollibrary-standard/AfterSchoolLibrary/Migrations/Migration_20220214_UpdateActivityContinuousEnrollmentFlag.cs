using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220214_UpdateActivityContinuousEnrollmentFlag
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var query = "UPDATE Activity SET IsContinuousEnrollment = 0 WHERE IsContinuousEnrollment IS NULL";
                    var dq = Activity.get_DatabaseQuery(Activity.ObjectMap, adc);
                    amsgs.AddRange(dq.ExecuteSql(query));
                },
                uniqueIdenfier: "Migration_20220214_UpdateActivityContinuousEnrollmentFlag",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
