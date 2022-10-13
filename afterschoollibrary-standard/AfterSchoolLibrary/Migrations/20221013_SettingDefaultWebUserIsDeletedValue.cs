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
    public class Migration_20221013_SettingDefaultWebUserIsDeletedValue {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(WebUser.Update(
                        new Hashtable() {{"IsDeleted", 0}}, 
                        new Hashtable() {{"IsDeleted", DBNull.Value}}, 
                        WebUser.ObjectMap, 
                        adc));
                },
                uniqueIdenfier: "Migration_20221013_SettingDefaultWebUserIsDeletedValue",
                msgs: msgs,
                dc: dc
            );
        }
    }
}