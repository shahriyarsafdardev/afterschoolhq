using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class SetExistingAccountsToOnboarded_20200724
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Account.Update(
                        new Hashtable() { { "OnboardingStepDone", (int)OnboardingStepType.All } }, 
                        new Hashtable() { { "IsClaimed", true } }, 
                        Account.ObjectMap,
                        adc));
                },
                uniqueIdenfier: "SetExistingAccountsToOnboarded_20200724",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
