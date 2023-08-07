using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230622_UpdateSubscriptionPlanEndDate
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql1 = "UPDATE SubscriptionPlan SET EndDate = ActivitySchedule.EndDate " +
                    "FROM SubscriptionPlanINNER JOIN Activity ON SubscriptionPlan.ParticipationObjectId = Activity.ID " +
                    "INNER JOIN ActivitySchedule ON Activity.Name = ActivitySchedule.Name " +
                    "WHERE SubscriptionPlan.EndDate IS NULL;";

                    var dq = SubscriptionPlan.get_DatabaseQuery(SubscriptionPlan.ObjectMap);
                    amsgs.AddRange(dq.ExecuteSql(sql1));
                },
                uniqueIdenfier: "Migration_20230622_UpdateSubscriptionPlanEndDate",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
