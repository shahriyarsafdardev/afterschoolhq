using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class AddFieldTripsFeatureToSchools_20200126
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    Account.LoadByAccountTypeId(AccountType.School, amsgs, adc)
                        .ForEach(school => school.EnsureDefaultPackageFeatures(new WebUser { ID = 0 }, amsgs, adc));
                },
                uniqueIdenfier: "AddFieldTripsFeatureToSchools-20200126",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
