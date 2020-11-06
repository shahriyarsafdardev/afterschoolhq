using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateOnboardingStep_20201105
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var accounts = Account.Load(msgs: amsgs, dc: adc);

                    accounts.ForEach(account =>
                    {
                        if ((int)account.OnboardingStepDone == 7)
                        {
                            account.OnboardingStepDone = OnboardingStepType.All;
                            amsgs.AddRange(account.Save(adc));
                        }
                            
                    });

                },
                uniqueIdenfier: "UpdateOnboardingStep_20201105",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
