using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using AfterSchoolLibrary.Enums;
using System.Collections;
using System;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20221208_AddActivityMessageParentsOnly
    {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(ActivityMessage.Update(
                        new Hashtable() { { "ParentsOnly", 0 } },
                        new Hashtable() { { "ParentsOnly", DBNull.Value } },
                        Registrant.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20221208_AddActivityMessageParentsOnly",
                msgs: msgs,
                dc: dc
            );
        }
    }
}