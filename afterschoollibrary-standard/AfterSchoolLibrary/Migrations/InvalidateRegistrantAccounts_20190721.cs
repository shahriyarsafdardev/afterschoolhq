using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class InvalidateRegistrantAccounts_20190721
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var registrants = Registrant.LoadMastersNotRecentlyUpdated("7/1/2019".ToDateTime(), amsgs, adc);

                    registrants.ForEach(registrant =>
                    {
                        registrant.UpdateProfileStatus(
                            RegistrantProfileStatus.OutOfDate,
                            new WebUser() { ID = 0 },
                            amsgs,
                            adc);
                    });
                },
                uniqueIdenfier: "InvalidateRegistrantAccounts-20190721",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
