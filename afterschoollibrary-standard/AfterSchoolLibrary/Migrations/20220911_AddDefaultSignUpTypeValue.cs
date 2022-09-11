using System.Collections;
using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220911_AddDefaultSignUpTypeValue {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Account.Update(new Hashtable() {{"SignUpTypeId", (int) SignUpType.NotSet}}, new Hashtable(), Account.ObjectMap, adc));
                },
                uniqueIdenfier: "Migration_20220911_AddDefaultSignUpTypeValue",
                msgs: msgs,
                dc: dc
            );
        }
    }
}