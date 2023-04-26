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
    public class Migration_20230101_AddFormIsDeletedDefault
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Form.Update(
                        new Hashtable() { { "IsDeleted", 0 } },
                        new Hashtable() { { "IsDeleted", DBNull.Value } },
                        Form.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20230101_AddFormIsDeletedDefault",
                msgs: msgs,
                dc: dc
            );
        }
    }
}