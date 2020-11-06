using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateAccountOnboardingStepDone_20201106
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    Account.Update(updateValues: new Hashtable() { { "OnboardingStepDone", 8 } }, 
                        criterias: new Hashtable() { { "OnboardingStepDone", 7 } }, 
                        ObjectMap: Account.ObjectMap, 
                        dc: adc);   
                },
                uniqueIdenfier: "UpdateAccountOnboardingStepDone_20201106",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
