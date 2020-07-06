using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class IncreaseStripeKeyColumnSizes_20200126
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var dq = new DatabaseQuery();
                    dq.ExecuteSql("alter table account alter column StripePublishableKey NVarChar(400)");
                    dq.ExecuteSql("alter table account alter column StripeAccessToken NVarChar(400)");
                    dq.ExecuteSql("alter table account alter column StripeRefreshToken NVarChar(400)");
                },
                uniqueIdenfier: "IncreaseStripeKeyColumnSizes_20200126-20200126",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
