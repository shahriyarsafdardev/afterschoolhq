using System;

namespace AfterSchool.Areas.Api.Models
{
    public class ActivityDto
    {
        public string StudentUid { get; set; }
        public string ActivityUid { get; set; }
        public string ActivityName { get; set; }
        public string ActivityStartTime { get; set; }
        public string ActivityEndTime { get; set; }
        public CheckInCheckOutSetupDto  CheckSetupDto{ get; set; }
    }
}