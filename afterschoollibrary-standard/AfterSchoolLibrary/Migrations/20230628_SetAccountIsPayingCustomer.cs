using System.Collections;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;
using System;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230628_SetAccountIsPayingCustomer
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Account.Update(
                        new Hashtable() { { "IsPayingCustomer", 0 } },
                        new Hashtable() { { "IsPayingCustomer", DBNull.Value } },
                        Account.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20230628_SetAccountIsPayingCustomer",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
