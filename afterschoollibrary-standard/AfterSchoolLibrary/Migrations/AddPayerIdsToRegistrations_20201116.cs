using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class AddPayerIdsToRegistrations_20201116
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var registrationCount = Registration.GetCount(msgs: amsgs, dc: adc);
                    var registrations = Registration.Load(
                                            start: Math.Max(0, registrationCount - 500), // don't need to get the whole table
                                            length: 500,
                                            msgs: amsgs,
                                            dc: adc);

                    registrations
                        .Where(registration => registration.CostPerClass.HasValue)
                        .ToList()
                        .ForEach(registration => {

                            var masterRegistrant = registration.GetMasterRegistrant(amsgs, adc);

                            if (masterRegistrant.IsNotEmpty())
                            {
                                registration.ConvenienceFeePayerAccountId = masterRegistrant.AccountId;
                                registration.PaymentTypeFeePayerAccountId = masterRegistrant.AccountId;
                                amsgs.AddRange(registration.Save(adc));
                            }

                        });
                },
                uniqueIdenfier: "AddPayerIdsToRegistrations_20201116",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
