using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateScheduleDaysOfTheWeek_20211016
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // Loop through All Activity Payments
                    var schedules = ActivitySchedule.Load(null, null, null, amsgs, adc);

                    schedules.ForEach(schedule => 
                    {
                        if (schedule.DaysOfTheWeek.IsEmpty())
                        {
                            var instanceCount = ActivityInstance.GetCountByActivityScheduleId(schedule.ID.Value, amsgs, adc);

                            if (instanceCount > 0)
                            {
                                schedule.DaysOfTheWeek = schedule.GetDaysOfTheWeek(amsgs, adc).Select(x => (int)x).ToStrings().ToStringList(",");
                                amsgs.AddRange(schedule.Save(adc));
                            }
                        }
                    });
                },
                uniqueIdenfier: "UpdateScheduleDaysOfTheWeek_20211016",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
