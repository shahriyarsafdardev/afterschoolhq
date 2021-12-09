using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class SetExistingActivityCostFrequencyType_20211003
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Activity.Update(
                        new Hashtable() { { "CostFrequencyTypeId", (int)ActivityCostFrequencyType.PerActivity } },
                        new Hashtable() { },
                        Activity.ObjectMap,
                        adc));
                },
                uniqueIdenfier: "SetExistingActivityCostFrequencyType_20211003",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
