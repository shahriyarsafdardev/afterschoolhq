using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateAccountPropertyValueForJobs_20210226
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    AccountProperty.Update(updateValues: new Hashtable() { { "value", "Youth Employment" } }, 
                        criterias: new Hashtable() { { "value", "Jobs" } }, 
                        ObjectMap: AccountProperty.ObjectMap, 
                        dc: adc);   
                },
                uniqueIdenfier: "UpdateAccountPropertyValueForJobs_20210226",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
