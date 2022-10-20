using System;
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
    public class Migration_20221013_UpdateNullValuesIsGenderEnforced {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Activity.Update(
                        new Hashtable() {{"IsGenderEnforced", 0}}, 
                        new Hashtable() {{"IsGenderEnforced", DBNull.Value}}, 
                        Activity.ObjectMap, 
                        adc));
                },
                uniqueIdenfier: "Migration_20221013_UpdateNullValuesIsGenderEnforced",
                msgs: msgs,
                dc: dc
            );
        }
    }
}