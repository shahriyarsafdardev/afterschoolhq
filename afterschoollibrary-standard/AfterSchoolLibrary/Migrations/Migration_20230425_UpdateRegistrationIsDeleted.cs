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
    public class Migration_20230425_UpdateRegistrationIsDeleted {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Registration.Update(
                        new Hashtable() {{"IsDeleted", 0}}, 
                        new Hashtable() {{"IsDeleted", DBNull.Value}}, 
                        Registration.ObjectMap, 
                        adc));
                },
                uniqueIdenfier: "Migration_20230425_UpdateRegistrationIsDeleted",
                msgs: msgs,
                dc: dc
            );
        }
    }
}