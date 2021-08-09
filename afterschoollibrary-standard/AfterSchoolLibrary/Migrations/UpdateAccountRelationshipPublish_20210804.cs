using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateAccountRelationshipPublish_20210804
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(AccountRelationship.Update(
                        new Hashtable() { { "RequiresPublishPermission", true } }, 
                        new Hashtable(), 
                        AccountRelationship.ObjectMap, 
                        adc));
                },
                uniqueIdenfier: "UpdateAccountRelationshipPublish_20210804",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
