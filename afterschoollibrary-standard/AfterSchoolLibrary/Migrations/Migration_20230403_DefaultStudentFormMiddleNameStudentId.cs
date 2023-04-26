using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230403_DefaultStudentFormMiddleNameStudentId
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    FormService.AddNewAfterSchoolDefaultStudentForm(amsgs, adc);
                },
                uniqueIdenfier: "Migration_20230403_DefaultStudentFormMiddleNameStudentId",
                msgs: msgs,
                dc: dc
            );
        }
    }
}