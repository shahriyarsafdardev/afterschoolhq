using System;

namespace AfterSchool.Areas.Api.Models
{
    public class CheckInCheckOutStudentInfoDto
    {
        public string StudentName { get; set; }
        public string ActivityName { get; set; }
        public string ActivityStartTime { get; set; }
        public string ActivityEndTime { get; set; }
    }
}