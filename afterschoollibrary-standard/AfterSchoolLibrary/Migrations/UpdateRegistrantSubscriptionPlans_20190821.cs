using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateRegistrantSubscriptionPlans_20190821
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var rsps = RegistrantSubscriptionPlan.Load(
                                criterias: new Hashtable() { { "PaymentTypeId", 0 } },
                                msgs: amsgs,
                                dc: adc);

                    rsps.ForEach(rsp =>
                    {
                        rsp.PaymentTypeId = PaymentType.CreditCard;
                        amsgs.AddRange(rsp.Save(adc));
                    });
                },
                uniqueIdenfier: "UpdateRegistrantSubscriptionPlans-20190821",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
