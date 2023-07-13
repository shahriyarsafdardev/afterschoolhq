using System;

namespace AfterSchool.Areas.Api.Models
{
    public class CheckInCheckOutStudentInfoDto
    {
        public string StudentName { get; set; }
        public string ActivityName { get; set; }
        public DateTime? ActivityStartTime { get; set; }
        public DateTime? ActivityEndTime { get; set; }
    }
}