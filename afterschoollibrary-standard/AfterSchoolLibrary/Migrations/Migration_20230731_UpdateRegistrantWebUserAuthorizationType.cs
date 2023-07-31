using System;
using System.Collections;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230731_UpdateRegistrantWebUserAuthorizationType
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(RegistrantWebUser.Update(
                        new Hashtable() {{"AuthorizationTypeId", (int) WebUserAuthorizationType.FullyAuthorized}},
                        new Hashtable() {{"AuthorizationTypeId", DBNull.Value}},
                        RegistrantWebUser.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20230731_UpdateRegistrantWebUserAuthorizationType",
                msgs: msgs,
                dc: dc
            );
        }
    }
}


