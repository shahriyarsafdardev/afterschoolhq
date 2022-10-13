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
    public class Migration_20221013_AddIsDeletedWebUser {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (amsgs, adc) =>
                {
                    var dq = new DatabaseQuery{Connection = adc};
                    amsgs.AddRange(dq.ExecuteSql("Alter Table WebUser Add IsDeleted bit not null default  0"));
                },
                uniqueIdenfier: "Migration_20221013_AddIsDeletedWebUser",
                msgs: msgs,
                dc: dc
            );
        }
    }
}