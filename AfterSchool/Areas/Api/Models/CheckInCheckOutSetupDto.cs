using System;

namespace AfterSchool.Areas.Api.Models
{
    public class CheckInCheckOutSetupDto
    {
        public bool CheckIn { get; set; }
        public string CheckInTime { get; set; }
        public string CheckInBy { get; set; } 
    }
}