using System.Collections.Generic;

namespace AfterSchool.Areas.Api.Models
{
    public class CheckInCheckOutStudentInfoDto
    {
        public string StudentUid { get; set; }
        public string StudentName { get; set; }
        public List<ActivityDto> Activities { get; set; }
    }
}