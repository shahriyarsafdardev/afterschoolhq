using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using System.Collections.Generic;

namespace AfterSchool.Models
{
    public class AttendanceSummaryResult
    {
        public Account CurrentAccount { get; set; }
        public WebUser CurrentUser { get; set; }
        public SystemMessages Messages { get; set; }
        public List<ActivityAttendanceSummary> ActivityAttendanceSummaries { get; set; }
    }

}