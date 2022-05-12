using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;
using Xunit;

namespace AfterSchoolLibraryTests.AdHocTests
{
    public class EmailCsvReportDebugging
    {
        [Fact(Skip = "For report debugging purposes")]
        public void RunBelleChasseReport()
        {
            var dc = new DatabaseConnection(new DatabaseCredentials(
                "NEEDS HOSTNAME",
                "AfterSchool",
                "NEEDS USERNAME",
                "NEEDS PASSWORD").ConnectionString);
            //["1148",null,null,"[3494,3495,3496,3497,3498,3499,3500,3501]","1","\"Registrations\"","4",null,"0","\"Activity\"","\"\"",null,null,null,null]

            RegistrationService.EmailCsvReport(1148, null, null, new List<int> { 3494, 3495, 3496, 3497, 3498, 3499, 3500, 3501 },
                ParticipationObjectType.Activity, "Registrations", RegistrationReportType.Dismissal, null, RegistrationStatus.NotSet,
                "Activity", "", null, null, null, dc);
        }

    }
}