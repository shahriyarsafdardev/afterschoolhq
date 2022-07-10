using System.Collections;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220709_AddPersonTypeToRegistrant 
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(Registrant.Update(
                        new Hashtable() { { "PersonTypeId", (int)RegistrantPersonType.Student } },
                        new Hashtable() { { "IsParentOrGuardian", 0 } },
                        Registrant.ObjectMap,
                        adc
                    ));

                    amsgs.AddRange(Registrant.Update(
                        new Hashtable() { { "PersonTypeId", (int)RegistrantPersonType.Parent } },
                        new Hashtable() { { "IsParentOrGuardian", 1 } },
                        Registrant.ObjectMap,
                        adc
                    ));
                },
                uniqueIdenfier: "Migration_20220709_AddPersonTypeToRegistrant",
                msgs: msgs,
                dc: dc
            );
        }
    }
}