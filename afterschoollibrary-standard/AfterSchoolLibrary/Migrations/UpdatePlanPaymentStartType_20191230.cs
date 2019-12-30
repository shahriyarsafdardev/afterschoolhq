using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdatePlanPaymentStartType_20191230
    {
        public static SystemMessages Do(DatabaseConnection dc)
        {
            return Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(SubscriptionPlan.Update(
                       updateValues: new Hashtable() { { "PaymentStartTypeId", (int)PlanPaymentStartType.WhenActivityStarts } },
                       criterias: new Hashtable() { },
                       ObjectMap: SubscriptionPlan.ObjectMap,
                       dc: adc));
                },
                uniqueIdenfier: "UpdatePlanPaymentStartType_20191230",
                dc: dc
            );
        }
    }
}
