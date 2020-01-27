using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateRegistrantAccess_20190721
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc = null)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var registrants = Registrant.Load(
                        criterias: new Hashtable() { { "IsMaster", true } },
                        msgs: amsgs,
                        dc: adc);

                    registrants.ForEach(registrant =>
                    {
                        var user = new WebUser() { ID = registrant.WebUserId };

                        if (!registrant.WebUserId.HasValue)
                            user = registrant.GetAccount(amsgs, adc).GetAccountOwner(amsgs, adc);

                        registrant.AddUserWithAccess(
                                user: user,
                                loggedInUser: null,
                                msgs: amsgs,
                                dc: adc);
                    });
                },
                uniqueIdenfier: "UpdateRegistrantAccess-20190721",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
